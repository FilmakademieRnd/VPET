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
using UnityEngine.Events;
using System.Reflection;
#if USE_AR
using UnityEngine.XR.ARFoundation;
#endif

//!
//! INFO: the mainController class is separeted into different files
//! find all parts in the files named like MainController.XXX
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour
    {


        public Material matteMaterial;

        public GameObject arCoverSphere;

        //is AR currently used
		public bool arMode;
        //is AR currently setup (placing scene in camera view)
        public bool arSetupMode;

        public bool lockARRotation;
        public bool lockARScale;


#if USE_AR
		private GameObject m_anchorModifier = null;
		private GameObject m_anchorPrefab = null;
#endif

        //!
        //! move the camera relative to last position
        //! @param      translation     new position relative to old one
        //!
        public void moveCameraObject(Vector3 translation)
        {
            Vector3 camSpaceTranslation = translation * VPETSettings.Instance.controllerSpeed * VPETSettings.Instance.sceneScale * (VPETSettings.Instance.maxExtend / 3000f);
#if USE_AR //&& !UNITY_EDITOR
            if (!arMode)
            {
                GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform,Camera.main.transform.rotation * -camSpaceTranslation);
            }
#else
            Camera.main.transform.Translate(camSpaceTranslation);
#endif
        }

        //!
        //! move the camera's relative to last position
        //! @param      translation     new position relative to old one
        //!
        public void moveCameraParent( Vector3 translation )
	    {
            cameraRig.Translate(translation * VPETSettings.Instance.controllerSpeed / VPETSettings.Instance.maxExtend);
	    }

        //!
        //! switch wether a standalone windows version will recive klicks or touch events
        //!
        public bool standaloneReceivesClicks = false;

	
	    //!
	    //! open light menu when currentSelection is a light otherwise open the objectMenu
	    //! is normally called when an editing mode is terminated but the object remains selected
	    //!
	    public void openMenu()
	    {
			if(currentSelection)
	        {
                if (currentSceneObject.GetType() == typeof(SceneObjectLight))
                {
                    activeMode = Mode.lightMenuMode;
                }
                else if (currentSceneObject.GetType() == typeof(SceneObjectCamera))
                {
                    if (currentSceneObject.locked)
                        activeMode = Mode.cameraLockedMode;
                    else
                        activeMode = Mode.cameraMenuMode;
                }
                else
				{
                    activeMode = Mode.objectMenuMode;
                }
	        }
	    }
	
	
	    //!
	    //! get the position of the current selection if something is selected
	    //! @return absolut position of selection, if nothing selected (0,0,0)
	    //!
	    public Vector3 getSelectionPosition()
	    {
	        if(currentSelection)
                return currentSelection.position;
            else
	            return Vector3.zero;
	    }

        //!
        //! unselect the given object if it is selected (is currentSelection)
        //! @param      obj     obj to check against
        //!
        public void unselectIfSelected(Transform obj)
	    {
	        if (currentSelection == obj)
	            activeMode = Mode.idle;
	    }
	
	    //!
	    //! resets all elements to initial positions
	    //!
	    public void globalReset()
	    {
	        recursiveResetChilds(scene.transform);
	    }
	
	    //!
	    //! recursively reset all childs
	    //! @param      obj     target transform
	    //!
	    private void recursiveResetChilds(Transform obj)
	    {
	        for(int i = 0; i < obj.childCount; i++)
	        {
                SceneObject sco = obj.GetChild(i).GetComponent<SceneObject>();
                SceneObject scl = obj.GetChild(i).GetComponent<SceneObjectLight>();
                SceneObject scc = obj.GetChild(i).GetComponent<SceneObjectCamera>();

                if (sco && obj.GetChild(i).gameObject.activeSelf)
	            {
                    if (obj.GetChild(i).GetComponent<SceneObject>().GetType() == typeof(SceneObjectLight))
                        obj.GetChild(i).GetComponent<SceneObjectLight>().resetAll();
                    else
                        obj.GetChild(i).GetComponent<SceneObject>().resetAll();
	            }
	            recursiveResetChilds(obj.GetChild(i));
	        }
	    }
	
		//!
		//! turn on/off ncam synchronisation
		//!
		public void toggleNcam()
		{
			bool isOn = serverAdapter.receiveNcam;
			serverAdapter.receiveNcam = !isOn;
			camObject.GetComponent<Renderer>().enabled = isOn;
			cameraAdapter.setMove( isOn );

			VPETSettings.Instance.msg = "Ncam set: " + serverAdapter.receiveNcam.ToString();
		}
	
	    //!
	    //! toggle ignore rotation
	    //!
	    public void toggleCameraRotation()
	    {
	        cameraAdapter.doApplyRotation = !cameraAdapter.doApplyRotation;
#if USE_AR
            if (!arMode)
                GameObject.Find("ARSession").GetComponent<ARSession>().enabled = cameraAdapter.doApplyRotation;
            if (cameraAdapter.doApplyRotation)
            {
                GameObject camObject = SceneLoader.SceneCameraList[(camPrefabPosition-1) % SceneLoader.SceneCameraList.Count];

                scene.transform.position = Vector3.zero;
                scene.transform.rotation = Quaternion.identity;
                GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, Camera.main.transform.position);
                Quaternion newRotation = Camera.main.transform.rotation * Quaternion.Inverse(camObject.transform.rotation);
                GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, newRotation);
                Vector3 newPosition = -(camObject.transform.position + Camera.main.transform.position);
                GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, newPosition);
            }
