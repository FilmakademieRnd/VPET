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

        public Camera m_camera;
        private UnityARSessionNativeInterface m_session;
        private Material savedClearMaterial;

        [Header("AR Config Options")]
        public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;
        public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;
        public ARReferenceImagesSet detectionImages = null;
        public bool getPointCloud = false;
        public bool enableLightEstimation = false;
        public bool enableAutoFocus = true;
        private bool sessionStarted = false;

        [SerializeField]
        private ARReferenceImage referenceImage;

        private GameObject scene;


        /*private void OnEnable()
        {
            // Subscribe event
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
        }

        private void OnDisable()
        {
            // Unsubscribe event
            UnityARSessionNativeInterface.ARFrameUpdatedEvent -= ARFrameUpdated;
        }

        private void ARFrameUpdated(UnityARCamera camera)
        {
            Debug.Log("ARKit State: " + camera.trackingState);
            Debug.Log("ARKit Reason: " + camera.trackingReason);
        }*/

        // Use this for initialization
        void Start()
        {
            // Initialize some variables
    		m_arMode = false;
    		
            m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

            UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
            scene = GameObject.Find("Scene");
            
#if !UNITY_EDITOR
    		Application.targetFrameRate = 60;
            ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
    		config.planeDetection = planeDetection;
    		config.alignment = startAlignment;
    		config.getPointCloudData = getPointCloud;
    		config.enableLightEstimation = enableLightEstimation;
            config.enableAutoFocus = enableAutoFocus;
            if (detectionImages != null) {
                config.arResourceGroupName = detectionImages.resourceGroupName;
            }

            if (config.IsSupported) {
                m_session.RunWithConfig (config);
                UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
            }

            if (m_camera == null) {
                m_camera = Camera.main;
            }

#else
            //put some defaults so that it doesnt complain
            UnityARCamera scamera = new UnityARCamera();
            scamera.worldTransform = new UnityARMatrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 projMat = Matrix4x4.Perspective(60.0f, 1.33f, 0.1f, 30.0f);
            scamera.projectionMatrix = new UnityARMatrix4x4(projMat.GetColumn(0), projMat.GetColumn(1), projMat.GetColumn(2), projMat.GetColumn(3));

            UnityARSessionNativeInterface.SetStaticCamera(scamera);
#endif
        }

        void FirstFrameUpdate(UnityARCamera cam)
        {
            sessionStarted = true;
            UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
        }

        public void SetCamera(Camera newCamera)
        {
            if (m_camera != null)
            {
                UnityARVideo oldARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
                if (oldARVideo != null)
                {
                    savedClearMaterial = oldARVideo.m_ClearMaterial;
                    Destroy(oldARVideo);
                }
            }
            SetupNewCamera(newCamera);
        }

        private void SetupNewCamera(Camera newCamera)
        {
            m_camera = newCamera;

            if (m_camera != null)
            {
                UnityARVideo unityARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
                if (unityARVideo != null)
                {
                    savedClearMaterial = unityARVideo.m_ClearMaterial;
                    Destroy(unityARVideo);
                }
                unityARVideo = m_camera.gameObject.AddComponent<UnityARVideo>();
                unityARVideo.m_ClearMaterial = savedClearMaterial;
            }
        }

        // Update is called once per frame
        void Update()
        {
			//Debug.Log ("movementScale: " + VPETSettings.Instance.trackingScale.ToString());
            Matrix4x4 matrix = m_session.GetCameraPose();
			//transform.localPosition = UnityARMatrixOps.GetPosition (matrix) * VPETSettings.Instance.trackingScale * m_movementScale;
            transform.localPosition = UnityARMatrixOps.GetPosition(matrix) * m_movementScale;
            transform.localRotation = UnityARMatrixOps.GetRotation(matrix);
            if (m_arMode)
            {
                //for AR mode
                Camera.main.projectionMatrix = m_session.GetCameraProjection();
                foreach (Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>())
                {
                   cam.projectionMatrix = Camera.main.projectionMatrix;
                }
            }
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

        void AddImageAnchor(ARImageAnchor arImageAnchor)
        {
            Debug.Log("image anchor added");
            if (arImageAnchor.referenceImageName == referenceImage.imageName)
            {
                if (scene)
                {
                    scene.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);// * VPETSettings.Instance.trackingScale;
                    scene.transform.rotation = Quaternion.Euler(0,UnityARMatrixOps.GetRotation(arImageAnchor.transform).eulerAngles.y,0);
                }
            }
        }

        void UpdateImageAnchor(ARImageAnchor arImageAnchor)
        {
            Debug.Log("image anchor updated");
            if (arImageAnchor.referenceImageName == referenceImage.imageName)
            {
                if (scene)
                {
                    scene.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);// * VPETSettings.Instance.trackingScale;
                    scene.transform.rotation = Quaternion.Euler(0, UnityARMatrixOps.GetRotation(arImageAnchor.transform).eulerAngles.y, 0);
                }
            }
        }

        void OnDestroy()
        {
            UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;

        }

        void RemoveImageAnchor(ARImageAnchor arImageAnchor)
        {
            //currently not used
        }
	}
}
#endif
