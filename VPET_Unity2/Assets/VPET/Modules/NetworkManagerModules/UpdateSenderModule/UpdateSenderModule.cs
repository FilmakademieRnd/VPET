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
        //!
        //! List of medified parameters for undo/redo handling.
        //!
        private List<AbstractParameter> m_modifiedParameters;

        //!
        //! The size of all currently modified parameters in byte;
        //!
        private int m_modifiedParametersDataSize = 0;

        //!
        //! Array of control messages, containing all vpet messages besides parameter updates.
        //!
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
        public override void Dispose()
        {
            base.Dispose();
            SceneManager sceneManager = core.getManager<SceneManager>();
            UIManager uiManager = core.getManager<UIManager>();

            if (sceneManager != null && uiManager != null)
            {
                sceneManager.sceneReady -= connectAndStart;
                uiManager.selectionAdded -= lockSceneObject;
                uiManager.selectionRemoved -= unlockSceneObject;
            }

            core.syncEvent -= queuePingMessage;

            if (core.isServer)
                core.syncEvent -= queueSyncMessage;

            foreach (SceneObject sceneObject in sceneManager.getAllSceneObjects())
            {
                sceneObject.hasChanged -= queueModifiedParameter;
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

            foreach (SceneObject sceneObject in ((SceneManager)sender).getAllSceneObjects())
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
            m_controlMessage = new byte[7];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = core.time;
            m_controlMessage[2] = (byte)MessageType.LOCK;
            Helpers.copyArray(BitConverter.GetBytes(sceneObject.sceneID), 0, m_controlMessage, 3, 1);  // ScenetID
            Helpers.copyArray(BitConverter.GetBytes(sceneObject.id), 0, m_controlMessage, 4, 2);  // SceneObjectID
            m_controlMessage[6] = Convert.ToByte(true);

            m_mre.Set();
        }

        //!
        //! Function that creates and sends a (un)lock message after a selectionRemove event invokes.
        //!
        //! @param sender The UI manager.
        //! @param sceneObject The deselected scene object.
        //!
        private void unlockSceneObject(object sender, SceneObject sceneObject)
        {
            m_controlMessage = new byte[7];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = core.time;
            m_controlMessage[2] = (byte)MessageType.LOCK;
            Helpers.copyArray(BitConverter.GetBytes(sceneObject.sceneID), 0, m_controlMessage, 3, 1);  // ScenetID
            Helpers.copyArray(BitConverter.GetBytes(sceneObject.id), 0, m_controlMessage, 4, 2);  // SceneObjectID
            m_controlMessage[6] = Convert.ToByte(false);

            m_mre.Set();
        }


        //!
        //! Function that creates a ping message and adds it to the message queue for sending.
        //!
        //! @param sender The VPET core.
        //! @param time The clients global time.
        //!
        private void queuePingMessage(object sender, byte time)
        {
            m_controlMessage = new byte[3];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = time;
            m_controlMessage[2] = (byte)MessageType.PING;

            m_mre.Set();
        }

        //!
        //! Function that creates a sync message and adds it to the message queue for sending.
        //!
        //! @param sender The VPET core.
        //! @param time The clients global time.
        //!
        private void queueSyncMessage(object sender, byte time)
        {
            m_controlMessage = new byte[3];

            // header
            m_controlMessage[0] = manager.cID;
            m_controlMessage[1] = time;
            m_controlMessage[2] = (byte)MessageType.SYNC;

            m_mre.Set();
        }

        //!
        //! Function that creates a undo redo message.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //! @param sender The spinner UI element.
        //!
        public void queueUndoRedoMessage(object sender, AbstractParameter parameter)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneID, ParameterObjectID, ParameterID, ParameterType, ParameterData

            lock (parameter)
            {
                int parameterSize = parameter.dataSize();
                m_controlMessage = new byte[9 + parameterSize];
                parameter.Serialize(new Span<byte>(m_controlMessage, 9, parameterSize)); // ParameterData;

                // header
                m_controlMessage[0] = manager.cID;
                m_controlMessage[1] = core.time;
                m_controlMessage[2] = (byte)MessageType.UNDOREDOADD;

                // parameter
                Helpers.copyArray(BitConverter.GetBytes(parameter.parent.sceneID), 0, m_controlMessage, 3, 1);  // SceneID
                Helpers.copyArray(BitConverter.GetBytes(parameter.parent.id), 0, m_controlMessage, 4, 2);  // SceneObjectID
                Helpers.copyArray(BitConverter.GetBytes(parameter.id), 0, m_controlMessage, 6, 2);  // ParameterID
                m_controlMessage[8] = (byte)parameter.vpetType;  // ParameterType
            }

            m_mre.Set();
        }

        //!
        //! Function that creates a reset message.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //!
        public void queueResetMessage(SceneObject sceneObject)
        {
            // Message structure: Header, Parameter (optional)
            // Header: ClientID, Time, MessageType
            // Parameter: SceneID, ParameterObjectID, ParameterID, ParameterType, ParameterData

            lock (sceneObject)
            {
                m_controlMessage = new byte[6]; // ParameterData;

                // header
                m_controlMessage[0] = manager.cID;
                m_controlMessage[1] = core.time;
                m_controlMessage[2] = (byte)MessageType.RESETOBJECT;

                // parameter
                Helpers.copyArray(BitConverter.GetBytes(sceneObject.sceneID), 0, m_controlMessage, 3, 1);  // SceneID
                Helpers.copyArray(BitConverter.GetBytes(sceneObject.id), 0, m_controlMessage, 4, 2);  // SceneObjectID
            }

            m_mre.Set();
        }

        //!
        //! Function collects all parameter modifications within one global time tick for sending.
        //!
        //! @param sender The scene object containing the modified parameter.
        //! @param parameter The modified parameter.
        //!
        private void queueModifiedParameter(object sender, AbstractParameter parameter)
        {
            lock (m_modifiedParameters)
            {
                bool paramInList = m_modifiedParameters.Contains(parameter);
                if (parameter.isNetworkLocked)
                {
                    if (paramInList)
                    {
                        m_modifiedParameters.Remove(parameter);
                        m_modifiedParametersDataSize -= parameter.dataSize();
                    }
                }
                else if (!paramInList)
                {
                    m_modifiedParameters.Add(parameter);
                    m_modifiedParametersDataSize += parameter.dataSize();
                }
            }
        }

        //!
        //! Function that creates a parameter update message (byte[]) based on a abstract parameter and a time value.
        //!
        //! @param parameter The modified parameter the message will be based on.
        //! @param time The time for synchronization
        //! @param addToHistory should this update be added to undo/redo history
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] createParameterMessage()
        {
            // Message structure: Header, Parameter List (optional)
            // Header: ClientID, Time, MessageType
            // ParameterList: List<SceneObjectID, ParameterID, ParameterType, Parameter message length, ParameterData>

            byte[] message = new byte[3 + m_modifiedParametersDataSize + 7 * m_modifiedParameters.Count];
            Span<byte> msgSpan = new Span<byte>(message);

            // header
            msgSpan[0] = manager.cID; // ClientID
            msgSpan[1] = core.time; // Time
            msgSpan[2] = (byte)MessageType.PARAMETERUPDATE; // MessageType

            int start = 3;
            for (int i = 0; i < m_modifiedParameters.Count; i++)
            {
                AbstractParameter parameter = m_modifiedParameters[i];
                lock (parameter)
                {
                    int length = 7 + parameter.dataSize();
                    Span<byte> newSpan = msgSpan.Slice(start, length);

                    newSpan[0] = parameter.parent.sceneID;  // SceneID
                    BitConverter.TryWriteBytes(newSpan.Slice(1, 2), parameter.parent.id);  // SceneObjectID
                    BitConverter.TryWriteBytes(newSpan.Slice(3, 2), parameter.id);  // ParameterID
                    newSpan[5] = (byte)parameter.vpetType;  // ParameterType
                    newSpan[6] = (byte)newSpan.Length;  // Parameter message length
                    parameter.Serialize(newSpan.Slice(7)); // Parameter data

                    start += length;
                }
            }

            return message;
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
                else if (m_modifiedParameters.Count > 0)
                {
                    lock (m_modifiedParameters)
                    {
                        sender.SendFrame(createParameterMessage(), false); // true not wait
                        m_modifiedParameters.Clear();
                        m_modifiedParametersDataSize = 0;
                    }
                }
                // reset to stop the thread after one loop is done
                m_mre.Reset();

                Thread.Yield();
            }
            try
            {
                sender.Disconnect("tcp://" + m_ip + ":" + m_port);
                sender.Close();
                sender.Dispose();
                // wait until sender is disposed
                while (!sender.IsDisposed)
                    Thread.Sleep(25);
                Helpers.Log(this.name + " disposed.");
                m_disposed?.Invoke();
            }
            catch
            {
            }
        }

        //!
        //! Function that unlocks the sender thread once (called with every global tick event).
        //!
        //! @param sender The VPET core.
        //! @param e Empty.
        //!
        private void sendParameterMessages(object sender, EventArgs e)
        {
            if (m_modifiedParameters.Count > 0)
                m_mre.Set();
        }

        //!
        //! Function to start the scene sender module.
        //!
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        public void startUpdateSender(string ip, string port)
        {
            start(ip, port);
        }
    }
}
