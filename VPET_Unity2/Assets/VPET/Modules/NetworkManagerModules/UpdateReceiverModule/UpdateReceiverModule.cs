/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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
        private SceneManager m_sceneManager;
        
        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public UpdateReceiverModule(string name, Core core) : base(name, core)
        {
        }

        //!
        //! Function for custom initialisation.
        //! 
        //! @param sender The VPET core.
        //! @param e The pssed event arguments.
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            // initialize message buffer
            m_messageBuffer = new List<List<byte[]>>(m_core.m_timesteps);
            for (int i = 0; i < m_core.m_timesteps; i++)
                m_messageBuffer.Add(new List<byte[]>(16));

            m_sceneManager = m_core.getManager<SceneManager>();
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
            // [REVIEW] port should be in global config
            startUpdateReceiver(manager.settings.m_serverIP, "5556");

            m_core.timeEvent += consumeMessages;
        }

        //!
        //! Function, listening for messages and adds them to m_messageQueue (executed in separate thread).
        //!
        protected override void run()
        {
            m_isRunning = true;
            AsyncIO.ForceDotNet.Force();
            using (var receiver = new SubscriberSocket())
            {
                receiver.SubscribeToAnyTopic();
                receiver.Connect("tcp://" + m_ip + ":" + m_port);

                Helpers.Log("Receiver connected: " + "tcp://" + m_ip + ":" + m_port);
                byte[] input = null;
                while (m_isRunning)
                {
                    if (receiver.TryReceiveFrameBytes(System.TimeSpan.FromSeconds(5), out input))
                    {
                        if (input[0] != manager.cID)
                        {
                            switch ((MessageType)input[2])
                            {
                                case MessageType.LOCK:
                                        decodeLockMessage(ref input);
                                    break;
                                case MessageType.SYNC:
                                    if (!m_core.settings.isServer)
                                        decodeSyncMessage(ref input);
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
                    Thread.Sleep(1);
                }
                try
                {
                    // [Review] Thread end causes exeptions!
                    receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    receiver.Close();
                    receiver.Dispose();
                }
                finally
                {
                    NetMQConfig.Cleanup(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeSyncMessage(ref byte[] message)
        {
            m_core.time = message[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void decodeLockMessage(ref byte[] message)
        {
            bool lockState = BitConverter.ToBoolean(message, 2);
            short sceneObjectID = BitConverter.ToInt16(message, 3);

            SceneObject sceneObject = m_sceneManager.getSceneObject(sceneObjectID);
            sceneObject._lock = lockState;
        }

        private void consumeMessages(object o, EventArgs e)
        {
            // define the buffer size by defining the time offset in the ringbuffer
            // % time steps to take ring (0 to m_timesteps) into account
            // set to 1/4 second
            int bufferTime = (((m_core.time - m_core.settings.framerate/4) + m_core.m_timesteps) % m_core.m_timesteps);

            foreach (Byte[] message in m_messageBuffer[bufferTime])
                decodeMessage(message);

            m_messageBuffer[bufferTime].Clear();
        }

        private void decodeMessage(byte[] message)
        {
            short sceneObjectID = BitConverter.ToInt16(message, 3);
            short parameterID = BitConverter.ToInt16(message, 5);
            //AbstractParameter.ParameterType type = (AbstractParameter.ParameterType)message[7];

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
