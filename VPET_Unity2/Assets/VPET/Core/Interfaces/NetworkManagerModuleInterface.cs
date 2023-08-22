/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "SceneManagerModule.cs"
//! @brief base implementation for scene manager modules
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 28.10.2021

using System;
using System.Threading;

namespace vpet
{
    //!
    //! class for scene manager modules
    //!
    public abstract class NetworkManagerModule : Module
    {
        //!
        //! Enumeration defining VPET message types.
        //!
        public enum MessageType
        {
            PARAMETERUPDATE, LOCK, // node
            SYNC, PING, RESENDUPDATE, // sync
            UNDOREDOADD, RESETOBJECT, // undo redo
            DATAHUB // DataHub
        }

        //!
        //! IP address of the network interface to be used.
        //!
        protected string m_ip;

        //!
        //! Port number to be used.
        //!
        protected string m_port;

        //!
        //! Flag specifing if the thread should stop running.
        //!
        protected bool m_isRunning;

        //!
        //! Action emitted when worker thread has been disposed
        //!
        protected Action m_disposed;

        //!
        //! The Thread used for receiving or sending messages.
        //!
        private Thread m_transeiverThread;
        
        //!
        //! Function, listening for messages and adds them to m_messageQueue (executed in separate thread).
        //!
        protected abstract void run();

        //!
        //! Reset event for stopping and resetting the run thread.
        //!
        protected ManualResetEvent m_mre; 

        //!
        //! Ret the manager of this module.
        //!
        public NetworkManager manager
        {
            get => (NetworkManager) m_manager;
        }

        //!
        //! constructor
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public NetworkManagerModule(string name, Manager manager) : base(name, manager) 
        {
            m_mre = new ManualResetEvent(false);

            manager.cleanupEvent += stopThread;
            m_disposed += this.manager.NetMQCleanup;
        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        public override void Dispose() 
        {
            base.Dispose();
            manager.cleanupEvent -= stopThread;
            //m_disposed -= manager.NetMQCleanup;
        }

        //!
        //! Function to stop all tranceiver threads (called when VPET core will be destroyed).
        //!
        private void stopThread(object sender, EventArgs e)
        {
            stop();
        }

        //!
        //! Function to start a new thread.
        //!
        //! @param ip IP address of the network interface.
        //! @param port Port number to be used.
        //!
        protected virtual void start(string ip, string port)
        {
            stop();

            m_ip = ip;
            m_port = port;

            ThreadStart transeiver = new ThreadStart(run);
            m_transeiverThread = new Thread(transeiver);
            m_transeiverThread.Start();
            NetworkManager.threadCount++;
        }

        //!
        //! Stop the transeiver.
        //!
        public void stop()
        {
            m_isRunning = false;
            m_mre.Set();
        }
    }
}
