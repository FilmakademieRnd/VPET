/*
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

//! @file "NetworkManager.cs"
//! @brief Implementation of the network manager and netMQ sender/receiver.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 13.10.2021

using System;
using System.Net;
using NetMQ;

namespace vpet
{
    //!
    //! Class implementing the network manager and netMQ sender/receiver.
    //!
    public class NetworkManager : Manager
    {
        [Serializable]
        public class NetworkManagerSettings : Settings
        {
            // to store a parameters value into the settings files.
            public Parameter<string> ipAddress;
        }

        //!
        //! number of threads
        //!
        public static int threadCount = 0;

        //!
        //! number of disposed network worker threads
        //!
        private int m_disposeCount = 0;

        //!
        //! Cast for accessing the settings variable with the correct type.
        //!
        public NetworkManagerSettings settings { get => (NetworkManagerSettings)_settings; }
        //!
        //! Constructor initializing member variables.
        //!
        //! @param  moduleType  type of modules to be loaded by this manager
        //! @param vpetCore A reference to the VPET core.
        //!
        public NetworkManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            settings.ipAddress = new Parameter<string>("127.0.0.1", "ipAddress");

            //reads the network name of the device
            var hostName = Dns.GetHostName();
            var host = Dns.GetHostEntry(hostName);

            //Take last ip adress of local network (which is local wlan ip address)
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (core.isServer)
                    {
                        // prevent equal cIDs if server and client running on the same machine
                        m_cID = 0;
                    }
                    else
                        m_cID = byte.Parse(ip.ToString().Split('.')[3]);
                    m_ip = ip.ToString();
                }
            }
        }

        protected override void Cleanup(object sender, EventArgs e) 
        {
            base.Cleanup(sender, e);
        }

        //!
        //! Clean up the NetMQ COntext
        //!
        public void NetMQCleanup()
        {
            m_disposeCount++;
            if (m_disposeCount == threadCount)
            {
                try
                {
                    NetMQConfig.Cleanup(false);
                }
                finally
                {
                    Helpers.Log("netMQ cleaned up.");
                }
            }
        }


        //!
        //! The ID if the client (based on the last digit of IP address)
        //!
        private byte m_cID;
        public byte cID
        {
            get => m_cID;
        }

        //!
        //! The local IP address.
        //!
        private string m_ip;
        public string ip
        {
            get => m_ip;
        }
    }
}
