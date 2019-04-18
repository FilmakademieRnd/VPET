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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//!
//! This class provides all functionallities for the principal camera
namespace vpet
{
    public class MoveCamera : MonoBehaviour
    {

#if UNITY_STANDALONE_WIN
	    //!
	    //! Setup the connection to the sensors and initalize sensor fusion.
	    //! C style binding to external c++ library to read sensor values on Windows (without deploying it for windows store).
	    //!
	    [DllImport("WinSensorPlugin", EntryPoint = "initalizeSensorReading")]
	    private static extern void initalizeSensorReading();
	
	    //!
	    //! Read sensor fusion value for device orientation based on gyroscope, compass and accelerometer
	    //! C style binding to external c++ library to read sensor values on windows (without deploying it for windows store)
	    //!
	    [DllImport("WinSensorPlugin", EntryPoint="getOrientationSensorData")]
	    private static extern IntPtr getOrientationSensorData();
#endif

        //data reveived from different sensors on Windows or Android
#if UNITY_STANDALONE_WIN
	    //!
	    //! float vector holding the quaternion of the current device orientation when on a windows machine
	    //!
	    float[] orientationSensorData = new float[4];
#endif

        //!
        //! This variable can enable and disable the automatic sensor based camera rotation.
        //!
        private bool move = true;


        //!
        //! Reference to main controller
        //!
        private MainController mainController = null;

        //!
        //! Reference to Scene root
        //!
        private Transform scene;

        //!
        //! Array of objects for which the camera is maintaining their visibility based on the distance to the camera
        //! this can be considered to be a manual near plane clipping implementation for special objects
        //!
        private List<GameObject> supervisedObjects;

        //!
        //! Reference to server adapter to send out changes on execution
        //!
        private ServerAdapter serverAdapter = null;


        //!
        //! Reference to server adapter to send out changes on execution
        //!
        private JoystickInput joystickAdapter = null;


        //!
        //! If a gameObject is attached to the camera, this value will hold the rotation of the object.
        //! It is used to restore the previous rotation after the gameObject has been moved with the camera.
        //!
        Quaternion childrotationBuffer = Quaternion.identity;


        //!
        //! rotation to use when in editor mode. get set from MouseInput
        //!
        private Quaternion rotationEditor;
        public Quaternion RotationEditor
        {
            get { return rotationEditor; }
            set { rotationEditor = value; }
        }


        //settings & variables for frames-per-second display

        //!
        //! update interval for fps display in seconds
        //!
        private float updateInterval = 0.5F;
        //!
        //! enable / disable fps display
        //!
        public bool showFPS = true;
        //!
        //! string to be displayed in fps display, updated regulary
        //!
        private string fpsText = "";
        //!
        //! FPS accumulated over the interval
        //!
        private float accum = 0;
        //!
        //! Frames drawn over the interval
        //!
        private int frames = 0;
        //!
        //! Left time for current interval
        //!
        private float timeleft;


        //smooth translation variables

        //!
        //! slow down factor for smooth translation
        //!
        private float translationDamping = 1.0f;
        //!
        //! final target position of current translation
        //!
        private Vector3 targetTranslation = Vector3.zero;
        //!
        //! enable / disable smooth translation
        //!
        private bool smoothTranslationActive = false;
        //!
        //! Time since the last smooth translation of the camera has been started.
        //! Used to terminate the smooth translation after 3 seconds.
        //!
        private float smoothTranslateTime = 0;


        //update sending parameters

        //!
        //! maximum update interval for server communication in seconds
        //!
        static private float updateIntervall = 1.0f / 30.0f;
        //!
        //! last server update time
        //!
        private float lastUpdateTime = -1;
        //!
        //! position of attached object at last server update
        //! used to track movement
        //!
        private Vector3 lastPosition = Vector3.zero;

        //!
        //! 
        //!
        public bool doApplyRotation = true;

        //!
        //! Lenovo Phab 2 needs flipped axis
        //!
        public bool TangoBuild4LenovoPhab2 = false;

