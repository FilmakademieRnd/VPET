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
    //! Class implementing the update sender module, sending parameter updates to clients.
    //!
    public class UpdateSenderModule : NetworkManagerModule
    {
        //!
        //! Constructor
        //!
        //! @param messageQueue List of byte[] to be received by the receiver.
        //!
        public UpdateSenderModule(string name, Core core) : base(name, core)
        {
        }

        //!
        //! Function for custom initialisation.
        //! 
        public override void initialise(out List<byte[]> messageQueue)
        {
            base.initialise(out messageQueue);
            messageQueue = m_messageQueue;
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
