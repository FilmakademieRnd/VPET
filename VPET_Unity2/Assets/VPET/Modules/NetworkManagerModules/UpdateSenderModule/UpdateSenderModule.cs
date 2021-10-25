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

//! @file "UpdateSenderModule.cs"
//! @brief Implementation of the update sender module, sending parameter updates to clients.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 15.10.2021

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the update sender module, sending parameter updates to clients.
    //!
    public class UpdateSenderModule : NetworkManagerModule
    {
        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public UpdateSenderModule(string name, Core core) : base(name, core)
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

            SceneManager sceneManager = m_core.getManager<SceneManager>();
            sceneManager.sceneReady += connectAndStart;
        }

        //!
        //! Function that connects the scene object change events for parameter queuing.
        //!
        //! @param sender The emitting scene object.
        //! @param e The pssed event arguments.
        //!
        private void connectAndStart(object sender, EventArgs e)
        {
            foreach (SceneObject sceneObject in ((SceneManager) sender).sceneObjects)
            {
                sceneObject.hasChanged += queueParameterMessage;
            }

            // [REVIEW] port should be in global config
            startUpdateSender(manager.settings.m_serverIP, "5557");
        }

        //!
        //! Function that creates a parameter update message and adds it to the message queue for sending.
        //!
        //! @param sender The emitting scene object.
        //! @param e The pssed event arguments.
        //!
        private void queueParameterMessage(object sender, AbstractParameter abstractParameter)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneObjectID, ParameterID, ParameterType, ParameterData
            
            byte[] message = abstractParameter.Serialize(8); // ParameterData;

            // header
            message[0] = manager.cID;
            message[1] = m_core.time;
            message[2] = (byte) MessageType.PARAMETERUPDATE;

            // parameter
            Buffer.BlockCopy(BitConverter.GetBytes( ((SceneObject)sender).id), 0, message, 3, 2);  // SceneObjectID
            Buffer.BlockCopy(BitConverter.GetBytes(abstractParameter.id), 0, message, 5, 2);  // ParameterID
            message[7] = (byte)abstractParameter.vpetType;  // ParameterType

            m_messageQueue.AddLast(message);
        }

        //!
        //! Function that creates a ping message and adds it to the message queue for sending.
        //!
        private void queuePingMessage()
        {
            byte[] message = new byte[3];

            // header
            message[0] = manager.cID;
            message[1] = m_core.time;
            message[2] = (byte)MessageType.PING;
        }

        //!
        //! Function that creates a sync message and adds it to the message queue for sending.
        //!
        private void queueSyncMessage()
        {
            byte[] message = new byte[3];

            // header
            message[0] = manager.cID;
            message[1] = m_core.time;
            message[2] = (byte)MessageType.SYNC;
        }

        //!
        //! Function, sending messages in m_messageQueue (executed in separate thread).
        //!
        protected override void run()
        {
            m_isRunning = true;
            AsyncIO.ForceDotNet.Force();
            using (var sender = new PublisherSocket())
            {
                sender.Connect("tcp://" + m_ip + ":" + m_port);

                Helpers.Log("Sender connected: " + "tcp://" + m_ip + ":" + m_port);
                while (m_isRunning)
                {
                    lock (m_messageQueue)
                    {
                        if (m_messageQueue.Count > 0)
                        {
                            try
                            {
                                sender.SendFrame(m_messageQueue.First.Value, false); // true not wait
                                m_messageQueue.RemoveFirst();
                            }
                            catch { }
                        }
                    }
                    Thread.Yield();
                    Thread.Sleep(1);
                }
                sender.Disconnect("tcp://" + m_ip + ":" + m_port);
                sender.Close();
                sender.Dispose();
            }
        }

        //!
        //! Function to start the scene sender module.
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        public void startUpdateSender(string ip, string port)
        {
            start(ip, port);
        }
    }
}
