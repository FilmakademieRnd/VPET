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
            m_messageQueue = new List<byte[]>();
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
                byte[] input;
                while (m_isRunning)
                {
                    if (receiver.TryReceiveFrameBytes(System.TimeSpan.FromSeconds(5), out input))
                    {
                        m_messageQueue.Add(input);
                    }
                    Thread.Yield();
                    Thread.Sleep(1);
                }
                receiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                receiver.Close();
                receiver.Dispose();
            }
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
