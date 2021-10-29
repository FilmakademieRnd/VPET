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
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "core.cs"
//! @brief VPET core implementation. Central class for VPET initalization. Manages all VPETManagers and their modules.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 28.10.2021

using System;
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
        public class coreSettings : Settings
        {
            public Vector2Int screenSize = new Vector2Int(1280,720);
            public int vSyncCount = 1;
            public int framerate = 60;

            public bool isServer = false;
        }

        public Core()
        {

        }

        private byte m_time = 0;
        public byte time 
        { 
            set => m_time = value;
            get => m_time;
        }

        private Settings _settings;
        public coreSettings settings 
        {
            get { return (coreSettings) _settings; }
            set { _settings = value; } 
        }

        //!
        //! Event invoked when an Unity Update() callback is triggerd.
        //!
        public event EventHandler updateEvent;
        //!
        //! Event invoked when an Unity Awake() callback is triggerd.
        //!
        public event EventHandler awakeEvent;
        //!
        //! Event invoked when an Unity OnDestroy() callback is triggerd.
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
        //! Initialization of all Managers and modules.
        //!
        void Awake()
        {
            _settings = new coreSettings();

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
        }

        void Start()
        {
            // Sync framerate to monitors refresh rate
            QualitySettings.vSyncCount = settings.vSyncCount;
            Application.targetFrameRate = settings.framerate;

            InvokeRepeating("updateTime", 0f, 1f/settings.framerate);
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

        //!
        //! Function for increasing and resetting the time variable.
        //!
        private void updateTime()
        {
            timeEvent?.Invoke(this, EventArgs.Empty);

            m_time = (m_time > 239 ? (byte)0 : m_time+=1);

            if ((m_time % settings.framerate) == 0)
                syncEvent?.Invoke(this, m_time);
        }

        private void SaveSettings()
        {
            foreach (Manager manager in getManagers())
                if (manager._settings != null)
                    Save(Application.persistentDataPath, manager._settings);

            Save(Application.persistentDataPath, settings);
        }

        private void LoadSettings()
        {
            foreach (Manager manager in getManagers())
                if (manager._settings != null)
                    Load(Application.persistentDataPath, ref manager._settings);

            Load(Application.persistentDataPath, ref _settings);
        }

        internal void Save(string path, Settings settings)
        {
            string filepath = Path.Combine(path, settings.GetType().ToString() + ".cfg");
            File.WriteAllText(filepath, JsonUtility.ToJson(settings));
            Helpers.Log("Settings saved to: " + filepath);
        }

        internal void Load(string path, ref Settings settings)
        {
            string filepath = Path.Combine(path, settings.GetType() + ".cfg");
            if (File.Exists(filepath))
                settings = (Settings)JsonUtility.FromJson(File.ReadAllText(filepath), settings.GetType());
        }

    }
}