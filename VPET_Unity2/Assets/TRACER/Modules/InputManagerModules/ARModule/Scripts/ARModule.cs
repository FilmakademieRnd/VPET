/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
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

//! @file "ARModule.cs"
//! @brief implementation of VPET AR features
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 20.06.2023

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.XR;

namespace vpet
{
    //!
    //! implementation of VPET augmented reality (AR) tracking module
    //!
    public class ARModule : InputManagerModule
    {
        //!
        //! The origin of the AR Tracking (parent of main camera)
        //!
        private XROrigin m_arOrigin;
        //!
        //! The session of the tracking
        //!
        private ARSession arSession;
        //!
        //! The AR camera manager (component of main camera)
        //!
        private ARCameraManager m_camManager;
        //!
        //! The AR pose driver actually moving the camera (component of main camera)
        //!
        private TrackedPoseDriver m_poseDriver;
        //!
        //! The camera background used for displaying the real world camera image
        //!
        private ARCameraBackground m_cameraBg;
        //!
        //! The AR occlusion manager, adding depth based occlusions if available and activated (component of main camera)
        //!
        private AROcclusionManager m_occlusionManager;
        //!
        //! the root of the scene
        //!
        private Transform sceneRoot;
        //!
        //! The AR image manager, holding the library of tracked markers (component of main camera)
        //!
        private ARTrackedImageManager arImgManager;

        //!
        //! UI menu for AR
        //!
        private MenuTree _menu;
        //!
        //! user setting available in the UI
        //! enable / disable AR tracking
        //!
        private Parameter<bool> enableAR;
        //!
        //! user setting available in the UI
        //! enable / disable depth based occlusion mapping
        //!
        private Parameter<bool> enableOcclusionMapping;
        //!
        //! user setting available in the UI
        //! enable / disable marker tracking
        //!
        private Parameter<bool> enableMarkerTracking;

        //!
        //! is AR setup correctly and tracking in place
        //!
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

        //!
        //! Destructor
        //!
        ~ARModule()
        {
            if (arImgManager != null)
                arImgManager.trackedImagesChanged -= MarkerTrackingChanged;

        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor.
        //! @param sender event sender
        //! @param e event arguments
        //!
        protected override void Init(object sender, EventArgs e)
        {
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.started += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.performed += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.canceled += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
        }

        //!
        //! function being subscribed to the ARSession.stateChanged when it is yet unknown if the device supports AR
        //! @param obj unused additional information given by ARSession
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
                    //changeActive(this, true);
                    break;
                default:
                    //AR not available
                    Helpers.Log("ARModule: AR not available.");
                    changeActive(this, false);
                    SceneObject.Destroy(arSession);
                    break;
            }
        }

        //!
        //! initialize the AR environement in the scene, executed after veryfing AR support of device
        //!
        private void initialize()
        {
            //Instanciate XROrigin from Prefab
            GameObject arSessionOriginPrefab = Resources.Load<GameObject>("Prefabs/ARSessionOrigin");
            m_arOrigin = SceneObject.Instantiate(arSessionOriginPrefab, Vector3.zero, Quaternion.identity).GetComponent<XROrigin>();

            //m_arOrigin.GetComponent<PlaceScene>().scene = core.getManager<SceneManager>().scnRoot;

            //place XROrigin as parent of main camera
            m_arOrigin.transform.parent = Camera.main.transform.parent;
            Camera.main.transform.parent = m_arOrigin.transform;
            m_arOrigin.GetComponent<XROrigin>().Camera = Camera.main;
            m_arOrigin.GetComponent<XROrigin>().CameraFloorOffsetObject = sceneRoot.gameObject;

            //add required components to camera
            Camera.main.gameObject.GetComponent<TrackedPoseDriver>().enabled = true;
            m_poseDriver = Camera.main.gameObject.GetComponent<TrackedPoseDriver>();
            m_camManager = Camera.main.gameObject.AddComponent<ARCameraManager>();
            m_occlusionManager = Camera.main.gameObject.AddComponent<AROcclusionManager>();
            m_occlusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Best;
            m_cameraBg = Camera.main.gameObject.AddComponent<ARCameraBackground>();

            //Add Marker Tracking
            arImgManager = arSession.gameObject.AddComponent<ARTrackedImageManager>();
            RuntimeReferenceImageLibrary imgLib = arImgManager.CreateRuntimeLibrary(Resources.Load<XRReferenceImageLibrary>("ReferenceImageLibrary"));
            arImgManager.referenceLibrary = imgLib;
            arImgManager.requestedMaxNumberOfMovingImages = 1;
            arImgManager.trackedImagesChanged += MarkerTrackingChanged;
            arImgManager.enabled = true;

            //prepare UI
            enableAR = new Parameter<bool>(false, "enableAR");
            enableOcclusionMapping = new Parameter<bool>(false, "enableOcclusion");
            enableMarkerTracking = new Parameter<bool>(false, "enableMarkerTracking");

            _menu = new MenuTree()
                .Begin(MenuItem.IType.VSPLIT)
                    .Begin(MenuItem.IType.HSPLIT)
                         .Add("AR Tracking")
                         .Add(enableAR)
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add("Occlusion Mapping")
                         .Add(enableOcclusionMapping)
                     .End()
                     .Begin(MenuItem.IType.HSPLIT)
                         .Add("Marker Tracking")
                         .Add(enableMarkerTracking)
                     .End()
                .End();
            _menu.caption = "AR";
            _menu.iconResourceLocation = "Images/button_ar";
            core.getManager<UIManager>().addMenu(_menu);

            //connect events
            enableAR.hasChanged += changeActive;
            enableOcclusionMapping.hasChanged += changeOcclusion;
            enableMarkerTracking.hasChanged += changeMarkerTracking;

            //initialise state
            changeActive(this, false);
            changeOcclusion(this, false);
            changeMarkerTracking(this, false);

            //enable AR tracking
            _arActive = true;
        }

