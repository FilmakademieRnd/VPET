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

//! @file "ARModule.cs"
//! @brief implementation of VPET AR features
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 04.04.2022

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace vpet
{
    //!
    //! implementation of VPET augmented reality (AR) tracking module
    //!
    public class ARModule : InputManagerModule
    {
        ARSessionOrigin arOrigin;
        ARSession arSession;
        Transform sceneRoot;
        ARTrackedImageManager arImgManager;

        MenuTree _menu;

        //user settings
        Parameter<bool> enableAR;
        Parameter<bool> enableOcclusionMapping;
        Parameter<bool> enableMarkerTracking;

        private bool _arActive;
        public bool arActive { get => _arActive; }

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public ARModule(string name, Manager manager) : base(name, manager)
        {
            arImgManager = null;
            sceneRoot = core.getManager<SceneManager>().scnRoot.transform;
            _arActive = false;
            if (!Application.isEditor)
            {
                GameObject arSessionPrefab = Resources.Load<GameObject>("Prefabs/ARSession");
                arSession = SceneObject.Instantiate(arSessionPrefab, Vector3.zero, Quaternion.identity).GetComponent<ARSession>();

                switch (ARSession.state)
                {
                    case ARSessionState.CheckingAvailability:
                        // still checking AR state
                        Helpers.Log("ARModule: Checking availability...");
                        ARSession.stateChanged += arStateChanged;
                        break;
                    case ARSessionState.Ready:
                    case ARSessionState.SessionInitializing:
                    case ARSessionState.SessionTracking:
                        // AR is available
                        Helpers.Log("ARModule: AR available.");
                        ARSession.stateChanged -= arStateChanged;
                        initialize();
                        break;
                    default:
                        //AR not available
                        Helpers.Log("ARModule: AR not available.");
                        SceneObject.Destroy(arSession);
                        break;
                }
            }
        }

        ~ARModule()
        {
            if(arImgManager != null)
                arImgManager.trackedImagesChanged -= MarkerTrackingChanged;

        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            enableAR = new Parameter<bool>(true, "enableAR");
            enableOcclusionMapping = new Parameter<bool>(true, "enableOcclusion");
            enableMarkerTracking = new Parameter<bool>(true, "enableMarkerTracking");

            _menu = new MenuTree()
                .Begin(MenuItem.IType.VSPLIT)
                    .Begin(MenuItem.IType.HSPLIT)
                         .Add("Enable AR Tracking")
                         .Add(enableAR)
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add("Enable Occlusion Mapping")
                         .Add(enableOcclusionMapping)
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add("Enable Marker Tracking")
                         .Add(enableMarkerTracking)
                     .End()
                .End();

            enableAR.hasChanged += changeActive;
            enableOcclusionMapping.hasChanged += changeOcclusion;
            enableMarkerTracking.hasChanged += changeMarkerTracking;

            _menu.caption = "AR";
            core.getManager<UIManager>().addMenu(_menu);

            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.started += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.performed += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.canceled += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
        }

        //!
        //! function being subscribed to the ARSession.stateChanged when it is yet unknown if the device supports AR
        //!
        private void arStateChanged(ARSessionStateChangedEventArgs obj)
        {
            switch (ARSession.state)
            {
                case ARSessionState.Ready:
                case ARSessionState.SessionInitializing:
                case ARSessionState.SessionTracking:
                    // AR is available
                    Helpers.Log("ARModule: AR available.");
                    ARSession.stateChanged -= arStateChanged;
                    initialize();
                    break;
                default:
                    //AR not available
                    Helpers.Log("ARModule: AR not available.");
                    SceneObject.Destroy(arSession);
                    break;
            }
        }

        private void initialize()
        {
            GameObject arSessionOriginPrefab = Resources.Load<GameObject>("Prefabs/ARSessionOrigin");
            arOrigin = SceneObject.Instantiate(arSessionOriginPrefab, Vector3.zero, Quaternion.identity).GetComponent<ARSessionOrigin>();

            arOrigin.GetComponent<PlaceScene>().scene = core.getManager<SceneManager>().scnRoot;

            arOrigin.transform.parent = Camera.main.transform.parent;
            Camera.main.transform.parent = arOrigin.transform;
            arOrigin.GetComponent<ARSessionOrigin>().camera = Camera.main;

            Camera.main.gameObject.AddComponent<ARPoseDriver>();
            Camera.main.gameObject.AddComponent<ARCameraManager>();
            Camera.main.gameObject.AddComponent<AROcclusionManager>();
            Camera.main.gameObject.AddComponent<ARCameraBackground>();

            //Add Marker Tracking
            arImgManager = arSession.gameObject.AddComponent<ARTrackedImageManager>();
            RuntimeReferenceImageLibrary imgLib = arImgManager.CreateRuntimeLibrary(Resources.Load<XRReferenceImageLibrary>("ReferenceImageLibrary"));
            arImgManager.referenceLibrary = imgLib;
            arImgManager.requestedMaxNumberOfMovingImages = 1;
            arImgManager.trackedImagesChanged += MarkerTrackingChanged;
            arImgManager.enabled = true;
            
            _arActive = true;
        }

        private void MarkerTrackingChanged(ARTrackedImagesChangedEventArgs e)
        {
            foreach (ARTrackedImage newImage in e.added)
            {
                if (newImage.referenceImage.name == "vpetMarker")
                {
                    sceneRoot.position = newImage.transform.position;
                    sceneRoot.rotation = newImage.transform.rotation;
                }
            }

            foreach (ARTrackedImage updatedImage in e.updated)
            {
                if (updatedImage.referenceImage.name == "vpetMarker")
                {
                    sceneRoot.position = updatedImage.transform.position;
                    sceneRoot.rotation = updatedImage.transform.rotation;
                }
            }

            //foreach (ARTrackedImage removedImage in e.removed)
            //{
                // Handle removed event
            //}
        }

        private void changeActive(object sender, bool b)
        {
            manager.m_cameraControl = CameraControl.AR;
            arSession.enabled = b;
        }

        private void changeOcclusion(object sender, bool b)
        {
            Camera.main.gameObject.GetComponent<AROcclusionManager>().enabled = b;
        }

        private void changeMarkerTracking(object sender, bool b)
        {
            Camera.main.gameObject.AddComponent<ARTrackedImageManager>().enabled = b;
        }

        public void moveCamera(Vector3 pos, Quaternion rot)
        {
            arOrigin.MakeContentAppearAt(sceneRoot, pos, rot);
        }
    }
}