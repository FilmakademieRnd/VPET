/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
// using UnityEngine.Rendering;

//!
//! MainController part handling initalization of MainController
//!
namespace vpet
{
    public partial class MainController : MonoBehaviour
    {
        //!
        //! is the help menu currently displayed
        //!
        public bool helpActive;

        //!
        //! shall camera icons be displayed
        //!
        public bool showCam;

        //!
        //! lock all scene objects locally
        //! used to avoid unwanted updates while in ar setup
        //!
        public bool lockScene;

        //!
        //! are changes reported to server immediatelly (while dragging)
        //!
        public bool liveMode = true;
        //!
        //! shall we ignore config file 
        //!
        public bool ignoreConfig = false;
        //!
        //! IP Adress of server
        //!
        public string serverIP = "172.17.21.129";
        //!
        //! IP Adress of katana server
        //!
        public string katanaIP = "172.17.21.129";
        //!
        //! Shall we read the binary scene from cache
        //!
        public bool doLoadFromResource = false;


        public bool isMaster = false;

        //!
        //! Value of scene ambient light
        //!
        public float ambientLight = 0.1f;
        //!
        //! cached references to server adapter
        //!
        private ServerAdapter serverAdapter;
        //!
        //! cached references to camera adapter
        //!
        public MoveCamera cameraAdapter;
        //!
        //! cached references to scene adapter
        //!
        private SceneLoader sceneAdapter;
        //!
        //! cached reference to animation adapter
        //!
        private AnimationController animationController;
        public AnimationController AnimationController
        {
            get { return animationController; }
        }
        //!
        //! cached references to user interface
        //!
        private UI ui;
        public UI UIAdapter
        {
            get { return ui; }
        }

#if !SCENE_HOST
        //!
        //! cached references to joystic adapter
        //!
        private JoystickInput joystickAdapter;
#endif

        //!
        //! cached references to input adapter
        //!
        private InputAdapter inputAdapter;

        //!
        //! cached Reference to Scene containing all relevant Assets
        //!
        private GameObject scene;

        //!
        //! reference to the asset representing the camera
        //!
        GameObject camObject;

        //!
        //! reference to camera rig / parent
        //!
        Transform cameraRig;

        //!
        //! currently selected camera position (index of cameraPositions array)
        //!
        int camPrefabPosition = 0;
		//!
		//! latest camera position
		//!
		Vector3 camPreviousPosition = Vector3.zero;
		//!
		//! latest camera rotation
		//!
		Quaternion camPreviousRotation = Quaternion.identity;
        //!
        //! cache references to TRS group
        //!
        private GameObject trsGroup;
        //!
        //! cache references to translation modifier
        //!
        private GameObject translateModifier;
        //!
        //! cache references to rotation modifier
        //!
        private GameObject rotationModifier;
        //!
        //! cache references to scale modifier
        //!
        private GameObject scaleModifier;
        //!
        //! cache references to point o move modifier
        //!
        private GameObject pointToMoveModifier;

        [HideInInspector]
        //!
        //! cache references to active transform plane
        //!
        public Plane helperPlane;

        //!
        //! currently selected Object (if none = null)
        //!
        private Transform currentSelection;

        //!
        //! currently selected Object (if none = null)
        //!
        private SceneObject currentSceneObject;

        //!
        //! Reference to UI element visualizing the look through camera mode
        //! this is the frame on the screen borders
        //!
        public GameObject camVisualizer;

        //!
        //! Variables to handle object movement
        //!
        public bool isTranslating = false;
        public bool isRotating = false;
        public bool isScaling = false;

        //!
        //! currently selected Object (if none = null)
        //!
        private View currentCameraView = View.PERSP;

        // Debuging only
        [HideInInspector]
        public bool MouseInputActive = false;
        [HideInInspector]
        public bool TouchInputActive = false;

        private bool hasUpdatedProjectionMatrix = false;

        private PropertyInfo rangeSliderInfo;

        private System.Object rangeSliderInfoObj;

        //private CommandBuffer[] bufBeforeForwardOpaque;
        //private CommandBuffer[] bufBeforeGBuffer;

