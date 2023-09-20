/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
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

//! @file "NetworkManager.cs"
//! @brief Implementation of the network manager and netMQ sender/receiver.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 13.10.2021

using System;
using System.Diagnostics;
using System.Net;
using NetMQ;

namespace tracer
{
    //!
    //! Class implementing the network manager and netMQ sender/receiver.
    //!
    public class NetworkManager : Manager
    {
        [Serializable]
        public class NetworkManagerSettings : Settings
        {
            [ShowInMenu]
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
        //! Event that is invoket when a client has left the network session.
        //!
        public event EventHandler<byte> clientLost;
        
        //!
        //! Event that is invoket when a new client enters the network session.
        //!
        public event EventHandler<byte> clientRegistered;

        //!
        //! Event that is invoket when a new scene object has been added.
        //!
        public event EventHandler<SceneObject> sceneObjectAdded;

        //!
        //! Event that is invoket when a new scene object has been removed.
        //!
        public event EventHandler<SceneObject> sceneObjectRemoved;

        //!
        //! Cast for accessing the settings variable with the correct type.
        //!
        public NetworkManagerSettings settings { get => (NetworkManagerSettings)_settings; }
        
        //!
        //! Constructor initializing member variables.
        //!
        //! @param  moduleType  type of modules to be loaded by this manager
        //! @param vpetCore A reference to the TRACER core.
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
                        m_cID = 254;
                    }
                    else
                        m_cID = byte.Parse(ip.ToString().Split('.')[3]);
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
                catch { }
                finally
                {
                    Helpers.Log("netMQ cleaned up.");
                }
            }
        }

        //!
        //! Function to add a scene object to the network sync.
        //!
        //! @param sceneObject The scene object to be added to the network sync.
        //!
        public void AddSceneObject(SceneObject sceneObject)
        {
            sceneObjectAdded?.Invoke(this, sceneObject);
        }

        //!
        //! Function to remove a scene object to the network sync.
        //!
        //! @param sceneObject The scene object to be removed from the network sync.
        //!
        public void RemoveSceneObject(SceneObject sceneObject)
        {
            sceneObjectRemoved?.Invoke(this, sceneObject);
        }

        //!
        //! The ID if the client (based on the last digit of IP address)
        //!
        private byte m_cID = 254;
        public byte cID
        {
            get => m_cID;
        }

        //!
        //! Function to invoke client connection status updates.
        //!
        //! @param connectionStatus Wether a client has been connected or disconnected.
        //! @param clientID The ID of the client that has been connected or disconnected.
        //!
        public void clientConnectionUpdate(bool connectionStatus, byte clientID)
        {
            if (connectionStatus)
                clientRegistered?.Invoke(this, clientID);
            else
                clientLost?.Invoke(this, clientID);
        }
    }
}
