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
#if USE_ARKIT
using UnityEngine.XR.iOS;
#endif

//!
//! INFO: the mainController class is separeted into different files
//! find all parts in the files named like MainController.XXX
//!
namespace vpet
{
	// public class GravityChangeEvent : UnityEvent<bool> { }
	// public class VPETBoolEvent : UnityEvent<bool> { }

	public partial class MainController : MonoBehaviour
    {


        public Material matteMaterial;

        public GameObject arCoverSphere;

		public bool arMode;
        public bool lockARRotation;
        public bool lockARScale;


#if USE_ARKIT
		private GameObject m_anchorModifier = null;
		private GameObject m_anchorPrefab = null;
#endif

        /*
	    //!
	    //! check if screen position is on an active GUI element & get id of the button at screen position
	    //! @param      pos     position on screen (z is always 0)
	    //! @return     id of button under screen position, -1 if none
	    //!
	    public int getGuiElementId(Vector3 pos)
	    {
	        return ui.getButtonId(new Vector2(pos.x, Screen.height - pos.y)); ;
	    }
		*/

        //!
        //! move the camera relative to last position
        //! @param      translation     new position relative to old one
        //!
        public void moveCameraObject(Vector3 translation)
        {
            Camera.main.transform.Translate(translation * VPETSettings.Instance.controllerSpeed * VPETSettings.Instance.sceneScale * ( VPETSettings.Instance.maxExtend/ 3000f));
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

        /*
	    //!
	    //! check if screen position is on color or intensity picker (in light mode)
	    //! @param      pos     position on screen (z is always 0)
	    //! @return     true if no GUI Element at screen position
	    //!
	    public bool isOnLightSettingsPicker(Vector3 pos)
	    {
	        return ui.isOnLightSettingsPicker(pos);
	    }
		*/

        /*
	    //!
	    //! update the light parameters when beeing in lightEditing mode based on the mouse position on GUI
	    //! @param      pos     position on screen (z is always 0)
	    //!
	    public void updateLight(Vector3 pos)
	    {
	        Color newColor = ui.getColorPickerValue(pos);
	        if (newColor != new Color(0, 0, 0, 0))
	        {
	            if (currentSelection) currentSceneObject.setLightColor(newColor);
	            return;
	        }
	        float newIntensity = ui.getIntensityPickerValue(pos);
	        if (newIntensity != -1)
	        {
	            if (currentSelection) currentSceneObject.setLightIntensity(newIntensity * 8.0f);
	            return;
	        }
	        float newDeltaRange = ui.getRangePickerDeltaValue(pos);
	        if (newDeltaRange != 0)
	        {
	            if (currentSelection) currentSceneObject.setLightDeltaRange(newDeltaRange);
	            return;
	        }
	        float newAngle = ui.getAnglePickerValue(pos);
	        if (newAngle != -1)
	        {
	            if (currentSelection) currentSceneObject.setLightAngle(newAngle);
	            return;
	        }
	    }
		*/

	
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

			/*
			if (serverAdapter.receiveNcam) 
			{
				serverAdapter.receiveNcam = false;
	            camObject.GetComponent<Renderer>().enabled = true;
				cameraAdapter.setMove(true);
			}
			else
			{
				serverAdapter.receiveNcam = true;
	            camObject.GetComponent<Renderer>().enabled = false;
				cameraAdapter.setMove(false);
			}
			*/
		}
	