        //!
        //! handle updates to marker tracking (added, updated, removed in camera view), triggered by ARImageManager
        //!
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

        //!
        //! event handling changes to the AR tracking state (active/inactive) through user input / UI
        //! @param sender event sender
        //! @param e event arguments
        //!
        private void changeActive(object sender, bool b)
        {
            if (b)
            {
                Camera.main.transform.parent = m_arOrigin.transform;
                manager.disableAttitudeSensor();
                manager.cameraControl = InputManager.CameraControl.AR;
            }
            else
            {
                manager.cameraControl = InputManager.CameraControl.NONE;
                Camera.main.transform.parent = m_arOrigin.transform.parent;
                m_arOrigin.transform.position = Vector3.zero;
                m_arOrigin.transform.rotation = Quaternion.identity;
                manager.enableAttitudeSensor();
                manager.setCameraAttitudeOffsets();
            }
            if (arSession)
                arSession.enabled = b;
            if (m_cameraBg)
                m_cameraBg.enabled = b;
            if (m_camManager)
                m_cameraBg.enabled = b;
            if (m_poseDriver)
                m_poseDriver.enabled = b;
            if (m_occlusionManager && enableOcclusionMapping != null)
                if (b) m_occlusionManager.enabled = enableOcclusionMapping.value;
                else m_occlusionManager.enabled = false;

            if (b)
            {
                m_arOrigin.transform.position = Camera.main.transform.position;
                m_arOrigin.transform.rotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, new Vector3(0, 1f, 0));
            }

        }

        //!
        //! event handling changes to the AR occlusion mapping (active/inactive) through user input / UI
        //! @param sender event sender
        //! @param e event arguments
        //!
        private void changeOcclusion(object sender, bool b)
        {
            if (b)
                m_occlusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Best;
            else
                m_occlusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Disabled;
        }

        //!
        //! event handling changes to the AR marker tracking (active/inactive) through user input / UI
        //! @param sender event sender
        //! @param e event arguments
        //!
        private void changeMarkerTracking(object sender, bool b)
        {
            Camera.main.gameObject.AddComponent<ARTrackedImageManager>().enabled = b;
        }

        //!
        //! function to move the camera manually (in addition to tracking)
        //! @param pos position th apply to the camera
        //! @param rot rotation to apply to the camera
        //!
        public void moveCamera(Vector3 pos, Quaternion rot)
        {
            XROriginExtensions.MakeContentAppearAt(m_arOrigin, sceneRoot, pos, rot);
        }
    }

    //!
    //! extension class to XROrigin to reimplement MakeContentAppearAt function
    //!
    public static class XROriginExtensions
    {
        //!
        //! function to move XROrigin so that scene appears at given location relative to camera
        //! @param origin XROrigin the location offset is applied to
        //! @param content scene to be moved
        //! @param position position the scene shall be moved to
        //! @param rotation rotation the scene shall be rotated to
        //!
        public static void MakeContentAppearAt(this XROrigin origin, Transform content, Vector3 position, Quaternion rotation)
        {
            MakeContentAppearAt(origin, content, position);
            MakeContentAppearAt(origin, content, rotation);
        }

        //!
        //! function to reposition XROrigin so that scene appears at given location relative to camera
        //! @param origin XROrigin the location offset is applied to
        //! @param content scene to be moved
        //! @param position position the scene shall be moved to
        //!
        private static void MakeContentAppearAt(this XROrigin origin, Transform content, Vector3 position)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var originTransform = origin.transform;

            origin.CameraFloorOffsetObject.transform.position += originTransform.position - position;

            originTransform.position = content.position;
        }

        //!
        //! function to rotate XROrigin so that scene appears at intended rotation
        //! @param origin XROrigin the location offset is applied to
        //! @param content scene to be moved
        //! @param rotation rotation the scene shall be rotated to
        //!
        private static void MakeContentAppearAt(this XROrigin origin, Transform content, Quaternion rotation)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            origin.transform.rotation = Quaternion.Inverse(rotation) * content.rotation;
        }
    }
}