        //!
        //! Current main camera
        //!
        private Camera mainCamera;

#if USE_TANGO || USE_ARKIT
        private Transform trackingTransform;
#endif
        private Transform cameraParent;
        private GyroAdapter gyroAdapter;
        private bool firstApplyTransform = true;
        private Quaternion rotationOffset = Quaternion.identity;
        private Quaternion rotationFirst = Quaternion.identity;
        private Vector3 positionOffset = Vector3.zero;
        private Vector3 positionFirst = Vector3.zero;
        private Vector3 oldPosition = Vector3.zero;
        private Quaternion oldRotation = Quaternion.identity;
        private Vector3 newPosition = Vector3.zero;
        private Quaternion newRotation = Quaternion.identity;

        //! set / get camera field of view (vertical)
        public float Fov
        {
            set { mainCamera.fieldOfView = value; }
            get { return mainCamera.fieldOfView; }
        }

        void Awake()
        {
            gyroAdapter = new GyroAdapter();
            cameraParent = this.transform.parent;
        }

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            timeleft = updateInterval;
            //initialize the sensor reading for the current platform
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
	        initalizeSensorReading();
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
	        // SensorHelper.ActivateRotation();
#endif

            supervisedObjects = new List<GameObject>(0);
            mainCamera = this.GetComponent<Camera>();
            //sync renderInFront camera to mainCamera
            Camera frontCamera = this.transform.GetChild(0).GetComponent<Camera>();
            if (frontCamera)
            {
                frontCamera.fieldOfView = mainCamera.fieldOfView;
                frontCamera.farClipPlane = mainCamera.farClipPlane;
                frontCamera.nearClipPlane = mainCamera.nearClipPlane;
            }

            //sync Outline camera to mainCamera
            if (frontCamera.transform.childCount > 0)
            {
                Camera outlineCamera = frontCamera.transform.GetChild(0).GetComponent<Camera>();
                outlineCamera.fieldOfView = mainCamera.fieldOfView;
                outlineCamera.farClipPlane = mainCamera.farClipPlane;
                outlineCamera.nearClipPlane = mainCamera.nearClipPlane;
            }

            scene = GameObject.Find("Scene").transform;

            // get server adapter
            GameObject refObject = GameObject.Find("ServerAdapter");
            if (refObject != null) serverAdapter = refObject.GetComponent<ServerAdapter>();
            if (serverAdapter == null) Debug.LogError(string.Format("{0}: No ServerAdapter found.", this.GetType()));

            // get joystick adapter
            refObject = GameObject.Find("JoystickAdapter");
            if (refObject != null) joystickAdapter = refObject.GetComponent<JoystickInput>();
            if (joystickAdapter == null) Debug.LogError(string.Format("{0}: No JoystickInput found.", this.GetType()));

            // get mainController
            refObject = GameObject.Find("MainController");
            if (refObject != null) mainController = refObject.GetComponent<MainController>();
            if (mainController == null) Debug.LogError(string.Format("{0}: No MainController found.", this.GetType()));

#if USE_TANGO
            trackingTransform = GameObject.Find("Tango").transform;
#elif USE_ARKIT
            trackingTransform = GameObject.Find("ARKit").transform;
#else
            Camera.main.transform.parent.transform.Rotate(Vector3.right, 90);
#endif
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            mainController.UpdatePropertiesSecondaryCameras();

            //get sensor data from native Plugin on Windows
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
	        Marshal.Copy(getOrientationSensorData(), orientationSensorData, 0, 4);
#endif

