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

//! @file "SceneSenderModule.cs"
//! @brief Implementation of the scene sender module, listening to scene requests and sending scene data. 
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 11.03.2022

using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the scene sender module, listening to scene requests and sending scene data.
    //!
    public class SceneSenderModule : NetworkManagerModule
    {
        //!
        //! The menu for the network configuration.
        //!
        private MenuTree m_menu;
        //!
        //! Preloaded scene data split up into several packages for header, nodes, objects,
        //! characters, textures and materials.
        //!
        private Dictionary<string, byte[]> m_responses;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public SceneSenderModule(string name, Manager manager) : base(name, manager)
        {
            if (!core.isServer)
                load = false;
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

            m_menu = new MenuTree()
               .Begin(MenuItem.IType.VSPLIT)
                    .Begin(MenuItem.IType.HSPLIT)
                        .Add("IP Address")
                        .Add(manager.settings.ipAddress)
                    .End()
                    .Begin(MenuItem.IType.HSPLIT)
                        .Add(button)
                    .End()
              .End();

            m_menu.caption = "Network Settings";
            m_menu.iconResourceLocation = "Images/button_network";
            core.getManager<UIManager>().addMenu(m_menu);
        }

        //! 
        //! Function called when an Unity Start() callback is triggered
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            //core.getManager<UIManager>().showMenu(m_menu);
        }

        private void Connect()
        {
            Helpers.Log(manager.settings.ipAddress.value);

            core.getManager<UIManager>().hideMenu();

            SceneParserModule sceneParserModule = core.getManager<SceneManager>().getModule<SceneParserModule>();
            sceneParserModule.ParseScene();

            sendScene(manager.settings.ipAddress.value, "5555");
        }

        //!
        //! Function, sending messages containing the scene data as reponces to the requested package (executed in separate thread).
        //!
        protected override void run()
        {
            m_isRunning = true;
            AsyncIO.ForceDotNet.Force();
            var dataSender = new ResponseSocket();

            dataSender.Bind("tcp://" + m_ip + ":" + m_port);
            Debug.Log("Enter while.. ");

            while (m_isRunning)
            {
                string message = "";
                dataSender.TryReceiveFrameString(out message);       // TryReceiveFrameString returns null if no message has been received!
                if (message != null)
                {
                    if (m_responses.ContainsKey(message))
                        dataSender.SendFrame(m_responses[message]);
                    else
                        dataSender.SendFrame(new byte[0]);

                }
                Thread.Sleep(100);
            }

            // TODO: check first if closed
            try
            {
                dataSender.Disconnect("tcp://" + m_ip + ":" + m_port);
                dataSender.Close();
                dataSender.Dispose();
                // wait until sender is disposed
                while (!dataSender.IsDisposed)
                    Thread.Sleep(25);
                Helpers.Log(this.name + " disposed.");
                m_disposed?.Invoke();
            }
            catch
            {
            }
        }

        //!
        //! Function to start the scene sender module.
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        public void sendScene(string ip, string port)
        {
            m_responses = new Dictionary<string, byte[]>();
            SceneManager.SceneDataHandler dataHandler = core.getManager<SceneManager>().sceneDataHandler;

            m_responses.Add("header", dataHandler.headerByteDataRef);
            m_responses.Add("nodes", dataHandler.nodesByteDataRef);
            m_responses.Add("objects", dataHandler.objectsByteDataRef);
            m_responses.Add("characters", dataHandler.characterByteDataRef);
            m_responses.Add("textures", dataHandler.texturesByteDataRef);
            m_responses.Add("materials", dataHandler.materialsByteDataRef);

            start(ip, port);
        }
    }
}
