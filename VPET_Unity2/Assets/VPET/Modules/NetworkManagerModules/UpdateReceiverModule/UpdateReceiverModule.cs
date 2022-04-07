/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "UpdateReceiverModule.cs"
//! @brief Implementation of the update receiver module, listening to parameter updates from clients
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 28.10.2021

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the scene sender module, listening to scene requests and sending scene data.
    //!
    public class UpdateReceiverModule : NetworkManagerModule
    {
        //!
        //! Buffer for storing incoming messages by time.
        //!
        private List<List<byte[]>> m_messageBuffer;

        //!
        //! Event emitted when parameter change should be added to undo/redo history
        //!
        public event EventHandler<AbstractParameter> receivedHistoryUpdate;

        //!
        //! A referece to VPET's scene manager.
        //!
        private SceneManager m_sceneManager;

        private int m_timesteps;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public UpdateReceiverModule(string name, Manager manager) : base(name, manager)
        {
        }

        //!
        //! Cleaning up event registrations. 
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);
            core.timeEvent -= consumeMessages;
            m_sceneManager.sceneReady -= connectAndStart;
        }

        //!
        //! Function for custom initialisation.
        //! 
        //! @param sender The VPET core.
        //! @param e The pssed event arguments.
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            m_timesteps = ((int)(256f / core.settings.framerate)) * core.settings.framerate;

            // initialize message buffer
            m_messageBuffer = new List<List<byte[]>>(m_timesteps);
            for (int i = 0; i < m_timesteps; i++)
                m_messageBuffer.Add(new List<byte[]>(256));

            m_sceneManager = core.getManager<SceneManager>();
            m_sceneManager.sceneReady += connectAndStart;
        }

        //!
        //! Function that connects the scene object change events for parameter queuing.
        //!
        //! @param sender The emitting scene object.
        //! @param e The pssed event arguments.
        //!
        private void connectAndStart(object sender, EventArgs e)
        {
            startUpdateReceiver(manager.settings.ipAddress.value, "5556");

            core.timeEvent += consumeMessages;
        }

        //!
        //! Function, waiting for incoming messages (executed in separate thread).
        //! Control messages are executed immediately, parameter update messages are buffered
        //! and executed later to obtain synchronicity.
        //!
        protected override void run()
        {
            m_isRunning = true;
            AsyncIO.ForceDotNet.Force();
            var receiver = new SubscriberSocket();
            receiver.SubscribeToAnyTopic();
            receiver.Connect("tcp://" + m_ip + ":" + m_port);

            Helpers.Log("Update receiver connected: " + "tcp://" + m_ip + ":" + m_port);
            byte[] input = null;
            while (m_isRunning)
            {
                if (receiver.TryReceiveFrameBytes(System.TimeSpan.FromSeconds(1), out input))
                {
                    if (input[0] != manager.cID)
                    {
                        switch ((MessageType)input[2])
                        {
                            case MessageType.LOCK:
                                decodeLockMessage(ref input);
                                break;
                            case MessageType.SYNC:
                                if (!core.isServer)
                                    decodeSyncMessage(ref input);
                                break;
                            case MessageType.UNDOREDOADD:
                                    decodeUndoRedoMessage(ref input);
                                break;
                            case MessageType.RESETOBJECT:
                                decodeResetMessage(ref input);
                                break;
                            case MessageType.PARAMETERUPDATE:
                                // make shure that producer and consumer exclude eachother
                                lock (m_messageBuffer)
                                {
                                    // input[1] is time
                                    m_messageBuffer[input[1]].Add(input);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                Thread.Yield();
            }
            try
            {
                receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                receiver.Close();
                receiver.Dispose();
                Helpers.Log(this.name + " disposed.");
                Thread.Sleep(500);
            }
            catch
            {
            }
        }

        //! 
        //! Function that decodes a sync message and set the clients global time.
        //!
        //! @param message The message to be decoded.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeSyncMessage(ref byte[] message)
        {
            core.time = message[1];
        }

        //! 
        //! Function that decodes a undo redo message and adds it to the undo redo manager.
        //!
        //! @param message The message to be decoded.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeUndoRedoMessage(ref byte[] message)
        {
            short sceneObjectID = BitConverter.ToInt16(message, 3);
            short parameterID = BitConverter.ToInt16(message, 5);

            SceneObject sceneObject = m_sceneManager.getSceneObject(sceneObjectID);

            receivedHistoryUpdate?.Invoke(this, sceneObject.parameterList[parameterID]);
        }

        //! 
        //! Function that decodes a undo redo message and adds it to the undo redo manager.
        //!
        //! @param message The message to be decoded.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeResetMessage(ref byte[] message)
        {
            short sceneObjectID = BitConverter.ToInt16(message, 3);
            SceneObject sceneObject = m_sceneManager.getSceneObject(sceneObjectID);
            foreach (AbstractParameter p in sceneObject.parameterList)
                p.reset();
            core.getManager<SceneManager>().getModule<UndoRedoModule>().vanishHistory(sceneObject);
        }

        //! 
        //! Function that decodes a lock message and lock or unlock the corresponding scene object.
        //!
        //! @param message The message to be decoded.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeLockMessage(ref byte[] message)
        {
            bool lockState = BitConverter.ToBoolean(message, 5);
            short sceneObjectID = BitConverter.ToInt16(message, 3);

            SceneObject sceneObject = m_sceneManager.getSceneObject(sceneObjectID);
            sceneObject._lock = lockState;
        }

        //!
        //! Function that triggers the parameter updates (called once a global time tick).
        //!
        private void consumeMessages(object o, EventArgs e)
        {
            // define the buffer size by defining the time offset in the ringbuffer
            // % time steps to take ring (0 to m_timesteps) into account
            // set to 1/10 second
            int bufferTime = (((core.time - core.settings.framerate/10) + m_timesteps) % m_timesteps);

            lock (m_messageBuffer)
            {
                foreach (Byte[] message in m_messageBuffer[bufferTime])
                    decodeMessage(message);

                m_messageBuffer[bufferTime].Clear();
            }
        }

        //!
        //! Function to decode a parameter message and update the corresponding parameter. 
        //!
        //! @param message The message to be decoded.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeMessage(byte[] message)
        {
            short sceneObjectID = BitConverter.ToInt16(message, 3);
            short parameterID = BitConverter.ToInt16(message, 5);

            SceneObject sceneObject = m_sceneManager.getSceneObject(sceneObjectID);

            if (sceneObject != null)
                sceneObject.parameterList[parameterID].deSerialize(ref message, 8);
        }


        //!
        //! Function to start the scene sender module.
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        void startUpdateReceiver(string ip, string port)
        {
            start(ip, port);
        }
    }
}