	    //!
	    //! toggle ignore rotation
	    //!
	    public void toggleCameraRotation()
	    {
	        cameraAdapter.doApplyRotation = !cameraAdapter.doApplyRotation; 
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

			// restore previous view, e.g. when returning from orthographic view
			Camera.main.transform.position = camPreviousPosition;
			Camera.main.transform.rotation = camPreviousRotation;

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
            if (!arMode)
            {
                setPerspectiveCamera();
                if (SceneLoader.SceneCameraList.Count > 0)
                {
                    GameObject camObject = SceneLoader.SceneCameraList[camPrefabPosition];
                    SceneObjectCamera soc = camObject.GetComponent<SceneObjectCamera>();
                    GameObject camGeometry = camObject.transform.GetChild(0).gameObject;
                    camGeometry.SetActive(false);
                    cameraAdapter.registerNearObject(camGeometry);

                    Camera.main.transform.position = camObject.transform.position; 
                    Camera.main.transform.rotation = camObject.transform.rotation;          
                    Camera.main.nearClipPlane = 0.1f * VPETSettings.Instance.sceneScale;
                    Camera.main.farClipPlane = soc.far;
                    Camera.main.fieldOfView = soc.fov;

                    // callibrate 
                    cameraAdapter.calibrate(camObject.transform.rotation);

                    // set camera properties
                    CameraObject camScript = camObject.GetComponent<CameraObject>();
                    if (camScript != null)
                    {
                        Camera.main.fieldOfView = camScript.fov; //.hFovToVFov(); // convert horizontal fov from Katana to vertical
                                                                 //Camera.main.nearClipPlane = camScript.near * VPETSettings.Instance.sceneScale;
                                                                 //Camera.main.farClipPlane = camScript.far * VPETSettings.Instance.sceneScale;
                        UpdatePropertiesSecondaryCameras();
                    }
                    // set properties for DOF component from CameraObject
                    //Camera.main.GetComponent<DepthOfField>().focalLength = camScript.focDist;
                    //Camera.main.GetComponent<DepthOfField>().focalSize = camScript.focSize;
                    //Camera.main.GetComponent<DepthOfField>().aperture = camScript.aperture;
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





			/*
            // HACK to invoke
            MenuButton configButton = GameObject.Find("GUI/Canvas/UI/mainMenuObject/3_GeneralMenu_Settings_sel").GetComponent<MenuButton>();
            configButton.onClick.Invoke();
            configButton.Toggled = true;
			*/




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
#if USE_ARKIT
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

            GameObject root = GameObject.Find("Scene");
            if (root)
            {
                GameObject.Destroy(root.GetComponent<ARPlaneAlignment>());
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

        public void ToggleArMode(bool active)
        {
			arMode = active;
			ui.setupSecondaryMenu ();
#if USE_ARKIT
			GameObject root = GameObject.Find("Scene");
			GameObject arPlanes = GameObject.Find("ARPlanes");
			GameObject arKit = GameObject.Find("ARKit");
			TouchInput input = inputAdapter.GetComponent<TouchInput>();
			GameObject arConfigWidget = GameObject.Find("GUI/Canvas/ARConfigWidget");
			GameObject rootScene = SceneLoader.scnRoot;

			if (m_anchorPrefab == null)
				m_anchorPrefab = Resources.Load ("VPET/Prefabs/AnchorModifier", typeof(GameObject)) as GameObject;

			if (input)
 				input.enabled = !active;
				
#endif
            if (active)
            {
#if USE_TANGO
				sceneAdapter.HideGeometry();
                tangoApplication.m_enableVideoOverlay = true;
                tangoApplication.m_videoOverlayUseTextureMethod = true;
                TangoARScreen tangoArScreen = Camera.main.gameObject.GetComponent<TangoARScreen>();
                if (tangoArScreen == null) 
                    tangoArScreen = Camera.main.gameObject.AddComponent<TangoARScreen>();
                tangoArScreen.enabled = true;
#elif USE_ARKIT
                //avoids object updates while placing, scaling, rotating scene in AR setup
                lockScene = true;
                // reset camera
                cameraAdapter.globalCameraReset();
                //resetCameraOffset();
                Camera.main.transform.position = Vector3.zero;
                Camera.main.transform.rotation = Quaternion.identity;
                //Camera.main.fieldOfView = 60;
                //Camera.main.nearClipPlane = 0.1f;
                //Camera.main.farClipPlane = 100000;
                // enable video background

				ARScreen arkitScreen = Camera.main.gameObject.GetComponent<ARScreen>();
                if (arkitScreen == null) 
					arkitScreen = Camera.main.gameObject.AddComponent<ARScreen>();
				
				if (arKit)
				{
					ARKitController arController = arKit.GetComponent<ARKitController>();
					if (arController)
						arController.setARMode(true);
				}

				// enable plane alignment
				if (root)
				{
					ARPlaneAlignment hitTest = root.GetComponent<ARPlaneAlignment>();
					if (hitTest == null) {
						hitTest = root.AddComponent<ARPlaneAlignment>();
						hitTest.m_HitTransform = root.transform;
					}
				}
				// enable plane visualisation
				if (arPlanes)
				{
					ARPlane arPlaneComponent = arPlanes.GetComponent<ARPlane>();
					if (arPlaneComponent == null) {
						arPlaneComponent = arPlanes.AddComponent<ARPlane>();
					}
				}
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
                ui.hideConfigWidget();
				//hide scene while placing AR anchor
				//rootScene.SetActive(false);
				arConfigWidget.SetActive(true);
                SetSceneScale(VPETSettings.Instance.sceneScale);
                //arConfigWidget.transform.Find("scale_value").GetComponent<Text>().text;

                //initalize ar lock buttons
                ui.changeARLockRotationButtonImage(lockARRotation);
                ui.changeARLockScaleButtonImage(lockARScale);

#endif
            }
            else
            {
#if USE_TANGO
                Camera.main.gameObject.GetComponent<TangoARScreen>().enabled = false;
                GameObject.Destroy(Camera.main.GetComponent<TangoARScreen>());
                tangoApplication.m_enableVideoOverlay = false;
#elif USE_ARKIT
				if (arKit)
				{
					ARKitController arController = arKit.GetComponent<ARKitController>();
					if (arController)
						arController.setARMode(false);
				}
				// destroy video background
				GameObject.Destroy(Camera.main.GetComponent<ARScreen>());
				if (root)
				{
					GameObject.Destroy(root.GetComponent<ARPlaneAlignment>());
				}
				// destroy plane visualisation
				if (arPlanes)
				{
					GameObject.Destroy(arPlanes.GetComponent<ARPlane>());
				}
				// disable anchor visualisation
				if (m_anchorModifier) {
					m_anchorModifier.SetActive(false);
					GameObject.Destroy(m_anchorModifier);
					m_anchorModifier = null;
				}
#endif
				// reset cameras to defaults
                Camera.main.ResetProjectionMatrix();
                SetSceneScale(1f);
				repositionCamera();
                UpdateProjectionMatrixSecondaryCameras();
                sceneAdapter.ShowGeometry();

            }

            hasUpdatedProjectionMatrix = true;

        }

#if USE_ARKIT
		public void ToggleARKeyMode(bool active)
		{
			ARScreen arkitScreen = Camera.main.gameObject.GetComponent<ARScreen>();
            if (arkitScreen == null) 
				return;

			Shader shader = Shader.Find("VPET/ARCameraShader");
			if (active)
				shader = Shader.Find("VPET/ARCameraShaderChromaKey");

			if (shader)
				arkitScreen.m_ClearMaterial.shader = shader;
		}
#endif



#if USE_ARKIT
        public void ToggleARMatteMode(bool active)
        {
            
            ARScreen arkitScreen = Camera.main.gameObject.GetComponent<ARScreen>();
            if (arkitScreen == null)
                return;

            if (arCoverSphere != null)
                arCoverSphere.SetActive(active);

            if (active)
            {
                if (matteMaterial != null)
                {
                    matteMaterial.SetTexture("_textureY", arkitScreen.m_ClearMaterial.GetTexture("_textureY"));
                    matteMaterial.SetTexture("_textureCbCr", arkitScreen.m_ClearMaterial.GetTexture("_textureCbCr"));
                    matteMaterial.SetMatrix("_DisplayTransform", arkitScreen.m_ClearMaterial.GetMatrix("_DisplayTransform"));
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
            v = Mathf.Max(v,0.00001f);
            // scale the scene
            SceneLoader.scnRoot.transform.localScale = new Vector3(v, v, v);

            float sceneScalePrevious = VPETSettings.Instance.sceneScale;
            VPETSettings.Instance.sceneScale = v;

            // update light params
            foreach (GameObject obj in SceneLoader.SelectableLights)
            {
                float lightRange = obj.GetComponent<SceneObjectLight>().getLightRange() / sceneScalePrevious * v;
                obj.GetComponent<SceneObjectLight>().setLightRange(lightRange);
                //obj.GetComponent<SceneObject>().SourceLight.transform.localScale = Vector3.one / VPETSettings.Instance.sceneScale;
                obj.GetComponentInChildren<LightIcon>().TargetScale = obj.transform.lossyScale;
            }

            ui.updateScaleValue(v);
            Vector3 sceneExtends = VPETSettings.Instance.sceneBoundsMax - VPETSettings.Instance.sceneBoundsMin;
            float maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);
            QualitySettings.shadowDistance = v * maxExtend * 3f;
            //Camera.main.nearClipPlane = Mathf.Max(0.1f, Vector3.Distance(Camera.main.transform.position, scene.transform.position) - maxExtend * v);
            //Camera.main.farClipPlane = Mathf.Max(100f,Mathf.Min(100000f, Vector3.Distance(Camera.main.transform.position, scene.transform.position) + maxExtend * v));
            Physics.gravity = new Vector3(0, -0.24525f * VPETSettings.Instance.sceneScale * maxExtend, 0);

            /*foreach (Rigidbody rigi in FindObjectsOfType<Rigidbody>())
            {
                rigi.mass = 0.1f * maxExtend * v;
            }*/

            VPETSettings.Instance.maxExtend = maxExtend;
            // update camera params
            UpdatePropertiesSecondaryCameras();
        }


        public void SliderValueChanged( float x )
        {
            // set keyframe
            if ( ui.LayoutUI == layouts.ANIMATION )
            {
                animationController.setKeyFrame();
            }
        }

#if USE_ARKIT
		public void setARKeyDepth(float v)
		{
            if (Camera.main.GetComponent<ARScreen>() != null)
            {
                Camera.main.GetComponent<ARScreen>().m_ClearMaterial.SetFloat("_Depth", v);
            }
		}

		public void setARKeyColor(Color c)
		{
            if (Camera.main.GetComponent<ARScreen>() != null)
            {
                Camera.main.GetComponent<ARScreen>().m_ClearMaterial.SetColor("_KeyColor", c);
            }	

            if (matteMaterial)
                matteMaterial.SetColor("_KeyColor", c);
		}

		public void setARKeyRadius(float v)
		{
            if (Camera.main.GetComponent<ARScreen>() != null)
            {
                Camera.main.GetComponent<ARScreen>().m_ClearMaterial.SetFloat("_Radius", v);
            }						
            if (matteMaterial)
                matteMaterial.SetFloat("_Radius", v);
		}

		public void setARKeyThreshold(float v)
		{
            if (Camera.main.GetComponent<ARScreen>() != null)
            {
                Camera.main.GetComponent<ARScreen>().m_ClearMaterial.SetFloat("_Threshold", v);
            }						
            if (matteMaterial)
                matteMaterial.SetFloat("_Threshold", v);
		}
#endif

    }
}