#endif
        }
	
	
		//!
		//! set camera to perspective if is orthografic
		//!
		public void setPerspectiveCamera()
		{
			// switch off ncam if on
			if ( serverAdapter.receiveNcam ) toggleNcam();

			if (Camera.main.orthographic == true)
			{
                Camera.main.orthographic = false;
                Camera.main.renderingPath = RenderingPath.UsePlayerSettings;

                UpdatePropertiesSecondaryCameras();
                cameraAdapter.setMove(true);
				currentCameraView = View.PERSP;
            }
#if USE_AR
            GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, -camPreviousPosition, camPreviousRotation);
#else
            // restore previous view, e.g. when returning from orthographic view
            Camera.main.transform.position = camPreviousPosition;
			Camera.main.transform.rotation = camPreviousRotation;
#endif
        }

        public void resetCameraOffset()
        {
            cameraAdapter.resetCameraOffset();
        }

        //!
        //! reposition the camera to predefined positions
        //!
        public void repositionCamera()
        {
            if (!arMode && activeMode != Mode.lookThroughLightMode)
            {
                setPerspectiveCamera();
                if (SceneLoader.SceneCameraList.Count > 0)
                {
                    GameObject camObject = SceneLoader.SceneCameraList[camPrefabPosition];
                    SceneObjectCamera soc = camObject.GetComponent<SceneObjectCamera>();
                    GameObject camGeometry = camObject.transform.GetChild(0).gameObject;
                    camGeometry.SetActive(false);
                    cameraAdapter.registerNearObject(camGeometry);
#if USE_AR
                    scene.transform.position = Vector3.zero;
                    scene.transform.rotation = Quaternion.identity;
                    GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, Camera.main.transform.position);
                    Quaternion newRotation = Camera.main.transform.rotation * Quaternion.Inverse(camObject.transform.rotation);
                    GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, newRotation);
                    Vector3 newPosition = -(camObject.transform.position + Camera.main.transform.position);
                    GameObject.Find("Cameras").GetComponent<ARSessionOrigin>().MakeContentAppearAt(scene.transform, newPosition);
                    Camera.main.nearClipPlane = 0.01f;
                    Camera.main.farClipPlane = soc.far * 2f;
                    Camera.main.fieldOfView = soc.fov;
#else
                    Camera.main.transform.position = camObject.transform.position; 
                    Camera.main.transform.rotation = camObject.transform.rotation;          
                    Camera.main.nearClipPlane = 0.01f;
                    Camera.main.farClipPlane = soc.far * 2f;
                    Camera.main.fieldOfView = soc.fov;

                    // callibrate 
                    cameraAdapter.calibrate(camObject.transform.rotation);
#endif
                    // set camera properties
                    CameraObject camScript = camObject.GetComponent<CameraObject>();
                    if (camScript != null)
                    {
                        Camera.main.fieldOfView = camScript.fov; //.hFovToVFov(); // convert horizontal fov from Katana to vertical
                                                                 //Camera.main.nearClipPlane = camScript.near * VPETSettings.Instance.sceneScale;
                                                                 //Camera.main.farClipPlane = camScript.far * VPETSettings.Instance.sceneScale;
                        UpdatePropertiesSecondaryCameras();
                    }
                }
                if (SceneLoader.SceneCameraList.Count == 0)
                    camPrefabPosition = 0;
                else
                    camPrefabPosition = (camPrefabPosition + 1) % SceneLoader.SceneCameraList.Count;
            }
        }

        //!
        //! getter function for the current selection
        //! @return     reference to the currently selected object
        //!
        public Transform getCurrentSelection()
	    {
	        return currentSelection;
	    }
	
	    //!
	    //! switches the camera to orthographic view
	    //!
	    public void setOrthographicCamera( View view )
	    {
			// save current view
			camPreviousPosition = Camera.main.transform.position;
			camPreviousRotation = Camera.main.transform.rotation;

			// switch off ncam if on
			if ( serverAdapter.receiveNcam ) toggleNcam();

            Camera.main.renderingPath = RenderingPath.VertexLit;

	        Camera.main.orthographic = true;
            UpdatePropertiesSecondaryCameras();

            cameraAdapter.setMove(false);
			currentCameraView = view;

	        switch ( view )
	        {
	            case View.TOP:
	                Camera.main.transform.rotation = Quaternion.Euler( new Vector3(90f, 180f, 0f) );
					Camera.main.transform.position = new Vector3(-15f, 10f, 5f) + VPETSettings.Instance.cameraOffsetTop;
	                break;
	            case View.FRONT:
	                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
					Camera.main.transform.position = new Vector3(-15f, 5f, 14f) + VPETSettings.Instance.cameraOffsetFront;
                    break;
	            case View.RIGHT:
                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
					Camera.main.transform.position = new Vector3(-22.9f, 5f, 4.7f) + VPETSettings.Instance.cameraOffsetRight;
                    break;
	            case View.LEFT:
                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
					Camera.main.transform.position = new Vector3(-7.4f, 5f, 4.7f) + VPETSettings.Instance.cameraOffsetLeft;                    
	                break;
	            case View.BOTTOM:
	                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(270f, 0f, 0f));
					Camera.main.transform.position = new Vector3(-15f, -1.88f, 2.24f) + VPETSettings.Instance.cameraOffsetBottom;
	                break;
	        }
	    }
		//!
		//! calculate the camera positional offset and store in user preferences
		//!
		public void saveCameraOffset()
		{
			switch ( currentCameraView )
			{
				case View.PERSP:
					// nothing
					break;
				case View.TOP:
					VPETSettings.Instance.cameraOffsetTop = Camera.main.transform.position - new Vector3(-15f, 10f, 5f);
					break;
				case View.FRONT:
					VPETSettings.Instance.cameraOffsetFront = Camera.main.transform.position - new Vector3(-15f, 5f, 14f);
					break;
				case View.RIGHT:
					VPETSettings.Instance.cameraOffsetRight = Camera.main.transform.position - new Vector3(-22.9f, 5f, 4.7f);
					break;
				case View.LEFT:
					VPETSettings.Instance.cameraOffsetLeft = Camera.main.transform.position - new Vector3(-7.4f, 5f, 4.7f);
					break;
				case View.BOTTOM:
					VPETSettings.Instance.cameraOffsetLeft = Camera.main.transform.position - new Vector3(-15f, -1.88f, 2.24f);
					break;
			}

			VPETSettings.mapValuesToPreferences();

		}
	

        private void splashFinished()
        {
            // draw main menu 
            ui.drawMainMenuButton(true);

            // set values in config widget according to vpet setting
			ConfigWidget configWidget = ui.drawConfigWidget();
			VPETSettings.mapValuesToObject(configWidget);
            configWidget.initConfigWidget();

        }


        //!
        //! Handle submited config widget
        //! This triggers the server adapter to request a scene
        //! @param widget     source widget
        //!
        public void configWidgetSubmit(ConfigWidget widget)
	    {

			ui.hideConfigWidget();

            ui.switchLayoutMainMenu(layouts.DEFAULT);

            ui.resetMainMenu();

			TouchInput input = inputAdapter.GetComponent<TouchInput>();
			if (input)
				input.enabled = true;

			hideARWidgets();

            // request progress bar from UI and connect listener to server adapter
            RoundProgressBar progressWidget =  ui.drawProgressWidget();
			// Connect set value to progress event
			serverAdapter.OnProgressEvent.AddListener(progressWidget.SetValue);
			// Connect scene load complete to progress finish event
			progressWidget.OnFinishEvent.AddListener(this.sceneLoadCompleted);

            // init server adapter
            serverAdapter.initServerAdapterTransfer();
		}

        //!
        //! hide all AR related widgets like ARPlanes and the anchor
        //!
        public void hideARWidgets()
        {
#if USE_AR
            GameObject arPlanes = GameObject.Find("ARPlanes");
            if (arPlanes)
            {
                GameObject.Destroy(arPlanes.GetComponent<ARPlane>());
            }

            // disable anchor visualisation
            if (m_anchorModifier)
            {
                m_anchorModifier.SetActive(false);
                GameObject.Destroy(m_anchorModifier);
                m_anchorModifier = null;
            }
#endif
        }

        //!
        //! Handle after the scene has been transfered and loaded into the engine
        //! @param widget     source widget
        //!
        public void sceneLoadCompleted()
	    {
			ui.hideProgressWidget();
		}

		//
		public void setAmbientIntensity( float v )
		{
			RenderSettings.ambientIntensity = v;
            RenderSettings.ambientLight = new Color(v, v, v, 1f);
		}

        public void toggleLockRotation()
        {
            lockARRotation = !lockARRotation;
            ui.changeARLockRotationButtonImage(lockARRotation);
        }

        public void toggleLockScale()
        {
            lockARScale = !lockARScale;
            ui.changeARLockScaleButtonImage(lockARScale);
        }

        //! function called when user selects/deselects AR Tobble in ConfigWidget
        public void ToggleArMode(bool active)
        {
			arMode = active;

			ui.setupSecondaryMenu ();
#if USE_AR
			GameObject root = GameObject.Find("Scene");
			//TouchInput input = inputAdapter.GetComponent<TouchInput>();
			GameObject arConfigWidget = GameObject.Find("GUI/Canvas/ARConfigWidget");
			GameObject rootScene = SceneLoader.scnRoot;
            GameObject cameraParent = GameObject.Find("Cameras");
            GameObject arSession = GameObject.Find("ARSession");

            //arFoundation.GetComponent<ARSessionOrigin>().enabled = active;
            //arFoundation.GetComponent<ARRaycastManager>().enabled = active;
            //arFoundation.GetComponent<ARAnchorManager>().enabled = active;
            //arFoundation.GetComponent<ARTrackedImageManager>().enabled = active;
            //arSession.GetComponent<ARSession>().enabled = active;
            //arSession.GetComponent<ARInputManager>().enabled = active;

            if (m_anchorPrefab == null)
				m_anchorPrefab = Resources.Load ("VPET/Prefabs/AnchorModifier", typeof(GameObject)) as GameObject;

			//if (input)
 			//	input.enabled = !active;
				
#endif
            if (active)
            {
#if USE_AR
                //avoids object updates while placing, scaling, rotating scene in AR setup
                lockScene = true;

                // enable video background
                cameraAdapter.transform.GetComponent<ARCameraBackground>().enabled = true;

				// create anchor modifier
				if (m_anchorModifier == null) 
				{
					m_anchorModifier = GameObject.Instantiate(m_anchorPrefab);
                    m_anchorModifier.transform.position = Vector3.zero;
                    m_anchorModifier.layer = LayerMask.NameToLayer("RenderInFront");
                    foreach (Transform child in m_anchorModifier.transform)
                    {
                        child.gameObject.layer = 8;
                    }
                    m_anchorModifier.transform.localScale = Vector3.zero;
                    m_anchorModifier.name = "ARModifier";
					if (root)
						m_anchorModifier.transform.SetParent(root.transform, false);
				}

                //hide config widget
                ui.hideConfigWidget();

				//hide scene while placing AR anchor
				//rootScene.SetActive(false);

                //show AR config widget
				arConfigWidget.SetActive(true);
                arSetupMode = true;

                //reset scene scale
                SetSceneScale(VPETSettings.Instance.sceneScale);

                //initalize ar lock buttons
                ui.changeARLockRotationButtonImage(lockARRotation);
                ui.changeARLockScaleButtonImage(lockARScale);
#endif
            }
            else
            {
#if USE_AR
                // destroy video background
                cameraAdapter.transform.GetComponent<ARCameraBackground>().enabled = false;

				// disable anchor visualisation
				if (m_anchorModifier) {
					m_anchorModifier.SetActive(false);
					GameObject.Destroy(m_anchorModifier);
					m_anchorModifier = null;
				}


                //free scene from AR anchor
                cameraParent.GetComponent<ARFoundationController>().removeAnchor();
#endif

                SetSceneScale(1f);
                repositionCamera();

                sceneAdapter.ShowGeometry();

            }

            hasUpdatedProjectionMatrix = true;

        }