            if (move)
            {
                //cache rotation of attached gameObjects
                if (this.transform.childCount > 1)
                {
                    childrotationBuffer = this.transform.GetChild(1).rotation;
                }

#if !UNITY_EDITOR
#if (UNITY_ANDROID || UNITY_IOS)
#if USE_TANGO || USE_ARKIT
                newRotation = trackingTransform.rotation;
                newPosition = trackingTransform.position;
#else
                newRotation = gyroAdapter.Rotation;
#endif
#elif UNITY_STANDALONE_WIN
                newRotation = Quaternion.Euler(90,90,0) * convertRotation(new Quaternion(orientationSensorData[0], orientationSensorData[1], orientationSensorData[2], orientationSensorData[3]));
#endif
#endif
                if (doApplyRotation)
                {
                    if (!firstApplyTransform)
                    {
						if (!mainController.arMode) {
							calibrate (rotationFirst);
							positionOffset = positionFirst - newPosition;
						}
                        firstApplyTransform = true;
                    }
                    //grab sensor reading on current platform
#if !UNITY_EDITOR
	#if (UNITY_ANDROID) && !(USE_TANGO || UNITY_IOS)
                    transform.localRotation = rotationOffset * Quaternion.Euler(0,0,55) * newRotation;
	#elif UNITY_STANDALONE_WIN
                    transform.rotation = rotationOffset * newRotation;
	#else
                    if (TangoBuild4LenovoPhab2)
                        transform.rotation = rotationOffset * new Quaternion (-newRotation.y, newRotation.x, newRotation.z, newRotation.w) ;
                    else
                        transform.rotation = rotationOffset * newRotation;
                    // HACK: to block roll
					if (!mainController.arMode)
                    	transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
                    //transform.rotation *= newRotation * Quaternion.Inverse(oldRotation);
		#if USE_TANGO || USE_ARKIT
                    cameraParent.position += rotationOffset * (newPosition - oldPosition);
		#endif
	#endif
#endif
                }
                else if (firstApplyTransform)
                {
					rotationFirst = rotationOffset * newRotation; 
                    positionFirst = positionOffset + newPosition;
                    firstApplyTransform = false;
                }

#if USE_TANGO || USE_ARKIT
                oldPosition = trackingTransform.position;
                oldRotation = trackingTransform.rotation;
#endif

				if (joystickAdapter) 
				{
                    Vector3 val = joystickAdapter.getTranslation();
                    if (val.magnitude > 0.01)                    
                    {
                        if (joystickAdapter.moveCameraActive && !mainController.arMode)
                            mainController.moveCameraObject(val); 
                        if (joystickAdapter.moveObjectActive)
                            mainController.translateSelectionJoystick(val);
                        if (joystickAdapter.rotateObjectActive)
                            mainController.rotateSelectionJoystick(val);
                        if (joystickAdapter.scaleObjectActive)
                            mainController.scaleSelectionJoystick(val);
                        if (mainController.UIAdapter.LayoutUI == layouts.ANIMATION)
                            mainController.AnimationController.setKeyFrame();
                    }
                    joystickAdapter.getButtonUpdates();
				}
            }

            //smoothly "fly" the camera to a given position
            if (smoothTranslationActive)
            {
                transform.position = Vector3.Lerp(transform.position, targetTranslation, Time.deltaTime * translationDamping);
                //if the position is nearly reached, stop
                if (Vector3.Distance(transform.position, targetTranslation) < 0.0001f)
                {
                    transform.position = targetTranslation;
                    smoothTranslationActive = false;
                }
                //if 3 seconds have past, stop (avoids infinit translation for unreachable points)
                if ((Time.time - smoothTranslateTime) > 3.0f)
                {
                    smoothTranslationActive = false;
                }
            }

            if (mainController.ActiveMode == MainController.Mode.lookThroughCamMode ||
                mainController.ActiveMode == MainController.Mode.lookThroughLightMode)
            {
                Transform currentSelectedTransform = mainController.getCurrentSelection().transform;
                currentSelectedTransform.position = this.transform.position;
                currentSelectedTransform.rotation = this.transform.rotation;
            }

