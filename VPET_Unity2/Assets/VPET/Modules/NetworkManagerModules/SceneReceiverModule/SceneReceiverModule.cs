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

//! @file "SceneReceiverModule.cs"
//! @brief Implementation of the scene receiver module, sending scene requests and receives scene data. 
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 10.06.2021

using System.Collections;
using System.Collections.Generic;
using System;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! The scene receiver module, sending scene requests and receives scene data.
    //!
    public class SceneReceiverModule : NetworkManagerModule
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
        //! Dummy list, will be emty all the time!
        //!
        private List<byte[]> receivedData;

        //!
        //! Constructor
        //!
        public SceneReceiverModule(string name, Core core) : base(name, core)
        {
        }

        //!
        //! Function that overrides the default start function.
        //! That prevents that the system creates a new thread.
        //!
        //! @param ip IP address of the network interface.
        //! @param port Port number to be used.
        //!
        protected override void start(string ip, string port)
        {
            stop();

            m_ip = ip;
            m_port = port;

            run();
        }

        //!
        //! Function, requesting scene packages and receiving package data (executed in separate thread).
        //! As soon as all requested packages are received, a signal is emited that triggers the scene cration.
        //!
        protected override void run()
        {
            AsyncIO.ForceDotNet.Force();
            using (var sceneReceiver = new RequestSocket())
            {
                SceneManager sceneManager = m_core.getManager<SceneManager>();

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


        //! 
        //! Function that triggers the scene receiving process.
        //! @param ip The IP address to the server.
        //! @param port The port the server uses to send out the scene data.
        //! 
        public void receiveScene(string ip, string port)
        {
            m_requests = new List<string>() { "header", "nodes", "objects", "characters", "textures", "materials" };
            start(ip, port);
        }
    }

}
