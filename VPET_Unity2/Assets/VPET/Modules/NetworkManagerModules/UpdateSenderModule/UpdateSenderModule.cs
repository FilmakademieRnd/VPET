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

//! @file "UpdateSenderModule.cs"
//! @brief Implementation of the update sender module, sending parameter updates to clients.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 28.10.2021

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    //!
    //! Class implementing the update sender module, sending parameter updates to clients.
    //!
    public class UpdateSenderModule : NetworkManagerModule
    {
        
        private List<AbstractParameter> m_modifiedParameters;
        private byte[] m_controlMessage;

        //!
        //! Constructor
        //!
        //! @param  name  The  name of the module.
        //! @param core A reference to the VPET core.
        //!
        public UpdateSenderModule(string name, Manager manager) : base(name, manager)
        {
        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        ~UpdateSenderModule()
        {
            SceneManager sceneManager = core.getManager<SceneManager>();
            UIManager uiManager = core.getManager<UIManager>();

            sceneManager.sceneReady -= connectAndStart;
            uiManager.selectionAdded -= lockSceneObject;
            uiManager.selectionRemoved -= unlockSceneObject;

            core.syncEvent -= queuePingMessage;

            if (core.isServer)
                core.syncEvent -= queueSyncMessage;

            foreach (ParameterObject parameterObject in core.parameterObjectList)
            {
                parameterObject.hasChanged -= queueModifiedParameter;
            }

            core.timeEvent -= sendParameterMessages;
        }

        //!
        //! Function for custom initialisation.
        //! 
        //! @param sender The VPET core.
        //! @param e The pssed event arguments.
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            m_modifiedParameters = new List<AbstractParameter>();

            SceneManager sceneManager = core.getManager<SceneManager>();
            sceneManager.sceneReady += connectAndStart;
        }

        //!
        //! Function that connects the scene object change events for parameter queuing.
        //!
        //! @param sender The SceneManager.
        //! @param e The pssed event arguments.
        //!
        private void connectAndStart(object sender, EventArgs e)
        {
            startUpdateSender(manager.settings.ipAddress.value, "5557");

            UIManager uiManager = core.getManager<UIManager>();
            uiManager.selectionAdded += lockSceneObject;
            uiManager.selectionRemoved += unlockSceneObject;

            core.syncEvent += queuePingMessage;

            if (core.isServer)
                core.syncEvent += queueSyncMessage;

            foreach (SceneObject sceneObject in ((SceneManager) sender).sceneObjects)
            {
                sceneObject.hasChanged += queueModifiedParameter;
            }

            core.timeEvent += sendParameterMessages;
        }

        //!
        //! Function that creates and sends a lock message after a selectionAdd event invokes.
        //!
        //! @param sender The UI manager.
        //! @param sceneObject The selected scene object.
        //!
        private void lockSceneObject(object sender, SceneObject sceneObject)
        {
            m_controlMessage = new byte[6];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = core.time;
            m_controlMessage[2] = (byte)MessageType.LOCK;
            Buffer.BlockCopy(BitConverter.GetBytes(sceneObject.id), 0, m_controlMessage, 3, 2);  // SceneObjectID
            m_controlMessage[5] = Convert.ToByte(true);

            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function that creates and sends a (un)lock message after a selectionRemove event invokes.
        //!
        //! @param sender The UI manager.
        //! @param sceneObject The deselected scene object.
        //!
        private void unlockSceneObject(object sender, SceneObject sceneObject)
        {
            m_controlMessage = new byte[6];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = core.time;
            m_controlMessage[2] = (byte)MessageType.LOCK;
            Buffer.BlockCopy(BitConverter.GetBytes(sceneObject.id), 0, m_controlMessage, 3, 2);  // SceneObjectID
            m_controlMessage[5] = Convert.ToByte(false);

            m_mre.Set();
            m_mre.Reset();
        }


        //!
        //! Function that creates a ping message and adds it to the message queue for sending.
        //!
        //! @param sender The VPET core.
        //! @param time The clients global time.
        //!
        private void queuePingMessage(object o, byte time)
        {
            m_controlMessage = new byte[3];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = time;
            m_controlMessage[2] = (byte)MessageType.PING;

            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function that creates a sync message and adds it to the message queue for sending.
        //!
        //! @param sender The VPET core.
        //! @param t The clients global time.
        //!
        private void queueSyncMessage(object o, byte time)
        {
            m_controlMessage = new byte[3];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = time;
            m_controlMessage[2] = (byte)MessageType.SYNC;

            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function that creates a undo redo message.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //!
        public void queueUndoRedoMessage(object o, AbstractParameter parameter)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneObjectID, ParameterID, ParameterType, ParameterData

            lock (parameter)
            {
                m_controlMessage = parameter.Serialize(8); // ParameterData;

                // header
                m_controlMessage[0] = manager.cID;
                m_controlMessage[1] = 0;
                m_controlMessage[2] = (byte)MessageType.UNDOREDOADD;

                // parameter
                Buffer.BlockCopy(BitConverter.GetBytes(parameter.parent.id), 0, m_controlMessage, 3, 2);  // SceneObjectID
                Buffer.BlockCopy(BitConverter.GetBytes(parameter.id), 0, m_controlMessage, 5, 2);  // ParameterID
                m_controlMessage[7] = (byte)parameter.vpetType;  // ParameterType
            }

            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function that creates a reset message.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //!
        public void queueResetMessage(SceneObject s)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneObjectID, ParameterID, ParameterType, ParameterData

            lock (s)
            {
                m_controlMessage = new byte[5]; // ParameterData;

                // header
                m_controlMessage[0] = manager.cID;
                m_controlMessage[1] = 0;
                m_controlMessage[2] = (byte)MessageType.RESETOBJECT;

                // parameter
                Buffer.BlockCopy(BitConverter.GetBytes(s.id), 0, m_controlMessage, 3, 2);  // SceneObjectID
            }

            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function collects all parameter modifications within one global time tick for sending.
        //!
        //! @param sender The scene object containing the modified parameter.
        //! @param parameter The modified parameter.
        //!
        private void queueModifiedParameter(object sender, AbstractParameter parameter)
        {
            if (!m_modifiedParameters.Contains(parameter))
                m_modifiedParameters.Add(parameter);
        }

        //!
        //! Function that creates a parameter update message (byte[]) based on a abstract parameter and a time value.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //! @param time The time for synchronization
        //! @param addToHistory should this update be added to undo/redo history
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] createParameterMessage(AbstractParameter parameter, byte time)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneObjectID, ParameterID, ParameterType, ParameterData

            lock (parameter)
            {
                byte[] message = parameter.Serialize(8); // ParameterData;

                // header
                message[0] = manager.cID;
                message[1] = time;
                message[2] = (byte)MessageType.PARAMETERUPDATE;
                
                // parameter
                Buffer.BlockCopy(BitConverter.GetBytes(parameter.parent.id), 0, message, 3, 2);  // SceneObjectID
                Buffer.BlockCopy(BitConverter.GetBytes(parameter.id), 0, message, 5, 2);  // ParameterID
                message[7] = (byte)parameter.vpetType;  // ParameterType

                return message;
            }
        }

        //!
        //! Function, sending control messages and parameter update messages (executed in separate thread).
        //! Thread execution is locked after every loop and unlocked by sendParameterMessages every global tick.
        //!
        protected override void run()
        {
            m_isRunning = true;
            AsyncIO.ForceDotNet.Force();
            var sender = new PublisherSocket();

            sender.Connect("tcp://" + m_ip + ":" + m_port);
            Helpers.Log("Update sender connected: " + "tcp://" + m_ip + ":" + m_port);
            while (m_isRunning)
            {
                m_mre.WaitOne();
                if (m_controlMessage != null)
                {
                    lock (m_controlMessage)
                    {
                        sender.SendFrame(m_controlMessage, false); // true not wait 
                        m_controlMessage = null;
                    }
                }
                else
                {
                    lock (m_modifiedParameters)
                    {
                        if (m_modifiedParameters.Count > 0)
                        {
                            byte time = core.time;
                            foreach (AbstractParameter parameter in m_modifiedParameters)
                                sender.SendFrame(createParameterMessage(parameter, time), false); // true not wait
                            m_modifiedParameters.Clear();
                        }
                    }
                }
                Thread.Yield();
            }
            try
            {
                sender.Disconnect("tcp://" + m_ip + ":" + m_port);
                sender.Close();
                sender.Dispose();
                Helpers.Log(this.name + " disposed.");
                Thread.Sleep(500);
            }
            finally
            {
                //NetMQConfig.Cleanup(false);
            }

        }

        //!
        //! Function that unlocks the sender thread once (called with every global tick event).
        //!
        private void sendParameterMessages(object o, EventArgs e)
        {
            m_mre.Set();
            m_mre.Reset();
        }

        //!
        //! Function to start the scene sender module.
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        public void startUpdateSender(string ip, string port)
        {
            start(ip, port);
        }
    }
}
