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

//! @file "SceneDataHandler.cs"
//! @brief Implementation of the network manager and netMQ sender/receiver.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 20.05.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the network manager and netMQ sender/receiver.
    //!
    public class NetworkManager : Manager
    {
        //!
        //! Dictionary storing all registered network receivers and their global IDs.
        //!
        private Dictionary<int, NetworkReceiver> m_receiverDict;

        //!
        //! Next unique receiver ID.
        //!
        private static int m_receiverID;

        //!
        //! Dictionary storing all registered network senders and their global IDs.
        //!
        private Dictionary<int, NetworkSender> m_senderDict;

        //!
        //! Next unique sender ID.
        //!
        private static int m_senderID;

        //!
        //! Dictionary storing all registered network responders and their global IDs.
        //!
        private Dictionary<int, NetworkResponder> m_responderDict;

        //!
        //! Next unique responder ID.
        //!
        private static int m_responderID;

        //!
        //! Constructor initializing member variables.
        //!
        public NetworkManager(Type moduleType, CoreInterface vpetCore) : base(moduleType, vpetCore)
        {
            m_receiverDict = new Dictionary<int, NetworkReceiver>();
            m_receiverID = 0;
            m_senderDict = new Dictionary<int, NetworkSender>();
            m_senderID = 0;
        }

        //!
        //! Function to create and add new network receiver.
        //! @param ip IP address of the network interface the receiver shall use.
        //! @param port Port number the receiver shall use.
        //! @param receiveMessageQueue List of byte[] to be filled by the receiver.
        //! @return Global ID of the added receiver.
        //!
        public int addReceiver(string ip, string port, out List<byte[]> receiveMessageQueue)
        {
            NetworkReceiver receiver = new NetworkReceiver(out receiveMessageQueue);
            m_receiverDict.Add(m_receiverID++,receiver);
            return m_receiverID;
        }

        //!
        //! Function to start a new receiver thread.
        //! @param receiverID Global ID of the receiver to be started.
        //!
        public void startReceiver(int receiverID, string ip, string port)
        {
            m_receiverDict[receiverID].configure(ip, port);
            Thread receiverThread = new Thread(new ThreadStart(m_receiverDict[receiverID].run));
            receiverThread.Start();
        }

        //!
        //! Function to stop a receiver.
        //! @param receiverID Global ID of the receiver to be stopped.
        //!
        public void stopReceiver(int receiverID)
        {
            m_receiverDict[receiverID].stop();
        }

        //!
        //! Function to create and add new network sender.
        //! @param ip IP address of the network interface the sender shall use.
        //! @param port Port number the sender shall use.
        //! @param senderMessageQueue List of byte[] to be sent by the sender.
        //! @return Global ID of the added sender.
        //!
        public int addSender(out List<byte[]> senderMessageQueue)
        {
            NetworkSender sender = new NetworkSender(out senderMessageQueue);
            m_senderDict.Add(m_senderID++, sender);
            return m_senderID;
        }

        //!
        //! Function to start a new sender thread.
        //! @param receiverID Global ID of the sender to be started.
        //!
        public void startSender(int senderID,string ip, string port)
        {
            m_senderDict[senderID].configure(ip, port);
            Thread senderThread = new Thread(new ThreadStart(m_senderDict[senderID].run));
            senderThread.Start();
        }

        //!
        //! Function to stop a sender.
        //! @param receiverID Global ID of the sender to be stopped.
        //!
        public void stopSender(int senderID)
        {
            //TODO: send disconnect message
            m_senderDict[senderID].stop();
        }

        //!
        //! Function to create and add new network responder.
        //! @param ip IP address of the network interface the responder shall use.
        //! @param port Port number the responder shall use.
        //! @return Global ID of the added responder.
        //!
        public int addResponder(ref Dictionary<string, byte[]> responses)
        {
            NetworkResponder responder = new NetworkResponder(ref responses);
            m_responderDict.Add(m_senderID++, responder);
            return m_senderID;
        }

        //!
        //! Function to start a new  responder thread.
        //! @param receiverID Global ID of the  responder to be started.
        //!
        public void startResponder(int responderID, string ip, string port)
        {
            m_responderDict[responderID].configure(ip, port);
            Thread responderThread = new Thread(new ThreadStart(m_responderDict[responderID].run));
            responderThread.Start();
        }

        //!
        //! Function to stop a  responder.
        //! @param receiverID Global ID of the  responder to be stopped.
        //!
        public void stopResponder(int responderID)
        {
            //TODO: send disconnect message
            m_responderDict[responderID].stop();
        }

        //!
        //! Class implementing the network base class.
        //!
        private abstract class NetworkBase
        {
            //!
            //! IP address of the network interface to be used.
            //!
            protected string m_ip;

            //!
            //! Port number to be used.
            //!
            protected string m_port;

            //!
            //! List of byte[] storing the messages.
            //!
            protected List<byte[]> m_messageQueue;

            //!
            //! Flag specifing if the thread should stop running.
            //!
            protected bool m_isRunning;

            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be used.
            //!
            public NetworkBase(out List<byte[]> messageQueue)
            {
                messageQueue = m_messageQueue;
            }

            //!
            //! Function for setting the ip address and port.
            //! @param ip IP address of the network interface.
            //! @param port Port number to be used.
            //!
            public void configure(string ip, string port)
            {
                m_ip = ip;
                m_port = port;
            }

            //!
            //! Function, listening for messages and adds them to m_receiveMessageQueue (executed in separate thread).
            //!
            public abstract void run();

            //!
            //! Stop the receiver.
            //!
            public void stop()
            {
                m_isRunning = false;
            }
        }

        //!
        //! Class implementing the network receiver.
        //!
        private class NetworkReceiver : NetworkBase
        {
            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be received by the receiver.
            //!
            public NetworkReceiver(out List<byte[]> messageQueue) : base(out messageQueue) => base.m_messageQueue = messageQueue;
            //!
            //! Function, listening for messages and adds them to m_receiveMessageQueue (executed in separate thread).
            //!
            public override void run()
            {
                m_isRunning = true;
                AsyncIO.ForceDotNet.Force();
                using (var receiver = new SubscriberSocket())
                {
                    receiver.SubscribeToAnyTopic();

                    receiver.Connect("tcp://" + m_ip + ":" + m_port);

                    Helpers.Log("Receiver connected: " + "tcp://" + m_ip + ":" + m_port);
                    byte[] input;
                    while (m_isRunning)
                    {
                        if (receiver.TryReceiveFrameBytes(System.TimeSpan.FromSeconds(5), out input))
                        {
                            m_messageQueue.Add(input);
                        }
                    }
                    receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    receiver.Close();
                    receiver.Dispose();
                }
            }
        }

        //!
        //! Class implementing the network sender.
        //!
        private class NetworkSender : NetworkBase
        {
            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be sent by the sender.
            //!
            public NetworkSender(out List<byte[]> messageQueue) : base(out messageQueue) => base.m_messageQueue = messageQueue;

            //!
            //! Function, sending messages in m_sendMessageQueue (executed in separate thread).
            //!
            public override void run()
            {
                m_isRunning = true;
                AsyncIO.ForceDotNet.Force();
                using (var sender = new PublisherSocket())
                {
                    sender.Connect("tcp://" + m_ip + ":" + m_port);

                    Helpers.Log("Sender connected: " + "tcp://" + m_ip + ":" + m_port);
                    while (m_isRunning)
                    {
                        if (m_messageQueue.Count > 0)
                        {
                            try
                            {
                                sender.SendFrame(m_messageQueue[0], false); // true not wait
                                m_messageQueue.RemoveAt(0);
                            }
                            catch { }
                        }
                    }
                    sender.Disconnect("tcp://" + m_ip + ":" + m_port);
                    sender.Close();
                    sender.Dispose();
                }
            }
        }
        //!
        //! Class implementing the network responder.
        //!
        private class NetworkResponder : NetworkBase
        {
            private Dictionary<string, byte[]> m_responses;
            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be sent by the sender.
            //!
            public NetworkResponder(ref Dictionary<string, byte[]> responses, List<byte[]> messageQueue = null) : base(out messageQueue)
            {
                base.m_messageQueue = messageQueue;
                m_responses = responses;
            }

            //!
            //! Function, sending messages in m_sendMessageQueue (executed in separate thread).
            //!
            public override void run()
            {
                AsyncIO.ForceDotNet.Force();
                using (var dataSender = new ResponseSocket())
                {
                    dataSender.Bind("tcp://" + m_ip + ":" + m_port);
                    Debug.Log("Enter while.. ");

                    while (m_isRunning)
                    {
                        string message = "";
                        dataSender.TryReceiveFrameString(out message);
                        if (m_responses.ContainsKey(message))
                            dataSender.SendFrame(m_responses[message]);
                    }

                    // TODO: check first if closed
                    try
                    {
                        dataSender.Disconnect("tcp://" + m_ip + ":" + m_port);
                        dataSender.Dispose();
                        dataSender.Close();
                    }
                    finally
                    {
                        NetMQConfig.Cleanup(false);
                    }

                }
            }

        }
    }
}
