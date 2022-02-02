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
//! @date 25.06.2021

using System.Collections.Generic;
using System;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;

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
        //! The menu for the network configuration.
        //!
        private MenuTree m_menu;

        // [REVIEW]
        // needs to be replaced by an proper serialization fonctionality 
        // to store a parameters value into the settings files.
        private Parameter<string> m_serverIP_Param;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public SceneReceiverModule(string name, Core core) : base(name, core)
        {
        }

        //! 
        //!  Function called when an Unity Awake() callback is triggered
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            Parameter<Action> button = new Parameter<Action>(Connect, "Start");
            m_serverIP_Param = new Parameter<string>(manager.settings.m_serverIP, "IP Adress");

            m_menu = new MenuTree()
               .Begin(MenuItem.IType.VSPLIT)
                   .Add("Please enter IP Adress!")
                   .Add(m_serverIP_Param)
                   .Add(button)
              .End();

            m_menu.name = "Network Client";
            m_core.getManager<UIManager>().addMenu(m_menu);
        }

        //! 
        //! Function called when an Unity Start() callback is triggered
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            m_core.getManager<UIManager>().showMenu(m_menu);
        }

        private void Connect()
        {
            manager.settings.m_serverIP = m_serverIP_Param.value;
            Helpers.Log(manager.settings.m_serverIP);

            m_core.getManager<UIManager>().showMenu(null);

            receiveScene(manager.settings.m_serverIP, "5555");
        }

        //!
        //! Function that overrides the default start function.
        //! That prevents that the system creates a new thread.
        //!
        //! @param ip IP address of the network interface.
        //! @param port Port number to be used.
        //!
        protected async override void start(string ip, string port)
        {
            m_ip = ip;
            Helpers.Log(ip);
            m_port = port;

            await Task.Run(() => run());
            
            // emit sceneReceived signal to trigger scene cration in the sceneCreator module
            m_sceneReceived?.Invoke(this, new EventArgs());
        }


        //!
        //! Function, requesting scene packages and receiving package data (executed in separate thread).
        //! As soon as all requested packages are received, a signal is emited that triggers the scene cration.
        //!
        protected override void run()
        {
            AsyncIO.ForceDotNet.Force();
            var sceneReceiver = new RequestSocket();

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
            }
            try
            {
                sceneReceiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                sceneReceiver.Dispose();
                sceneReceiver.Close();
            }
            finally
            {
                NetMQConfig.Cleanup(false);
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
