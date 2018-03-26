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

#if USE_ARKIT
using UnityEngine;
using System.Collections;
using UnityEngine.XR.iOS;
using System;

namespace vpet
{
	public class ARKitController : MonoBehaviour
	{
		private bool m_arMode = false;
		private float m_movementScale = 1f;

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
		m_arMode = false;
		
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

        // Update is called once per frame
        void Update()
        {
			//Debug.Log ("movementScale: " + VPETSettings.Instance.trackingScale.ToString());
            Matrix4x4 matrix = m_session.GetCameraPose();
			transform.localPosition = UnityARMatrixOps.GetPosition (matrix) * VPETSettings.Instance.trackingScale * m_movementScale;
            transform.localRotation = UnityARMatrixOps.GetRotation(matrix);

			if (m_arMode)
				Camera.main.projectionMatrix = m_session.GetCameraProjection ();   // for AR
        }

   //     public void setTrackingScaleIntensity( float v )
   //     {
			////m_movementScale = v;
			////VPETSettings.Instance.trackingScale = v;  // SEIM: moved to main controller
   //     }

        public void scaleMovement( float v )
        {
            m_movementScale *= v;
        }

		public void setARMode (bool v)
		{
			m_arMode = v;
		}
	}
}
#endif
