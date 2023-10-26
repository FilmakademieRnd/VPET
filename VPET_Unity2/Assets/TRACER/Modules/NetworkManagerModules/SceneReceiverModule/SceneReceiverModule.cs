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

//! @file "SceneReceiverModule.cs"
//! @brief Implementation of the scene receiver module, sending scene requests and receives scene data. 
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 25.06.2021

using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

namespace tracer
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
        //! A local reference to the netMQ scene receiver socket.
        //!
        private RequestSocket m_sceneReceiver;
        
        private GameObject _qrCanvas;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the TRACER core.
        //!
        public SceneReceiverModule(string name, Manager manager) : base(name, manager)
        {
            if (core.isServer)
                load = false;
        }

        //! 
        //!  Function called when an Unity Awake() callback is triggered
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {

        }

        //! 
        //! Function called when an Unity Start() callback is triggered
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            
            _qrCanvas = Resources.Load("Prefab/QR_Reader") as GameObject;
            
            Parameter<Action> button = new Parameter<Action>(Connect, "Connect");
            Parameter<Action> buttonQr = new Parameter<Action>(ConnectUsingQR, "QR Connect");

            List<AbstractParameter> parameterList1 = new List<AbstractParameter>
            {
                new Parameter<string>(null, "Server"),
                new Parameter<string>(null, "Device")
            };

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
                         .Add(buttonQr)
                     .End()
                .End();

            m_menu.iconResourceLocation = "Images/button_network";
            m_menu.caption = "Network Settings";
            UIManager uiManager = core.getManager<UIManager>();
            uiManager.addMenu(m_menu);

            // add elements to start menu;
            uiManager.startMenu
                .Begin(MenuItem.IType.HSPLIT)
                    .Add("IP Address")
                    .Add(manager.settings.ipAddress)
                .End()
                .Begin(MenuItem.IType.HSPLIT)
                     .Add(button)
                     .Add(buttonQr)
                .End();

            //core.getManager<UIManager>().showMenu(m_menu);
        }

        private void Connect()
        {
            Helpers.Log(manager.settings.ipAddress.value);

            core.getManager<UIManager>().hideMenu();

            receiveScene(manager.settings.ipAddress.value, "5555");
        }
        
                
        private void ConnectUsingQR()
        {
            GameObject QR = GameObject.Instantiate(_qrCanvas);
            core.getManager<UIManager>().hideMenu();
        }

        //!
        //! Function that overrides the default start function.
        //! Because of Unity's single threded design we have to 
        //! split the work within a coroutine.
        //!
        //! @param ip IP address of the network interface.
        //! @param port Port number to be used.
        //!
        protected override void start(string ip, string port)
        {
            m_ip = ip;
            Helpers.Log(ip);
            m_port = port;

            NetworkManager.threadCount++;

            core.StartCoroutine(startReceive());
        }

        //!
        //! Coroutine that creates a new thread receiving the scene data
        //! and yielding to allow the main thread to update the statusDialog.
        //!
        private IEnumerator startReceive()
        {
            Dialog statusDialog = new Dialog("Receive Scene", "", Dialog.DTypes.BAR);
            UIManager uiManager = core.getManager<UIManager>();
            uiManager.showDialog(statusDialog);

            Thread receiverThread = new Thread(run);
            receiverThread.Start();

            while (receiverThread.IsAlive)
            {
                yield return null;
                statusDialog.progress += 3;
            }

            // emit sceneReceived signal to trigger scene cration in the sceneCreator module
            if (core.getManager<SceneManager>().sceneDataHandler.headerByteDataRef != null)
                m_sceneReceived?.Invoke(this, EventArgs.Empty);

            uiManager.showDialog(null);
        }

        //!
        //! Function for cleanup. Stopping the receiver thread and dispose
        //! the netMQ sockets.
        //!
        public override void Dispose()
        {
            base.Dispose();

            disposeReceiver();
            m_disposed?.Invoke();   // does stall for some reason [REVIEW]
        }

        //!
        //! Funcrtion for dispoising the netMQ sockets of the receiver.
        //!
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