#if USE_AR
		public void ToggleARKeyMode(bool active)
		{
            if (active)
            {
                Camera.main.transform.GetComponent<ARCameraBackground>().enabled = false;
                Camera.main.transform.GetChild(0).GetComponent<ARCameraBackground>().enabled = true;
            }
            else
            {
                Camera.main.transform.GetChild(0).GetComponent<ARCameraBackground>().enabled = false;
                Camera.main.transform.GetComponent<ARCameraBackground>().enabled = true;
            }
            //Shader shader = Shader.Find("VPET/ARCameraShader");
            //if (active)
            //	shader = Shader.Find("VPET/ARCameraShaderChromaKey");
            //GameObject.Find("RenderInFrontCamera").GetComponent<ARCameraBackground>().customMaterial.shader = shader;
        }
#endif



#if USE_AR
        public void ToggleARMatteMode(bool active)
        {
            Material camMaterial = cameraAdapter.transform.GetComponent<ARCameraBackground>().material;

            if (arCoverSphere != null)
                arCoverSphere.SetActive(active);

            if (active)
            {
                if (matteMaterial != null)
                {
                    matteMaterial.SetTexture("_textureY", camMaterial.GetTexture("_textureY"));
                    matteMaterial.SetTexture("_textureCbCr", camMaterial.GetTexture("_textureCbCr"));
                    matteMaterial.SetMatrix("_DisplayTransform", camMaterial.GetMatrix("_DisplayTransform"));

                    float displayRatio = Camera.main.aspect;  //2.1653
                    float videoRatio = 16.0f / 9.0f;
                    if (camMaterial)
                        videoRatio = (float)camMaterial.GetTexture("_textureY").width / (float)camMaterial.GetTexture("_textureY").height;
                    float cropScaleHorizontal = 0, cropScaleVertical = 0;

                    if (displayRatio < videoRatio)
                    {
                        cropScaleHorizontal = 1.0f - (displayRatio / videoRatio);
                        cropScaleVertical = 0.0f;
                    }
                    else
                    {
                        cropScaleHorizontal = 0.0f;
                        cropScaleVertical = 1.0f - ( videoRatio / displayRatio);
                    }

                    matteMaterial.SetFloat("_cropScaleX", cropScaleHorizontal);
                    matteMaterial.SetFloat("_cropScaleY", cropScaleVertical);
                }
            }
            else
            {
                if (matteMaterial != null)
                {
                    matteMaterial.SetTexture("_textureY", null);
                    matteMaterial.SetTexture("_textureCbCr", null);
                    // matteMaterial.SetMatrix("_DisplayTransform", arkitScreen.m_ClearMaterial.GetMatrix("_DisplayTransform"));
                }
            }


        }
