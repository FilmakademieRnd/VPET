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
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace vpet
{
	public class ConfigEvent : UnityEvent<ConfigWidget> { }
	public class AmbientIntensityChangedEvent : UnityEvent<float> { }
	public class VisibilityChangeEvent: UnityEvent<bool> { }

#if USE_AR
    public class FloatChangedEvent : UnityEvent<float> { }
    public class ARColorChangedEvent: UnityEvent<Color> {}
	public class ToggleARSwitchEvent : UnityEvent<bool> { }
#endif

    public class ConfigWidget: MonoBehaviour
	{
	    public ConfigEvent SubmitEvent = new ConfigEvent();
		public AmbientIntensityChangedEvent AmbientChangedEvent = new AmbientIntensityChangedEvent();
#if USE_AR
		public ToggleARSwitchEvent ToggleAREvent = new ToggleARSwitchEvent();
        public FloatChangedEvent KeyDepthChangedEvent = new FloatChangedEvent();
        public ARColorChangedEvent KeyColorChangedEvent  = new ARColorChangedEvent();
        public FloatChangedEvent KeyRadiusChangedEvent = new FloatChangedEvent();
        public FloatChangedEvent KeyThresholdChangedEvent = new FloatChangedEvent();
		public ToggleARSwitchEvent ToggleARKeyEvent = new ToggleARSwitchEvent();
        public ToggleARSwitchEvent ToggleARMatteEvent = new ToggleARSwitchEvent();
#endif

        //!
        //! IP Adress of server
        //!
        [HideInInspector]
	    public string serverIP = "172.17.21.129";
	    //!
	    //! Do we load scene from dump file
	    //!
	    [HideInInspector]
	    public bool doLoadFromResource;
	    //!
	    //! Dump scene file name
	    //!
	    [HideInInspector]
	    public string sceneFileName = "";
	    //!
	    //! Shall we load Textures ?
	    //!
	    [HideInInspector]
	    public bool doLoadTextures = true;
		//!
		//! 
		//!
		private float ambientLight = 0.1f;

        [HideInInspector]
        private Slider controllerSpeedSlider;

        public bool mattModeOn;
        public bool keyModeOn;

        private InputField serverIPField;

		private Button submitButton;

        private Toggle loadCacheToggle;

		private Toggle loadTexturesToggle;

		private Toggle debugToggle;

        private Toggle gridToggle;

        private Toggle arToggle;

        private Toggle showCamToggle;

        private Toggle arKeyToggle;

        private Toggle arMatteToggle;

        private Button arColorPickerButton;

        private ColorWheel arColorWheel;

        private ColorPicker arColorPicker;

        private Transform arKeyVideoPlane;

        private Image arColorField;

        private MainController mainController;

        [Config]
        private Color arkeyColor;

        [Config]
        private float arkeyThreshold;

        [Config]
        private float arkeyRadius;

        private Slider arkeyRadiusSlider;

        private Slider arkeyThresholdSlider;

        private Toggle cmToggle;

        private Slider ambientIntensitySlider;

	    void Awake()
		{
            doLoadFromResource = true;
            // Submit button
            Transform childWidget = this.transform.Find("Start_button");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Start_button.", this.GetType()));
            else
            {
				submitButton = childWidget.GetComponent<Button>();
				if (submitButton == null) Debug.LogError(string.Format("{0}: Cant Component: Button.", this.GetType()));
                else
                {
					submitButton.onClick.RemoveAllListeners();
					submitButton.onClick.AddListener(() => OnSubmit());
                }
            }

			// Textfield Server IP
			childWidget = this.transform.Find("ServerIP_field");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ServerIP_field.", this.GetType()));
			else
			{
				serverIPField = childWidget.GetComponent<InputField>();
				if (serverIPField == null) Debug.LogError(string.Format("{0}: Cant Component: InputField.", this.GetType()));
			}


			// debug toggle
            childWidget = this.transform.Find("Debug_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Debug_toggle.", this.GetType()));
            else
            {
				debugToggle = childWidget.GetComponent<Toggle>();
				if (debugToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    debugToggle.onValueChanged.AddListener(this.OnToggleDebug);
                }
            }

            // grid toggle
            childWidget = this.transform.Find("Grid_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Grid_toggle.", this.GetType()));
            else
            {
                gridToggle = childWidget.GetComponent<Toggle>();
                if (gridToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    gridToggle.onValueChanged.AddListener(this.OnToggleGrid);
                }
            }

            // ar toggle
            childWidget = this.transform.Find("AR_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: AR_toggle.", this.GetType()));
            else
            {

#if USE_AR
                arToggle = childWidget.GetComponent<Toggle>();
                if (arToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    arToggle.onValueChanged.AddListener(this.OnToggleAr);
                }
#else
                childWidget.gameObject.SetActive(false);
#endif
            }

            // showCameras toggle
            childWidget = this.transform.Find("ShowCam_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ShowCam_toggle.", this.GetType()));
            else
            {
                showCamToggle = childWidget.GetComponent<Toggle>();
                if (showCamToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    showCamToggle.onValueChanged.AddListener(this.OnToggleCamera);
                }
            }


            // ar key toggle
            childWidget = this.transform.Find("ARKey_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ARKey_toggle.", this.GetType()));
            else
            {
#if USE_AR
                arKeyToggle = childWidget.GetComponent<Toggle>();
                if (arKeyToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    arKeyToggle.onValueChanged.AddListener(this.OnToggleArKey);
                }
#endif
                // hide by default
                childWidget.gameObject.SetActive(false);
            }

            // ar matte toggle
            childWidget = this.transform.Find("ARMatte_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ARMatte_toggle.", this.GetType()));
            else
            {
#if USE_AR
                arMatteToggle = childWidget.GetComponent<Toggle>();
                if (arMatteToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    arMatteToggle.onValueChanged.AddListener(this.OnToggleMatte);
                }

#endif
                // hide by default
                childWidget.gameObject.SetActive(false);
            }

            //ar color picker button
            childWidget = this.transform.Find("ARKeyPick_button");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ARKeyPick_button.", this.GetType()));
            else
            {
#if USE_AR
                arColorPickerButton = childWidget.GetComponent<Button>();
                if (arColorPickerButton == null) Debug.LogError(string.Format("{0}: Cant Component: Button.", this.GetType()));
#endif
                // hide by default
                childWidget.gameObject.SetActive(false);
            }


            // ar video plane
            arKeyVideoPlane = this.transform.Find("../ARVideoPlane");
            if (arKeyVideoPlane == null) Debug.LogError(string.Format("{0}: Cant Find: ARColorPlane.", this.GetType()));
            else
            {
                // hide by default
                arKeyVideoPlane.gameObject.SetActive(false);
            }


            // ar color picker plane
            childWidget = this.transform.Find("../ARColorPlane");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ARColorPlane.", this.GetType()));
            else
            {
#if USE_AR
                arColorPicker = childWidget.GetComponent<ColorPicker>();
                if (arColorPicker == null) Debug.LogError(string.Format("{0}: Cant Component: ColorPicker.", this.GetType()));
                else
                {
                    arColorPicker.Callback = this.OnKeyColorChanged;
                }
#endif
            }


            // ar video color wheel
            childWidget = this.transform.Find("../ARKeyWidget/ARColorWheel");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ARColorWheel.", this.GetType()));
            else
            {
#if USE_AR
                arColorWheel = childWidget.GetComponent<ColorWheel>();
                if (arColorWheel == null) Debug.LogError(string.Format("{0}: Cant Component: ColorWheel.", this.GetType()));
                else
                {
                    arColorWheel.Callback = this.OnKeyColorChanged;
                }
#endif
                // hide by default
                childWidget.gameObject.SetActive(false);
            }

            // ar color field
            childWidget = this.transform.Find("../ARKeyWidget/ColorField");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ColorField.", this.GetType()));
            else
            {
#if USE_AR
                arColorField = childWidget.GetComponent<Image>();
                if (arColorField == null) Debug.LogError(string.Format("{0}: Cant Component: Image.", this.GetType()));
#endif                
                
            }

            // ar video radius slider
            childWidget = this.transform.Find("../ARKeyWidget/KeyRadiusSlider/KeyRadius_Slider");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: KeyRadiusSlider.", this.GetType()));
            else
            {
#if USE_AR
                arkeyRadiusSlider = childWidget.GetComponent<Slider>();
                if (arkeyRadiusSlider == null) Debug.LogError(string.Format("{0}: Cant Component: Slider.", this.GetType()));
                else
                {
                    arkeyRadiusSlider.onValueChanged.AddListener( this.OnKeyRadiusChanged);
                }
#endif
            }

            // ar video threshold slider
            childWidget = this.transform.Find("../ARKeyWidget/KeyThresholdSlider/KeyThreshold_Slider");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: KeyThresholdSlider.", this.GetType()));
            else
            {
#if USE_AR
                arkeyThresholdSlider = childWidget.GetComponent<Slider>();
                if (arkeyThresholdSlider == null) Debug.LogError(string.Format("{0}: Cant Component: Slider.", this.GetType()));
                else
                {
                    arkeyThresholdSlider.onValueChanged.AddListener( this.OnKeyThresholdChanged);
                }
#endif
            }


            // Cache Load Local
            childWidget = this.transform.Find("LoadCache_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadCache_toggle.", this.GetType()));
            else
            {
                loadCacheToggle = childWidget.GetComponent<Toggle>();
                if (loadCacheToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
            }

			// Checkbox Load Texture
			childWidget = this.transform.Find("LoadTextures_toggle");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadTextures_toggle.", this.GetType()));
			else
			{
				loadTexturesToggle = childWidget.GetComponent<Toggle>();
				if (loadTexturesToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
			}

			// ambient intensity
			childWidget = this.transform.Find("AI_Slider/Ambient_slider");
			if (childWidget == null) Debug.LogWarning(string.Format("{0}: Cant Find: Ambient_slider.", this.GetType()));
			else
			{
				ambientIntensitySlider = childWidget.GetComponent<Slider>();
				if (ambientIntensitySlider == null) Debug.LogWarning(string.Format("{0}: Cant Component: Slider.", this.GetType()));
				else
				{
					ambientIntensitySlider.value = ambientLight;
					ambientIntensitySlider.onValueChanged.AddListener( this.OnAmbientChanged );
				}
			}

            // controller speed
            childWidget = this.transform.Find("CS_Slider/ControllerSpeed_slider");
            if (childWidget == null) Debug.LogWarning(string.Format("{0}: Cant Find: ControllerSpeed.", this.GetType()));
            else
            {
                controllerSpeedSlider = childWidget.GetComponent<Slider>();
                if (controllerSpeedSlider == null) Debug.LogWarning(string.Format("{0}: Cant Component: Slider.", this.GetType()));
                else
                {
                    if(VPETSettings.Instance.controllerSpeed <= 1)
                        controllerSpeedSlider.value = (VPETSettings.Instance.controllerSpeed-0.25f)/ 1.5f;
                    else
                        controllerSpeedSlider.value = (VPETSettings.Instance.controllerSpeed + 23f) /48f;
                    controllerSpeedSlider.onValueChanged.AddListener(this.onControllerSpeedChanged);
                }
            }

        }


        void Start()
		{
            initUIValues();
            mainController = GameObject.Find("Controller/MainController").GetComponent<MainController>();
        }


        public void initConfigWidget()
	    {
	        initUIValues();
            if (VPETSettings.Instance.sceneDumpFolderEmpty)
            {
                loadCacheToggle.isOn = false;
                loadCacheToggle.interactable = false;
                doLoadFromResource = false;
            }
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
			readUIValues();
            gameObject.SetActive(false);
        }
	
        //!
        //! Assign values to UI elements
        //!
	    void initUIValues()
	    {
	        // Textfield Server IP
			if ( serverIPField )
			{
				serverIPField.text = serverIP;
			}
				
            // Checkbox Load Local
            if (loadCacheToggle )
            {
                loadCacheToggle.isOn = doLoadFromResource;
            }

			// Checkbox Load Texture
			if ( loadTexturesToggle )
			{
				loadTexturesToggle.isOn = doLoadTextures;
			}

			// Ambient intensity
			if ( ambientIntensitySlider )
			{
				ambientIntensitySlider.value = ambientLight;
                ambientIntensitySlider.transform.parent.Find("AI_SliderValue").GetComponent<Text>().text = ambientLight.ToString("f1");
                ambientIntensitySlider.onValueChanged.Invoke(ambientLight);
            }



            // scene Scale
            if (controllerSpeedSlider)
            {
                if (VPETSettings.Instance.controllerSpeed <= 1)
                    controllerSpeedSlider.value = (VPETSettings.Instance.controllerSpeed - 0.25f) / 1.5f;
                else
                    controllerSpeedSlider.value = (VPETSettings.Instance.controllerSpeed + 23f) / 48f;
                controllerSpeedSlider.transform.parent.Find("ControllerSpeed_Value").GetComponent<Text>().text = VPETSettings.Instance.controllerSpeed.ToString("n3");
            }

#if USE_AR

            // arkey settings
            if (arkeyRadiusSlider)
            {
                arkeyRadiusSlider.value = arkeyRadius;
                arkeyRadiusSlider.transform.parent.Find("KeyRadius_Value").GetComponent<Text>().text = arkeyRadius.ToString("n2");

            }

            if (arkeyThresholdSlider)
            {
                arkeyThresholdSlider.value = arkeyThreshold;
                arkeyThresholdSlider.transform.parent.Find("KeyThreshold_Value").GetComponent<Text>().text = arkeyThreshold.ToString("n2");

            }

            if (arColorField)
            {
                OnKeyColorChanged(arkeyColor);
            }
#endif

        }

        void readUIValues()
	    {

			// Textfield Server IP
			if ( serverIPField )
			{
				serverIP = serverIPField.text;
			}

			// Checkbox Load Local
			if (loadCacheToggle )
			{
				doLoadFromResource = loadCacheToggle.isOn;
			}
				
			// Checkbox Load Texture
			if ( loadTexturesToggle )
			{
				doLoadTextures = loadTexturesToggle.isOn;
			}

			// Ambient intensity
			if ( ambientIntensitySlider )
			{
				ambientLight = ambientIntensitySlider.value;
            }


            // Checkbox Load Local
            //if (loadCacheToggle != null)
            //{
            //    doLoadFromResource = loadCacheToggle.isOn;
            //}
	
	    }
	
	    public void OnSubmit()
	    {
	        readUIValues();
	        SubmitEvent.Invoke(this);
	    }


        private void OnToggleDebug( bool isOn)
        {
            VPETSettings.Instance.debugMsg = isOn;
        }


        private void OnToggleGrid(bool isOn)
        {
            drawGrid drawGrid = Camera.main.GetComponent<drawGrid>();
            if (drawGrid)
                drawGrid.enabled = isOn;
        }


        private void OnToggleCamera(bool isOn)
        {
            mainController.showCam = isOn;
            if (SceneLoader.SceneCameraList.Count > 0)
            {
                foreach (GameObject camObject in SceneLoader.SceneCameraList)
                {
                    GameObject camGeometry = camObject.transform.GetChild(0).gameObject;
                    BoxCollider[] col = camGeometry.GetComponentsInParent<BoxCollider>(true);
                    if (col.Length > 0)
                        col[0].enabled = mainController.showCam;
                    camGeometry.SetActive(isOn);
                }
            }
        }

#if USE_AR
        private void OnToggleAr( bool isOn )
        {
            // show/hide ar key toggle
            arKeyToggle.gameObject.SetActive(isOn);    
            arMatteToggle.gameObject.SetActive(isOn);

            if (arToggle)
            {
                //MainController mainCtrl = GameObject.Find("MainController").GetComponent<MainController>();
                //mainCtrl.ToggleArMode(isOn);
				ToggleAREvent.Invoke (isOn);

                // make sure state is correct to toggle button
                OnToggleArKey(arKeyToggle.isOn);
                OnToggleMatte(arMatteToggle.isOn);
            }
        }


        private void OnToggleArKey( bool isOn )
        {

            if (arToggle.isOn && isOn)
            {
                arKeyVideoPlane.gameObject.SetActive(true);
                arColorPickerButton.gameObject.SetActive(true);
            }
            else if (arToggle.isOn && arMatteToggle.isOn)
            {
                arKeyVideoPlane.gameObject.SetActive(false);
                arColorPickerButton.gameObject.SetActive(true);
            }
            else
            {
                arKeyVideoPlane.gameObject.SetActive(false);
                arColorPickerButton.gameObject.SetActive(false);
            }
            keyModeOn = isOn;
			ToggleARKeyEvent.Invoke (isOn);
        }

        private void OnToggleMatte(bool isOn)
        {

            if (arToggle.isOn && isOn)
            {
                arColorPickerButton.gameObject.SetActive(true);
            }
            else if (arToggle.isOn && arKeyToggle.isOn)
            {
                arColorPickerButton.gameObject.SetActive(true);
            }
            else
            {
                arColorPickerButton.gameObject.SetActive(false);
            }
            mattModeOn = isOn;
            ToggleARMatteEvent.Invoke(isOn);

        }

        private void OnDepthChanged(string v)
        {
            try
            {
                float vFloat = float.Parse(v);
                KeyDepthChangedEvent.Invoke(vFloat);            
            }
            catch (System.Exception)
            {
                
                // throw;
            }
        }

        private void OnKeyColorChanged(Color c)
        {
            arkeyColor = c;
            if (arColorField) arColorField.color = c;
            KeyColorChangedEvent.Invoke(c);
        }

        private void OnKeyRadiusChanged(float v)
        {
            arkeyRadius = v;
            Text sliderValueText = GameObject.Find("GUI/Canvas/ARKeyWidget/KeyRadiusSlider/KeyRadius_Value").GetComponent<Text>();
            sliderValueText.text = v.ToString("n2");
            KeyRadiusChangedEvent.Invoke(v);
        }

        private void OnKeyThresholdChanged(float v)
        {
            arkeyThreshold = v;
            Text sliderValueText = GameObject.Find("GUI/Canvas/ARKeyWidget/KeyThresholdSlider/KeyThreshold_Value").GetComponent<Text>();
            sliderValueText.text = v.ToString("n2");
            KeyThresholdChangedEvent.Invoke(v);
        }


#endif

        private void OnAmbientChanged( float v )
		{
			ambientLight = v;
            Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/AI_Slider/AI_SliderValue").GetComponent<Text>();
            sliderValueText.text = v.ToString("n1");
            AmbientChangedEvent.Invoke( v );
		}

        private void onControllerSpeedChanged(float v)
        {
            Text sliderValueText = controllerSpeedSlider.transform.Find("../ControllerSpeed_Value").GetComponent<Text>();
            if (v <= 0.5f)
            {
                sliderValueText.text = (0.25f + v * 1.5f).ToString("n3");
                VPETSettings.Instance.controllerSpeed = 0.25f + v * 1.5f;
            }
            else
            {
                sliderValueText.text = (v * 48f - 23f).ToString("n3");
                VPETSettings.Instance.controllerSpeed = v * 48f - 23f;
            }
        }

        public void ToggleColorWheel()
        {
            if (arColorWheel != null)
                arColorWheel.gameObject.SetActive(!arColorWheel.gameObject.activeSelf);
        }

    }

}
