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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "core.cs"
//! @brief VPET core implementation. Central class for VPET initalization. Manages all VPETManagers and their modules.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace vpet
{
    //!
    //! Central class for VPET initalization.
    //! Manages all VPETManagers and their modules.
    //!
    public class Core : CoreInterface
    {
        //!
        //! Class containing the cores settings.
        //! This is persistent data saved and loaded to/from disk.
        //!
        public class coreSettings : Settings
        {
            //!
            //! Default screen size (unused)
            //!
            public Vector2Int screenSize = new Vector2Int(1280,720);
            //! 
            //! VSync on every frame.
            //!
            public int vSyncCount = 1;
            //! 
            //! Global frame rate in t per second.
            //! 
            public int framerate = 60;
        }
        //!
        //! The core settings.
        //!
        private Settings _settings;
        //!
        //! Getter and setter for the core settings.
        //!
        public coreSettings settings
        {
            get { return (coreSettings)_settings; }
            set { _settings = value; }
        }
        //!
        //! Flag determining wether the VPERT instance acts as a server or client.
        //!
        public bool isServer = false;
        //!
        //! The current local time stores as value between 0 and 255.
        //!
        private byte m_time = 0;
        //!
        //! The current local time stores as value between 0 and 255.
        //!
        public byte time 
        { 
            set => m_time = value;
            get => m_time;
        }
        //!
        //! The max value for the local time.
        //!
        private byte m_timesteps;
        //!
        //! The max value for the local time.
        //!
        public byte timesteps
        {
            get => m_timesteps;
        }
        //!
        //! The global list of parameter objects.
        //!
        private List<ParameterObject> m_parameterObjectList;
        private DeviceOrientation m_orientation;
        //!
        //! Getter for the parameter object list.
        //!
        //! @return A reference to the parameter object list.
        //!
        public ref List<ParameterObject> parameterObjectList
        {
            get => ref m_parameterObjectList;
        }
        //!
        //! Event invoked when an Unity Update() callback is triggered.
        //!
        public event EventHandler updateEvent;
        //!
        //! Event invoked when an Unity Awake() callback is triggered.
        //!
        public event EventHandler awakeEvent;
        //!
        //! Event invoked after the awakeEvent is triggered.
        //!
        public event EventHandler lateAwakeEvent;
        //!
        //! Event invoked when an Unity Start() callback is triggered.
        //!
        public event EventHandler startEvent;
        //!
        //! Event invoked when an Unity OnDestroy() callback is triggered.
        //!
        public event EventHandler destroyEvent;
        //!
        //! Event invoked when VPETs global timer ticks.
        //!
        public event EventHandler timeEvent;
        //!
        //! Event invoked every second.
        //!
        public event EventHandler<byte> syncEvent;

        //!
        //! Unity's Awake callback, used for Initialization of all Managers and modules.
        //!
        void Awake()
        {
            _settings = new coreSettings();
            m_timesteps = (byte)(((int)(256f / settings.framerate)) * settings.framerate);
            m_parameterObjectList = new List<ParameterObject>();

            // Create network manager
            NetworkManager networkManager = new NetworkManager(typeof(NetworkManagerModule), this);
            m_managerList.Add(typeof(NetworkManager), networkManager);
            
            //Create scene manager
            SceneManager sceneManager = new SceneManager(typeof(SceneManagerModule), this);
            m_managerList.Add(typeof(SceneManager), sceneManager);

            //Create UI manager
            UIManager uiManager = new UIManager(typeof(UIManagerModule), this);
            m_managerList.Add(typeof(UIManager), uiManager);

            //Create Input manager
            InputManager inputManager = new InputManager(typeof(InputManagerModule), this);
            m_managerList.Add(typeof(InputManager), inputManager);

            LoadSettings();

            awakeEvent?.Invoke(this, new EventArgs());
            lateAwakeEvent?.Invoke(this, new EventArgs());
        }

        //!
        //! Unity's Start callback, used for Late initialization.
        //!
        void Start()
        {
            // Sync framerate to monitors refresh rate
            QualitySettings.vSyncCount = settings.vSyncCount;
            Application.targetFrameRate = settings.framerate;

            m_orientation = Input.deviceOrientation;

            InvokeRepeating("checkDeviceOrientation", 0f, 1f);
            InvokeRepeating("updateTime", 0f, 1f/settings.framerate);

            startEvent?.Invoke(this, new EventArgs());
        }

        //!
        //! Unity's OnDestroy callback, used to invoke a destroy event to inform VPET modules.
        //!
        private void OnDestroy()
        {
            SaveSettings();
            destroyEvent?.Invoke(this, new EventArgs());
        }

        //!
        //! Unity's Update callback, used to invoke a update event to inform VPET modules.
        //!
        private void Update()
        {
            QualitySettings.vSyncCount = 1;
            updateEvent?.Invoke(this, EventArgs.Empty);
        }

        private void checkDeviceOrientation()
        {
            if (Input.deviceOrientation != m_orientation)
            {
                Camera mainCamera = Camera.main;
                if ((Input.deviceOrientation == DeviceOrientation.Portrait &&
                     (m_orientation == DeviceOrientation.LandscapeLeft ||
                      m_orientation == DeviceOrientation.LandscapeRight))
                      ||
                     ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
                      Input.deviceOrientation == DeviceOrientation.LandscapeRight) &&
                     m_orientation == DeviceOrientation.Portrait))
                {
                    mainCamera.aspect = 1f / mainCamera.aspect;
                }
                m_orientation = Input.deviceOrientation;
            }
        }

        //!
        //! Function for increasing and resetting the time variable.
        //!
        private void updateTime()
        {
            timeEvent?.Invoke(this, EventArgs.Empty);

            m_time = (m_time > (m_timesteps-1) ? (byte)0 : m_time+=1);

            if ((m_time % settings.framerate) == 0)
                syncEvent?.Invoke(this, m_time);
        }

        //!
        //! Function to save the modules- and core settins to disk.
        //!
        private void SaveSettings()
        {
            foreach (Manager manager in getManagers())
                if (manager._settings != null)
                    Save(Application.persistentDataPath, manager._settings);

            Save(Application.persistentDataPath, _settings);
        }

        //!
        //! Function to load the modules- and core settins from disk.
        //!
        private void LoadSettings()
        {
            foreach (Manager manager in getManagers())
                if (manager._settings != null)
                    Load(Application.persistentDataPath, ref manager._settings);

            Load(Application.persistentDataPath, ref _settings);
        }

        //!
        //! Function to serialize settings and write it to disk.
        //!
        internal void Save(string path, Settings settings)
        {
            string filepath = Path.Combine(path, settings.GetType().ToString() + ".cfg");
            File.WriteAllText(filepath, JsonUtility.ToJson(settings));
            Helpers.Log("Settings saved to: " + filepath);
        }

        //!
        //! Function to read settings from disk and deserialze it to a Settings class.
        //!
        internal void Load(string path, ref Settings settings)
        {
            string filepath = Path.Combine(path, settings.GetType() + ".cfg");
            if (File.Exists(filepath))
                settings = (Settings)JsonUtility.FromJson(File.ReadAllText(filepath), settings.GetType());
        }

        //!
        //! Function for adding parameter objects to the prameter object list.
        //!
        //! @parameterObject The parameter object to be added to the parameter object list.
        //!
        internal void addParameterObject(ParameterObject parameterObject)
        {
            if (!m_parameterObjectList.Contains(parameterObject))
                m_parameterObjectList.Add(parameterObject);
            else
                Helpers.Log("Parameter object List already contains the Parameter Object.", Helpers.logMsgType.WARNING);
        }

    }
}