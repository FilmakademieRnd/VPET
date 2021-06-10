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
        //! Dictionary storing all registered network subscribers and their global IDs.
        //!
        private NetworkSubscriber m_subscriber;

        //!
        //! Dictionary storing all registered network requesters and their global IDs.
        //!
        private NetworkRequester m_requester;

        //!
        //! Dictionary storing all registered network publishers and their global IDs.
        //!
        private NetworkPublisher m_publisher;

        //!
        //! Dictionary storing all registered network responders and their global IDs.
        //!
        private NetworkResponder m_responder;

        //!
        //! Constructor initializing member variables.
        //!
        public NetworkManager(Type moduleType, CoreInterface vpetCore) : base(moduleType, vpetCore)
        {
        }

        //!
        //! Function to start a new subscriber thread.
        //! @param ip IP address of the network interface the subscriber shall use.
        //! @param port Port number the subscriber shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the subscriber.
        //!
        public void startSubscriber(string ip, string port, out List<byte[]> subscriberMessageQueue)
        {
            if (m_subscriber != null)
                m_subscriber.stop();

            m_subscriber = new NetworkSubscriber(out subscriberMessageQueue);
            m_subscriber.configure(ip, port);
            Thread subscriberThread = new Thread(new ThreadStart(m_subscriber.run));
            subscriberThread.Start();
        }

        //!
        //! Function to stop a subscriber.
        //! @param receiverID Global ID of the subscriber to be stopped.
        //!
        public void stopSubscriber(int receiverID)
        {
            m_subscriber.stop();
        }

        //!
        //! Function to start a new requester thread.
        //! @param ip IP address of the network interface the requester shall use.
        //! @param port Port number the requester shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the requester.
        //!
        public void startRequester(string ip, string port, out List<byte[]> requesterMessageQueue, ref List<string> requests)
        {
            if (m_requester != null)
                m_requester.stop();

            m_requester = new NetworkRequester(out requesterMessageQueue, ref requests);
            m_requester.configure(ip, port);
            Thread requesterThread = new Thread(new ThreadStart(m_requester.run));
            requesterThread.Start();
        }

        //!
        //! Function to stop a requester.
        //! @param receiverID Global ID of the requester to be stopped.
        //!
        public void stopRequester(int requesterID)
        {
            m_requester.stop();
        }

        public bool requesterDone()
        {
            return m_requester.done;
        }

        //!
        //! Function to start a new publisher thread.
        //! @param ip IP address of the network interface the publisher shall use.
        //! @param port Port number the publisher shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the publisher.
        //!
        public void startPublisher(string ip, string port, out List<byte[]> publisherMessageQueue)
        {
            if (m_publisher != null)
                m_publisher.stop();

            m_publisher = new NetworkPublisher(out publisherMessageQueue);
            m_publisher.configure(ip, port);
            Thread publisherThread = new Thread(new ThreadStart(m_publisher.run));
            publisherThread.Start();
        }

        //!
        //! Function to stop a publisher.
        //! @param receiverID Global ID of the publisher to be stopped.
        //!
        public void stopPublisher(int senderID)
        {
            //TODO: send disconnect message
            m_publisher.stop();
        }

        //!
        //! Function to start a new responder thread.
        //! @param ip IP address of the network interface the responder shall use.
        //! @param port Port number the responder shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the responder.
        //!
        public void startResponder(string ip, string port, ref Dictionary<string, byte[]> responses)
        {
            if (m_responder != null)
                m_responder.stop();

            // never used message queue
            List<byte[]> responderMessageQueue;
            
            m_responder = new NetworkResponder(ref responses, out responderMessageQueue);
            m_responder.configure(ip, port);
            Thread responderThread = new Thread(new ThreadStart(m_responder.run));
            responderThread.Start();
        }


        //!
        //! Function to stop a  responder.
        //! @param receiverID Global ID of the  responder to be stopped.
        //!
        public void stopResponder()
        {
            //TODO: send disconnect message
            m_responder.stop();
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
        private class NetworkSubscriber : NetworkBase
        {
            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be received by the receiver.
            //!
            public NetworkSubscriber(out List<byte[]> messageQueue) : base(out messageQueue) => base.m_messageQueue = messageQueue;
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
        //! Class implementing the network receiver.
        //!
        private class NetworkRequester : NetworkBase
        {
            private bool m_done;
            public bool done { get => m_done; }
            private List<string> m_requests;

            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be received by the receiver.
            //!
            public NetworkRequester(out List<byte[]> messageQueue, ref List<string> requests) : base(out messageQueue)
            { 
                base.m_messageQueue = messageQueue;

                m_done = false;
                m_requests = requests;
            }
            //!
            //! Function, listening for messages and adds them to m_receiveMessageQueue (executed in separate thread).
            //!
            public override void run()
            {
                AsyncIO.ForceDotNet.Force();
                using (var sceneReceiver = new RequestSocket())
                {
                    sceneReceiver.Connect("tcp://" + m_ip + ":" + m_port);

                    foreach (string request in m_requests)
                    {
                        sceneReceiver.SendFrame(request);
                        m_messageQueue.Add(sceneReceiver.ReceiveFrameBytes());
                    }
                    m_done = true;
                    sceneReceiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    sceneReceiver.Close();
                    sceneReceiver.Dispose();
                }
            }
        }

        //!
        //! Class implementing the network sender.
        //!
        private class NetworkPublisher : NetworkBase
        {
            //!
            //! Constructor
            //! @param messageQueue List of byte[] to be sent by the sender.
            //!
            public NetworkPublisher(out List<byte[]> messageQueue) : base(out messageQueue) => base.m_messageQueue = messageQueue;

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
            public NetworkResponder(ref Dictionary<string, byte[]> responses, out List<byte[]> messageQueue) : base(out messageQueue)
            {
                base.m_messageQueue = messageQueue;
                m_responses = responses;
            }

            //!
            //! Function, sending messages in m_sendMessageQueue (executed in separate thread).
            //!
            public override void run()
            {
                m_isRunning = true;
                AsyncIO.ForceDotNet.Force();
                using (var dataSender = new ResponseSocket())
                {
                    dataSender.Bind("tcp://" + m_ip + ":" + m_port);
                    Debug.Log("Enter while.. ");

                    while (m_isRunning)
                    {
                        string message = "";
                        dataSender.TryReceiveFrameString(out message);       // TryReceiveFrameString returns null if no message has been received!
                        if (message != null)
                        {
                            if (m_responses.ContainsKey(message))
                                dataSender.SendFrame(m_responses[message]);
                            else
                                dataSender.SendFrame(new byte[0]);
                        }
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
