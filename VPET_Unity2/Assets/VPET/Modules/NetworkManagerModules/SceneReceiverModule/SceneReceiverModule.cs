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

        //!
        //! A local reference to the netMQ scene reciver socket.
        //!
        private RequestSocket m_sceneReceiver;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public SceneReceiverModule(string name, Manager manager) : base(name, manager)
        {
            if (core.isServer)
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

            List<AbstractParameter> parameterList1 = new List<AbstractParameter>();
            parameterList1.Add(new Parameter<string>(null, "Server"));
            parameterList1.Add(new Parameter<string>(null, "Device"));

            List<AbstractParameter> parameterList2 = new List<AbstractParameter>();
            parameterList2.Add(new Parameter<string>(null, "Scout"));
            parameterList2.Add(new Parameter<string>(null, "Director"));
            parameterList2.Add(new Parameter<string>(null, "Lighting"));

            m_menu = new MenuTree()
                .Begin(MenuItem.IType.VSPLIT)
                    .Begin(MenuItem.IType.HSPLIT)
                         .Add("Scene Source")
                         .Add(new ListParameter(parameterList1, "Device"))
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add("IP Address")
                         .Add(manager.settings.ipAddress)
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add(button)
                     .End()
                .End();

            m_menu.iconResourceLocation = "Images/button_network";
            m_menu.caption = "Network Settings";
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
            core.getManager<UIManager>().showMenu(m_menu);
        }

        private void Connect()
        {
            Helpers.Log(manager.settings.ipAddress.value);

            core.getManager<UIManager>().hideMenu();

            receiveScene(manager.settings.ipAddress.value, "5555");
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

            NetworkManager.threadCount++;

            await Task.Run(() => run());

            // emit sceneReceived signal to trigger scene cration in the sceneCreator module
            if (core.getManager<SceneManager>().sceneDataHandler.headerByteDataRef != null)
                m_sceneReceived?.Invoke(this, new EventArgs());
        }

        public override void Dispose()
        {
            base.Dispose();

            disposeReceiver();
            m_disposed?.Invoke();   // does stall for some reason [REVIEW]
        }

        private void disposeReceiver()
        {
            try
            {
                if ((m_sceneReceiver != null) && !m_sceneReceiver.IsDisposed)
                {
                    m_sceneReceiver.Disconnect("tcp://" + m_ip + ":" + m_port);
                    m_sceneReceiver.Close();
                    m_sceneReceiver.Dispose();
                    // wait until receiver is disposed
                    while (!m_sceneReceiver.IsDisposed)
                        System.Threading.Thread.Sleep(25);
                    Helpers.Log(this.name + " disposed.");
                }
            }
            catch { }
        }

        //!
        //! Function, requesting scene packages and receiving package data (executed in separate thread).
        //! As soon as all requested packages are received, a signal is emited that triggers the scene cration.
        //!
        protected override void run()
        {
            AsyncIO.ForceDotNet.Force();
            m_sceneReceiver = new RequestSocket();

            SceneManager sceneManager = core.getManager<SceneManager>();
            m_sceneReceiver.Connect("tcp://" + m_ip + ":" + m_port);
            SceneManager.SceneDataHandler sceneDataHandler = sceneManager.sceneDataHandler;

            try
            {
                foreach (string request in m_requests)
                {
                    m_sceneReceiver.SendFrame(request);
                    switch (request)
                    {
                        case "header":
                            sceneDataHandler.headerByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                        case "nodes":
                            sceneDataHandler.nodesByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                        case "objects":
                            sceneDataHandler.objectsByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                        case "characters":
                            sceneDataHandler.characterByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                        case "textures":
                            sceneDataHandler.texturesByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                        case "materials":
                            sceneDataHandler.materialsByteData = m_sceneReceiver.ReceiveFrameBytes();
                            break;
                    }
                }
            }
            catch { }
            disposeReceiver();
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
