﻿/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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
            UNDOREDOADD, RESETOBJECT // undo redo
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

        protected static ManualResetEvent m_mre = new ManualResetEvent(false);

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
