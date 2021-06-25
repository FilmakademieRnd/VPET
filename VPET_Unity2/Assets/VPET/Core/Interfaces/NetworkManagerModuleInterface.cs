//! @file "SceneManagerModule.cs"
//! @brief base implementation for scene manager modules
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.06.2021

using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace vpet
{
    //!
    //! class for scene manager modules
    //!
    public abstract class NetworkManagerModule : Module
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
        //! Function, listening for messages and adds them to m_messageQueue (executed in separate thread).
        //!
        protected abstract void run();

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

            ThreadStart starter = new ThreadStart(run);

            Thread requesterThread = new Thread(starter);
            requesterThread.Start();

        }

        //!
        //! Stop the receiver.
        //!
        public void stop()
        {
            m_isRunning = false;
        }
        //!
        //! Ret the manager of this module.
        //!
        public NetworkManager manager
        {
            get => (NetworkManager) m_core.getManager<NetworkManager>();
        }
        //!
        //! constructor
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public NetworkManagerModule(string name, Core core) : base(name, core)
        {
        }
        //!
        //! Function for custom initialisation.
        //! 
        //! Reference to tist of byte[] storing the messages.
        //!
        public virtual void initialise(out List<byte[]> messageQueue)
        {
            messageQueue = m_messageQueue;
        }
    }
}
