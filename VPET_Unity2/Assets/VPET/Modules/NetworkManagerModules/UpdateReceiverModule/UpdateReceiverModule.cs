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
//! @date 15.10.2021

using System.Collections.Generic;
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
        
        private Thread m_consumerThread;
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
            m_messageQueue = new LinkedList<byte[]>();
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
            startUpdateReceiver(manager.settings.m_serverIP, "5557");

            m_core.syncEvent += runConsumeMessageOnce;

            ThreadStart consumer = new ThreadStart(consumeMessages);
            m_consumerThread = new Thread(consumer);
            m_consumerThread.Start();
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
                                case MessageType.PING:
                                    decodePingMessage();
                                    break;
                                case MessageType.SYNC:
                                    decodeSyncMessage();
                                    break;
                                case MessageType.PARAMETERUPDATE:
                                    // make shure that producer and consumer exclude eachother
                                    lock (m_messageQueue)
                                    {
                                        // [REVIEW] Queue length should be configurable globally
                                        // store only last 64 messages
                                        if (m_messageQueue.Count > 63)
                                            m_messageQueue.RemoveFirst();

                                        m_messageQueue.AddLast(input);
                                    }
                                    break;
                            }
                        }
                    }
                    Thread.Yield();
                    Thread.Sleep(1);
                }
                // [Review] Thread end causes exeptions!
                receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                receiver.Close();
                receiver.Dispose();
            }
        }
        
        private void decodePingMessage()
        { 
        }

        private void decodeSyncMessage()
        {
        }

        private void consumeMessages()
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneObjectID, ParameterID, ParameterType, ParameterData

            while (m_isRunning)
            {
                // lock the thread until global timer unlocks it
                m_mre.WaitOne();
                lock (m_messageQueue)
                { 
                    foreach (byte[] message in m_messageQueue)
                    {
                        //[REVIEW] time window for valid massages
                        if (m_core.time - message[1] < 2)
                        {
                            decodeMessage(message);
                        }
                    }
                    m_messageQueue.Clear();
                }
                Thread.Yield();
                //Thread.Sleep(1);
            }
        }

        private void runConsumeMessageOnce(object o, byte time)
        {
            m_mre.Set();
            m_mre.Reset();
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
