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
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace vpet
{
	public class ConfigEvent : UnityEvent<ConfigWidget> { }
	public class AmbientIntensityChangedEvent : UnityEvent<float> {}
	public class VisibilityChangeEvent: UnityEvent<bool> {}

#if USE_TANGO﻿
    public class TangoScaleIntensityChangedEvent : UnityEvent<float> { }
#endif

    public class ConfigWidget: MonoBehaviour
	{
	    public ConfigEvent SubmitEvent = new ConfigEvent();
		public AmbientIntensityChangedEvent AmbientChangedEvent = new AmbientIntensityChangedEvent();
	
#if USE_TANGO﻿
        public TangoScaleIntensityChangedEvent TangoScaleChangedEvent = new TangoScaleIntensityChangedEvent();
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
	    public bool doLoadFromResource = true;
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
		[HideInInspector]
		public float ambientLight = 0.1f;

#if USE_TANGO﻿
        [HideInInspector]
        public float tangoScale = 1.0f;
        private Slider tangoScaleSlider;
#endif


        private InputField serverIPField;

		private Button submitButton;

        private Toggle loadCacheToggle;

		private Toggle loadTexturesToggle;

		private Toggle debugToggle;

        private Toggle gridToggle;

        private Toggle arToggle;

        private Slider ambientIntensitySlider;

	    void Awake()
		{
            // Submit button
            Transform childWidget = this.transform.FindChild("Start_button");
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
			childWidget = this.transform.FindChild("ServerIP_field");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ServerIP_field.", this.GetType()));
			else
			{
				serverIPField = childWidget.GetComponent<InputField>();
				if (serverIPField == null) Debug.LogError(string.Format("{0}: Cant Component: InputField.", this.GetType()));
			}


			// debug toggle
            childWidget = this.transform.FindChild("Debug_toggle");
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
            childWidget = this.transform.FindChild("Grid_toggle");
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
            childWidget = this.transform.FindChild("AR_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: AR_toggle.", this.GetType()));
            else
            {
#if USE_TANGO
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

            // Cache Load Local
            childWidget = this.transform.FindChild("LoadCache_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadCache_toggle.", this.GetType()));
            else
            {
                loadCacheToggle = childWidget.GetComponent<Toggle>();
                if (loadCacheToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
            }



			// Checkbox Load Texture
			childWidget = this.transform.FindChild("LoadTextures_toggle");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadTextures_toggle.", this.GetType()));
			else
			{
				loadTexturesToggle = childWidget.GetComponent<Toggle>();
				if (loadTexturesToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
			}


			// ambient intensity
			childWidget = this.transform.FindChild("Ambient_slider");
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

#if USE_TANGO﻿
            // Tango scale
            childWidget = this.transform.FindChild("TangoScale_slider");
            if (childWidget == null) Debug.LogWarning(string.Format("{0}: Cant Find: Tango_Scale.", this.GetType()));
            else
            {
                tangoScaleSlider = childWidget.GetComponent<Slider>();
                if (tangoScaleSlider == null) Debug.LogWarning(string.Format("{0}: Cant Component: Slider.", this.GetType()));
                else
                {
                    tangoScaleSlider.value = tangoScale;
                    tangoScaleSlider.onValueChanged.AddListener(this.OnTangoScaleChanged);
                }
            }
#endif

        }


        void Start()
		{
			initUIValues();
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
                Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/AI_SliderValue").GetComponent<Text>();
                sliderValueText.text = ambientLight.ToString("f1");
            }

#if USE_TANGO﻿
            // Tango Scale
            if (tangoScaleSlider)
            {
                tangoScaleSlider.value = tangoScale;
                Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/TangoScale_Value").GetComponent<Text>();            
                sliderValueText.text = tangoScale.ToString("f1");
            }
#endif

            /*
	        // Dropdown Local Scenes
	        childWidget = this.transform.FindChild("SceneList_dropdown");
	        if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: SceneList_dropdown.", this.GetType()));
	        else
	        {
	            Dropdown widgetField = childWidget.GetComponent<Dropdown>();
	            if (widgetField == null) Debug.LogError(string.Format("{0}: Cant Component: Dropdown.", this.GetType()));
	            else
	            {
	                // sceneFileName = widgetField.itemText.text;
	            }
	        }	
            */
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

#if USE_TANGO﻿
            // Tango Scale
            if (tangoScaleSlider) {
                tangoScale = tangoScaleSlider.value;
            }
#endif

            // Checkbox Load Local
            if (loadCacheToggle != null)
            {
                doLoadFromResource = loadCacheToggle.isOn;
            }
	
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

        private void OnToggleAr( bool isOn )
        {
            if (arToggle)
            {
                // TODO: temporary
                MainController mainCtrl = GameObject.Find("MainController").GetComponent<MainController>();
                mainCtrl.ToggleArMode(isOn);
            }
        }

        private void OnAmbientChanged( float v )
		{
			ambientLight = v;
            Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/AI_SliderValue").GetComponent<Text>();
            sliderValueText.text = v.ToString("n1");
            AmbientChangedEvent.Invoke( v );
		}

#if USE_TANGO﻿
        private void OnTangoScaleChanged(float v)
        {
            tangoScale = v;            
            Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/TangoScale_Value").GetComponent<Text>();            
            sliderValueText.text = v.ToString("n1");
            TangoScaleChangedEvent.Invoke(v);
        }
#endif

    }


}