            //calculate & display frames per second
            if (VPETSettings.Instance.debugMsg)
            {
                timeleft -= Time.deltaTime;
                accum += Time.timeScale / Time.deltaTime;
                ++frames;

                // Interval ended - update GUI text and start new interval
                if (timeleft <= 0.0)
                {
                    // display two digits
                    float fps = accum / frames;
                    string format = System.String.Format("{0:F2} FPS", fps);
                    fpsText = format;
                    fpsText += " LiveView IP: " + VPETSettings.Instance.serverIP;
                    fpsText += " State: " + mainController.ActiveMode.ToString();
                    fpsText += " DeviceType: " + SystemInfo.deviceType.ToString();
                    fpsText += " DeviceName: " + SystemInfo.deviceName.ToString();
                    fpsText += " DeviceModel: " + SystemInfo.deviceModel.ToString();
                    fpsText += " SupportGyro: " + SystemInfo.supportsGyroscope.ToString();
                    fpsText += " DataPath: " + Application.dataPath;
                    fpsText += " PersistPath: " + Application.persistentDataPath;
                    fpsText += " Config1: " + Application.dataPath + "/VPET/editing_tool.cfg";
                    fpsText += " Config2: " + Application.persistentDataPath + "/editing_tool.cfg";
                    fpsText += " Mouse Active: " + mainController.MouseInputActive;
                    fpsText += " Touch Active: " + mainController.TouchInputActive;
                    fpsText += " Renderpath:" + Camera.main.renderingPath;
                    fpsText += " ActualRenderpath:" + Camera.main.actualRenderingPath;
                    fpsText += " Msg:" + VPETSettings.Instance.msg;
                    accum = 0.0f;
                    frames = 0;
                    timeleft = updateInterval;
                }
            }
            if(supervisedObjects.Count > 0)
            {
                for(int i = supervisedObjects.Count - 1; i >= 0; i--)
                {
                    if (Vector3.Distance(supervisedObjects[i].transform.position, this.transform.position) > 300.0f * VPETSettings.Instance.sceneScale)
                    {
                        supervisedObjects[i].SetActive(true);
                        supervisedObjects.RemoveAt(i);
                    }
                }
            }

        }

        public void resetCameraOffset()
        {
            rotationOffset = Quaternion.identity;
			cameraParent.position = newPosition;
        }

		public void globalCameraReset()
		{
			lastPosition = Vector3.zero;
			rotationOffset = Quaternion.identity;
			rotationFirst = Quaternion.identity;
			positionOffset = Vector3.zero;
			positionFirst = Vector3.zero;
			oldPosition = Vector3.zero;
			oldRotation = Quaternion.identity;
			newPosition = Vector3.zero;
			newRotation = Quaternion.identity;

			GameObject scene = GameObject.Find("Scene");
			scene.transform.position = Vector3.zero;
		}

        //!
        //! initalize a smooth translation of the camera to a given point
        //! @param    position    world position to send the camera to
        //!
        public void smoothTranslate(Vector3 position)
        {
            smoothTranslateTime = Time.time;
            smoothTranslationActive = true;
            targetTranslation = position;
        }

        //!
        //! converts a quaternion from right handed to left handed system
        //! @param    q    right handed quaternion
        //! @return   left handed quaternion
        //!
        private static Quaternion convertRotation(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        //!
        //! GUI draw call
        //!
        void OnGUI()
        {
            if (VPETSettings.Instance.debugMsg)
            {
                GUI.Label(new Rect(10, 10, 800, 200), fpsText);
            }
        }

        //!
        //! Setter function for the move variable.
        //! Enables / Disables sensor based, automatic camera rotation.
        //! @param    set     sensor based camera movement on/off    
        //!
        public void setMove(bool set)
        {
            move = set;

            // HACK TANGO
            mainController.setTangoActive(set);

        }

        //!
        //!
        //!
        public void registerNearObject(GameObject obj)
        {
            supervisedObjects.Add(obj);
        }
        
        public void calibrate( Quaternion targetRotation )
        {
            if (doApplyRotation)
            {
                rotationOffset = targetRotation * Quaternion.Inverse(newRotation);
            }
            else
            {
                rotationFirst = targetRotation * Quaternion.Inverse(newRotation) * newRotation;
            }
        }
    }
}