#endif


        private void UpdateProjectionMatrixSecondaryCameras()
        {
            foreach( Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>() )
            {
                cam.projectionMatrix = Camera.main.projectionMatrix;
            }
        }

        public void UpdatePropertiesSecondaryCameras()
        {
            foreach (Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>())
            {
                cam.orthographic = Camera.main.orthographic;
                cam.fieldOfView = Camera.main.fieldOfView;
                cam.nearClipPlane =  Camera.main.nearClipPlane;
                cam.farClipPlane = Camera.main.farClipPlane;
                cam.projectionMatrix = Camera.main.projectionMatrix;
            }
        }

        public bool HasGravityOn()
        {
            if ( currentSelection && currentSceneObject)
            {
                return !currentSceneObject.globalKinematic;
            }
            return false;
        }


        public void UpdateRangeSliderValue()
        {
            ui.updateRangeSlider((float)rangeSliderInfo.GetValue(rangeSliderInfoObj, null));
        }

        //!
        //! wire method and set as callback 
        //! @param
        //!
        public void ConnectRangeSlider( UnityEngine.Object obj, string prop, float sensitivity = 0.1f )
        {
            rangeSliderInfoObj = obj;
            rangeSliderInfo = rangeSliderInfoObj.GetType().GetProperty(prop);
            UnityAction<float> act = (UnityAction <float>) UnityAction.CreateDelegate( typeof(UnityAction<float>), rangeSliderInfoObj, rangeSliderInfo.GetSetMethod());
            ui.drawRangeSlider(act, (float)rangeSliderInfo.GetValue(rangeSliderInfoObj, null), sensitivity);
        }

        //!
        //! wire method and set as callback 
        //! @param
        //!
        public void ConnectRangeSlider(UnityAction<float> act, float initValue, float sensitivity = 0.1f)
        {
            ui.drawRangeSlider(act, initValue, sensitivity);
        }

        //!
        //! set the global scale of the scene
        //! @Param v absolute scale
        //!
        public void SetSceneScale(float v)
        {
            v = Mathf.Max(v,0.0001f);
            // scale the scene
            //SceneLoader.scnRoot.transform.localScale = new Vector3(v, v, v);
            GameObject.Find("Cameras").transform.GetChild(0).localScale = new Vector3(1f / v, 1f / v, 1f / v);

            //float sceneScalePrevious = VPETSettings.Instance.sceneScale;
            VPETSettings.Instance.sceneScale = v;

            // update light params
            //foreach (GameObject obj in SceneLoader.SelectableLights)
            //{
            //    float lightRange = obj.GetComponent<SceneObjectLight>().getLightRange() / sceneScalePrevious * v;
            //    obj.GetComponent<SceneObjectLight>().setLightRange(lightRange);
            //    //obj.GetComponent<SceneObject>().SourceLight.transform.localScale = Vector3.one / VPETSettings.Instance.sceneScale;
            //    obj.GetComponentInChildren<LightIcon>().TargetScale = obj.transform.lossyScale;
            //}

            ui.updateScaleValue(v);
            Vector3 sceneExtends = VPETSettings.Instance.sceneBoundsMax - VPETSettings.Instance.sceneBoundsMin;
            float maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);
            //QualitySettings.shadowDistance = v * maxExtend * 0.15f;
            Camera.main.nearClipPlane = Mathf.Max(0.01f, Vector3.Distance(Camera.main.transform.position, scene.transform.position) - maxExtend*3f);
            Camera.main.farClipPlane = Mathf.Max(1000f,Mathf.Min(100000f, Vector3.Distance(Camera.main.transform.position, scene.transform.position) + maxExtend*3f));
            //Physics.gravity = new Vector3(0, -0.24525f * VPETSettings.Instance.sceneScale * maxExtend, 0);

            /*foreach (Rigidbody rigi in FindObjectsOfType<Rigidbody>())
            {
                rigi.mass = 0.1f * maxExtend * v;
            }*/

            //VPETSettings.Instance.maxExtend = maxExtend;
            // update camera params
            //UpdatePropertiesSecondaryCameras();
        }


        public void SliderValueChanged( float x )
        {
            // set keyframe
            if ( ui.LayoutUI == layouts.ANIMATION )
            {
                animationController.setKeyFrame();
            }
        }

#if USE_AR
		public void setARKeyDepth(float v)
		{
            GameObject.Find("RenderInFrontCamera").GetComponent<ARCameraBackground>().customMaterial.SetFloat("_Depth", v);
		}

		public void setARKeyColor(Color c)
		{
            GameObject.Find("RenderInFrontCamera").GetComponent<ARCameraBackground>().customMaterial.SetColor("_KeyColor", c);

            if (matteMaterial)
                matteMaterial.SetColor("_KeyColor", c);
		}

		public void setARKeyRadius(float v)
		{
            GameObject.Find("RenderInFrontCamera").GetComponent<ARCameraBackground>().customMaterial.SetFloat("_Radius", v);					
            if (matteMaterial)
                matteMaterial.SetFloat("_Radius", v);
		}

		public void setARKeyThreshold(float v)
		{
            GameObject.Find("RenderInFrontCamera").GetComponent<ARCameraBackground>().customMaterial.SetFloat("_Threshold", v);			
            if (matteMaterial)
                matteMaterial.SetFloat("_Threshold", v);
		}
#endif

    }
}
