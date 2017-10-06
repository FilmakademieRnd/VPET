/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

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

#if USE_ARKIT
using UnityEngine;
using System.Collections;
using UnityEngine.XR.iOS;
using System;

namespace vpet
{
	public class ARKitController : MonoBehaviour
	{
	    private float m_movementScale = 100.0f;

        //public Camera m_camera;
        private UnityARSessionNativeInterface m_session;
        private Material savedClearMaterial;

        [Header("AR Config Options")]
        public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;
        public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;
        public bool getPointCloud = true;
        public bool enableLightEstimation = true;

        // Use this for initialization
        void Start()
        {
        // Initialize some variables
            
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
            
#if !UNITY_EDITOR
		Application.targetFrameRate = 60;
        ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
		config.planeDetection = planeDetection;
		config.alignment = startAlignment;
		config.getPointCloudData = getPointCloud;
		config.enableLightEstimation = enableLightEstimation;
        m_session.RunWithConfig(config);
#else
            //put some defaults so that it doesnt complain
            UnityARCamera scamera = new UnityARCamera();
            scamera.worldTransform = new UnityARMatrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 projMat = Matrix4x4.Perspective(60.0f, 1.33f, 0.1f, 30.0f);
            scamera.projectionMatrix = new UnityARMatrix4x4(projMat.GetColumn(0), projMat.GetColumn(1), projMat.GetColumn(2), projMat.GetColumn(3));

            UnityARSessionNativeInterface.SetStaticCamera(scamera);
#endif
        }

        // code example for AR camera feed 
        //public void SetCamera(Camera newCamera)
        //{
        //    if (m_camera != null)
        //    {
        //        UnityARVideo oldARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
        //        if (oldARVideo != null)
        //        {
        //            savedClearMaterial = oldARVideo.m_ClearMaterial;
        //            Destroy(oldARVideo);
        //        }
        //    }
        //    SetupNewCamera(newCamera);
        //}

        //private void SetupNewCamera(Camera newCamera)
        //{
        //    m_camera = newCamera;

        //    if (m_camera != null)
        //    {
        //        UnityARVideo unityARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
        //        if (unityARVideo != null)
        //        {
        //            savedClearMaterial = unityARVideo.m_ClearMaterial;
        //            Destroy(unityARVideo);
        //        }
        //        unityARVideo = m_camera.gameObject.AddComponent<UnityARVideo>();
        //        unityARVideo.m_ClearMaterial = savedClearMaterial;
        //    }
        //}

        // Update is called once per frame
        void Update()
        {
            Matrix4x4 matrix = m_session.GetCameraPose();
            transform.position = UnityARMatrixOps.GetPosition(matrix) * m_movementScale;
            transform.rotation = UnityARMatrixOps.GetRotation(matrix);
            
            // example code for AR
            //m_camera.projectionMatrix = m_session.GetCameraProjection();
        }

        public void setTrackingScaleIntensity(float v)
        {
            m_movementScale = 100f * VPETSettings.Instance.sceneScale * v;
        }

        public void scaleMovement( float v )
        {
            m_movementScale *= v;
        }

	}
}
#endif
