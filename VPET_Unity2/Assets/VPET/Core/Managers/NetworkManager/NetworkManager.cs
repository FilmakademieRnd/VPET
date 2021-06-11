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

//! @file "NetworkManager.cs"
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

        public ref NetworkRequester requester
        {
            get { return ref m_requester; }
        }

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
        //!
        //! @param ip IP address of the network interface the subscriber shall use.
        //! @param port Port number the subscriber shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the subscriber.
        //!
        public void startSubscriber(string ip, string port, out List<byte[]> subscriberMessageQueue)
        {
            if (m_subscriber != null)
                m_subscriber.stop();

            m_subscriber = new NetworkSubscriber(out subscriberMessageQueue, this);
            m_subscriber.configure(ip, port);
            Thread subscriberThread = new Thread(new ThreadStart(m_subscriber.run));
            subscriberThread.Start();
        }

        //!
        //! Function to stop a subscriber.
        //!
        //! @param receiverID Global ID of the subscriber to be stopped.
        //!
        public void stopSubscriber(int receiverID)
        {
            m_subscriber.stop();
        }

        //!
        //! Function to start a new requester thread.
        //!
        //! @param ip IP address of the network interface the requester shall use.
        //! @param port Port number the requester shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the requester.
        //!
        public void startRequester(string ip, string port, out List<byte[]> requesterMessageQueue, ref List<string> requests)
        {
            if (m_requester != null)
                m_requester.stop();

            m_requester = new NetworkRequester(out requesterMessageQueue, ref requests, this);
            m_requester.configure(ip, port);
            Thread requesterThread = new Thread(new ThreadStart(m_requester.run));
            requesterThread.Start();
        }

        //!
        //! Function to stop a requester.
        //!
        //! @param receiverID Global ID of the requester to be stopped.
        //!
        public void stopRequester(int requesterID)
        {
            m_requester.stop();
        }

        //!
        //! Function to start a new publisher thread.
        //!
        //! @param ip IP address of the network interface the publisher shall use.
        //! @param port Port number the publisher shall use.
        //! @param subscriberMessageQueue List of byte[] to be filled by the publisher.
        //!
        public void startPublisher(string ip, string port, out List<byte[]> publisherMessageQueue)
        {
            if (m_publisher != null)
                m_publisher.stop();

            m_publisher = new NetworkPublisher(out publisherMessageQueue, this);
            m_publisher.configure(ip, port);
            Thread publisherThread = new Thread(new ThreadStart(m_publisher.run));
            publisherThread.Start();
        }

        //!
        //! Function to stop a publisher.
        //!
        //! @param receiverID Global ID of the publisher to be stopped.
        //!
        public void stopPublisher(int senderID)
        {
            //TODO: send disconnect message
            m_publisher.stop();
        }

        //!
        //! Function to start a new responder thread.
        //!
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
            
            m_responder = new NetworkResponder(ref responses, out responderMessageQueue, this);
            m_responder.configure(ip, port);
            Thread responderThread = new Thread(new ThreadStart(m_responder.run));
            responderThread.Start();
        }


        //!
        //! Function to stop a  responder.
        //!
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
        public abstract class NetworkBase
        {
            protected NetworkManager m_networkManager;
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
            public NetworkBase(out List<byte[]> messageQueue, NetworkManager networkManager)
            {
                messageQueue = m_messageQueue;
                m_networkManager = networkManager;
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
            //! Function, listening for messages and adds them to m_messageQueue (executed in separate thread).
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
        //! Class implementing the network subscriber.
        //!
        private class NetworkSubscriber : NetworkBase
        {
            //!
            //! Constructor
            //!
            //! @param messageQueue List of byte[] to be received by the receiver.
            //!
            public NetworkSubscriber(out List<byte[]> messageQueue, NetworkManager networkManager) : base(out messageQueue, networkManager)
            {
                base.m_messageQueue = messageQueue;
                base.m_networkManager = networkManager;
            }
            //!
            //! Function, listening for messages and adds them to m_messageQueue (executed in separate thread).
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
        public class NetworkRequester : NetworkBase
        {
            //!
            //! The list of request the reqester uses to request the packages.
            //!
            private List<string> m_requests;
            //!
            //! The event that is triggerd, when the scene has been received.
            //!
            public event EventHandler m_sceneReceived;

            //!
            //! Constructor
            //!
            //! @param messageQueue List of byte[] to be received by the receiver.
            //! @param requests The string list containing the types of packages to be requested.
            //! @param networkManager The network manager as parent of the class.
            //!
            public NetworkRequester(out List<byte[]> messageQueue, ref List<string> requests, NetworkManager networkManager) : base(out messageQueue, networkManager)
            { 
                base.m_messageQueue = messageQueue;
                base.m_networkManager = networkManager;

                m_requests = requests;
            }
            //!
            //! Function, requesting scene packages and receiving package data (executed in separate thread).
            //! As soon as all requested packages are received, a signal is emited that triggers the scene cration.
            //!
            public override void run()
            {
                AsyncIO.ForceDotNet.Force();
                using (var sceneReceiver = new RequestSocket())
                {
                    SceneManager sceneManager = (SceneManager)m_networkManager.core.getManager(typeof(SceneManager));
                    
                    sceneReceiver.Connect("tcp://" + m_ip + ":" + m_port);

                    SceneManager.SceneDataHandler sceneDataHandler = sceneManager.sceneDataHandler;
                    foreach (string request in m_requests)
                    {
                        sceneReceiver.SendFrame(request);
                        switch (request)
                        {
                            case "header":
                                sceneDataHandler.headerByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                            case "nodes":
                                sceneDataHandler.nodesByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                            case "objects":
                                sceneDataHandler.objectsByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                            case "characters":
                                sceneDataHandler.characterByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                            case "textures":
                                sceneDataHandler.texturesByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                            case "materials":
                                sceneDataHandler.materialsByteData = sceneReceiver.ReceiveFrameBytes();
                                break;
                        }
                        
                        //sceneDataHandler.setByteData(request, sceneReceiver.ReceiveFrameBytes());
                    }

                    // emit sceneReceived signal to trigger scene cration in the sceneCreator module
                    m_sceneReceived?.Invoke(this, new EventArgs());

                    sceneReceiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    sceneReceiver.Close();
                    sceneReceiver.Dispose();
                }
            }
        }

        //!
        //! Class implementing the network publisher.
        //!
        private class NetworkPublisher : NetworkBase
        {
            //!
            //! Constructor
            //!
            //! @param messageQueue List of byte[] to be sent by the sender.
            //! @param networkManager The network manager as parent of the class.
            //!
            public NetworkPublisher(out List<byte[]> messageQueue, NetworkManager networkManager) : base(out messageQueue, networkManager)
            {
                base.m_messageQueue = messageQueue;
                base.m_networkManager = networkManager;
            }

            //!
            //! Function, sending messages in m_messageQueue (executed in separate thread).
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
            //!
            //! @param responses A dictionary containing the message type and the corresponding byte data.
            //! @param messageQueue List of byte[] to be received by the receiver.
            //! @param networkManager The network manager as parent of the class.
            //!
            public NetworkResponder(ref Dictionary<string, byte[]> responses, out List<byte[]> messageQueue, NetworkManager networkManager) : base(out messageQueue, networkManager)
            {
                base.m_messageQueue = messageQueue;
                base.m_networkManager = networkManager;
                m_responses = responses;
            }

            //!
            //! Function, sending messages containing the scene data as reponces to the requested package (executed in separate thread).
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
