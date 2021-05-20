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
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the network manager and netMQ sender/receiver.
    //!
    public class NetworkManager : ManagerInterface
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
        //! Constructor initializing member variables.
        //!
        public NetworkManager()
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
            NetworkReceiver receiver = new NetworkReceiver(ip, port, out receiveMessageQueue);
            m_receiverDict.Add(m_receiverID++,receiver);
            return m_receiverID;
        }

        //!
        //! Function to start a new receiver thread.
        //! @param receiverID Global ID of the receiver to be started.
        //!
        public void startReceiver(int receiverID)
        {
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
        public int addSender(string ip, string port, out List<byte[]> senderMessageQueue)
        {
            NetworkReceiver sender = new NetworkReceiver(ip, port, out senderMessageQueue);
            m_receiverDict.Add(m_senderID++, sender);
            return m_senderID;
        }

        //!
        //! Function to start a new sender thread.
        //! @param receiverID Global ID of the sender to be started.
        //!
        public void startSender(int senderID)
        {
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
        //! Class implementing the network receiver.
        //!
        private class NetworkReceiver
        {
            //!
            //! IP address of the network interface the receiver uses.
            //!
            private string m_ip;

            //!
            //! Port number the receiver uses.
            //!
            private string m_port;

            //!
            //! List of byte[] storing the received messages.
            //!
            private List<byte[]> m_receiveMessageQueue;

            //!
            //! Flag specifing if the receiver should stop running.
            //!
            private bool m_isRunning;

            //!
            //! Constructor
            //! @param ip IP address of the network interface the receiver shall use.
            //! @param port Port number the receiver shall use.
            //! @param receiveMessageQueue List of byte[] to be filled by the receiver.
            //!
            public NetworkReceiver(string ip, string port, out List<byte[]> receiveMessageQueue)
            {
                m_ip = ip;
                m_port = port;
                receiveMessageQueue = m_receiveMessageQueue;
            }

            //!
            //! Function, listening for messages and adds them to m_receiveMessageQueue (executed in separate thread).
            //!
            public void run()
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
                            m_receiveMessageQueue.Add(input);
                        }
                    }
                    receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    receiver.Close();
                    receiver.Dispose();
                }
            }

            //!
            //! Stop the receiver.
            //!
            public void stop()
            {
                m_isRunning = false;
            }
        }

        //!
        //!
        //!
        private class NetworkSender
        {
            //!
            //! IP address of the network interface the sender uses.
            //!
            private string m_ip;

            //!
            //! Port number the sender uses.
            //!
            private string m_port;

            //!
            //! List of byte[] storing messages to be sent.
            //!
            private List<byte[]> m_sendMessageQueue;

            //!
            //! Flag specifing if the sender should stop running.
            //!
            private bool m_isRunning;

            //!
            //! Constructor
            //! @param ip IP address of the network interface the sender shall use.
            //! @param port Port number the sender shall use.
            //! @param sendMessageQueue List of byte[] to be sent by the sender.
            //!
            public NetworkSender(string ip, string port, out List<byte[]> sendMessageQueue)
            {
                m_ip = ip;
                m_port = port;
                sendMessageQueue = m_sendMessageQueue;
            }

            //!
            //! Function, sending messages in m_sendMessageQueue (executed in separate thread).
            //!
            public void run()
            {
                m_isRunning = true;
                AsyncIO.ForceDotNet.Force();
                using (var sender = new PublisherSocket())
                {
                    sender.Connect("tcp://" + m_ip + ":" + m_port);

                    Helpers.Log("Sender connected: " + "tcp://" + m_ip + ":" + m_port);
                    while (m_isRunning)
                    {
                        if (m_sendMessageQueue.Count > 0)
                        {
                            try
                            {
                                sender.SendFrame(m_sendMessageQueue[0], false); // true not wait
                                m_sendMessageQueue.RemoveAt(0);
                            }
                            catch { }
                        }
                    }
                    sender.Disconnect("tcp://" + m_ip + ":" + m_port);
                    sender.Close();
                    sender.Dispose();
                }
            }

            //!
            //! Stop the sender.
            //!
            public void stop()
            {
                m_isRunning = false;
            }
        }
    }
}