        //!
        //! the available states (modes) of the app
        //!
        public enum Mode {
            translationMode,            // show/use translation modifier
            rotationMode,               // show/use rotation modifier
            scaleMode,                  // show/use scale modifier
            objectLinkCamera,           // deactivate kinematic and parent to camera
            pointToMoveMode,            // click on ground to move object
            lightSettingsMode,          // show/use light settings widget (intensity, color)
            cameraSettingsMode,         // show/use camera settings widget
            cameraLockedMode,           // locked camera scouting mode
            objectMenuMode,             // show centre menu (object or animation icons)
            lightMenuMode,              // show centre menu (light icons depending on light type)
            cameraMenuMode,             // show centre menu (camera icons)
            addMode,                    // multiple selection TODO: not implemented yet
            idle,                       // no selection and no action
            animationEditing,           // set animation editing, show translation manipulator
            lookThroughCamMode,         // camera scouting mode
            lookThroughLightMode        // light scouting mode
        };


		//!
		//! the available camera states / projections
		//!
        public enum View {
            PERSP,
            TOP,
            BOTTOM,
            LEFT,
            RIGHT,
            FRONT,
            BACK,
			NCAM };

        //!
        //! available edit paramters
        //!
        public enum Parameter { X, Y, Z, INTENSITY, ANGLE };

        [HideInInspector]
        //!
        //! currently active mode
        //!
        private Mode activeMode;
        public Mode ActiveMode
        {
            get { return activeMode; }
            set { activeMode = value; }
        }


        //!
        //! Use this for pre initialization 
        //!
        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
            Resolution res = Screen.currentResolution;
            //Screen.SetResolution(res.width/2, res.height/2, true);

            // read settings from inspector values
            VPETSettings.mapValuesFromObject(this);

            // read settings from config file if wanted
            if (!ignoreConfig)
            {
                string filePath = Application.dataPath + "/VPET/editing_tool.cfg";
                if (!File.Exists(filePath))
                {
                    filePath = Application.persistentDataPath + "/editing_tool.cfg";
                }

                VPETSettings.mapValuesFromConfigFile(filePath);
            }

			// read settings from user preferences
			VPETSettings.mapValuesFromPreferences();


            // check if scene dump file available
            if (Directory.Exists(Application.dataPath + "/Resources/VPET/SceneDumps"))
            {
                VPETSettings.Instance.sceneDumpFolderEmpty = (Directory.GetFiles(Application.dataPath + "/Resources/VPET/SceneDumps", "*.bytes").Length == 0);
                // print("PETSettings.Instance.sceneDumpFolderEmpty " + VPETSettings.Instance.sceneDumpFolderEmpty);
            }

            VPETSettings.Instance.sceneDumpFolderEmpty = false;

            // Register plugins
            VPETRegister.RegisterNodeParser();
            VPETRegister.RegisterNodeBuilder();
            VPETRegister.RegisterObjectSender();

            // get all adapters and add them if missing
            //	
            // get scene adapter
            GameObject refObject = GameObject.Find("SceneAdapter");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No SceneAdapter Object found. Create.", this.GetType()));
                refObject = new GameObject("SceneAdapter");
            }
            sceneAdapter = refObject.GetComponent<SceneLoader>();
            if (sceneAdapter == null)
            {
                Debug.LogWarning(string.Format("{0}: No SceneAdapter Component found. Create", this.GetType()));
                sceneAdapter = refObject.AddComponent<SceneLoader>();
            }

            // get server adapter
            refObject = GameObject.Find("ServerAdapter");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No ServerAdapter Object found. Create.", this.GetType()));
                refObject = new GameObject("ServerAdapter");
            }
            serverAdapter = refObject.GetComponent<ServerAdapter>();
            if (serverAdapter == null)
            {
                Debug.LogWarning(string.Format("{0}: No ServerAdapter Component found. Create", this.GetType()));
                serverAdapter = refObject.AddComponent<ServerAdapter>();
            }

            // copy cache name to settings
            VPETSettings.Instance.sceneFileName = serverAdapter.sceneFileName;

            // set properties
            serverAdapter.SceneLoader = sceneAdapter;

            // get animation adapter
            refObject = GameObject.Find("AnimationController");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No AnimationController Object found. Create.", this.GetType()));
                refObject = new GameObject("AnimationAdapter");
            }
            animationController = refObject.GetComponent<AnimationController>();
            if (animationController == null)
            {
                Debug.LogWarning(string.Format("{0}: No AnimationController Component found. Create", this.GetType()));
                animationController = refObject.AddComponent<AnimationController>();
            }


            // get JoystickAdapter adapter
            refObject = GameObject.Find("JoystickAdapter");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No JoystickAdapter Object found. Create.", this.GetType()));
                refObject = new GameObject("JoystickAdapter");
            }

#if !SCENE_HOST
            joystickAdapter = refObject.GetComponent<JoystickInput>();
            if (joystickAdapter == null)
            {
                Debug.LogWarning(string.Format("{0}: No JoystickAdapter Component found. Create", this.GetType()));
                joystickAdapter = refObject.AddComponent<JoystickInput>();
            }
#endif

            // get InputAdapter adapter
            refObject = GameObject.Find("InputAdapter");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No InputAdapter Object found. Create.", this.GetType()));
                refObject = new GameObject("InputAdapter");
            }
            inputAdapter = refObject.GetComponent<InputAdapter>();
            if (inputAdapter == null)
            {
                Debug.LogWarning(string.Format("{0}: No InputAdapter Component found. Create", this.GetType()));
                inputAdapter = refObject.AddComponent<InputAdapter>();
            }
            // inputAdapter.MainController = this;


            // get ui adapter
            refObject = GameObject.Find("GUI/Canvas/UI");
            if (refObject == null)
            {
                Debug.LogWarning(string.Format("{0}: No GUI/Canvas/UI Object found. Create.", this.GetType()));
                refObject = new GameObject("UI");
                GameObject refParent = GameObject.Find("GUI/Canvas");
                refObject.transform.SetParent(refParent.transform, false);
            }
            ui = refObject.GetComponent<UI>();
            if (ui == null)
            {
                Debug.LogWarning(string.Format("{0}: No UI Component found. Create", this.GetType()));
                ui = refObject.AddComponent<UI>();
            }

            // get move camera component camera Adapter
            cameraAdapter = Camera.main.GetComponent<MoveCamera>();
            if (cameraAdapter == null)
            {
                Debug.LogWarning(string.Format("{0}: No CameraAdapter Component found. Create", this.GetType()));
                cameraAdapter = Camera.main.gameObject.AddComponent<MoveCamera>();
            }
        }



        //!
        //! Use this for initialization
        //!
        void Start()
        {
            activeMode = Mode.idle;
            helpActive = false;
            arMode = false;
            lockARScale = true;
            lockARRotation = false;
            lockScene = false;

            //find & attach cached GameObjects
            ui = GameObject.Find("UI").GetComponent<UI>();
            scene = GameObject.Find("Scene");

            trsGroup = GameObject.Find("Modifiers");
            translateModifier = GameObject.Find("TranslateModifier");
            rotationModifier = GameObject.Find("RotationModifier");
            scaleModifier = GameObject.Find("ScaleModifier");
            pointToMoveModifier = GameObject.Find("PointToMoveModifier");
            camVisualizer = GameObject.Find("GUI/Canvas/CamVisualizer");

            animationController = GameObject.Find("AnimationController").GetComponent<AnimationController>();

            camObject = GameObject.Find("camera");

            cameraRig = Camera.main.transform.parent;
            print("Camera Rig is: " + cameraRig);

            currentSelection = null;
            currentSceneObject = null;

            // Set ambient light
            setAmbientIntensity(VPETSettings.Instance.ambientLight);


            // Splash Widget
            // Here all starts
            SplashWidget splashWidget = ui.drawSplashWidget();
            splashWidget.OnFinishEvent.AddListener(this.splashFinished);

            GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.layer = LayerMask.NameToLayer("GroundLayer");
            groundPlane.name = "GroundPlane";
            groundPlane.tag = "editable";
            groundPlane.GetComponent<MeshRenderer>().enabled = false;
            groundPlane.transform.parent = scene.transform;
            groundPlane.transform.localPosition = Vector3.zero;
            groundPlane.transform.localScale = new Vector3(5000,5000,5000);
        }
    }
}