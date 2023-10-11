/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "ChromaKeyUI.cs"
//! @brief Implementation of the ChromaKeyUI to modyfy the key colour
//! @author Alexandru-Sebastian Tufis-Schwartz
//! @version 0
//! @date 21.06.2023
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;

namespace tracer
{
    public class ChromaKeyUI : UIManagerModule
    {
        //!
        //! Variable to manage chromaButton states
        //!
        private bool _chromaButtonIsOn;
        //!
        //! Variable to manage chromaSettingsButton states
        //!
        private bool _chromaSettingsButtonIsOn;
        //!
        //! Reference to Canvas Prefab
        //!
        private GameObject _chromaKeyCanvasPrefab;
        //!
        //! Sliders Canvas Prefab
        //!
        private GameObject _chromaKeyCanvasSliders;
        //!
        //! Canvas GO
        //!
        private GameObject _chromaKeyCanvas;
        //!
        //! GO that instantiate the _uiColorPicker
        //!
        private GameObject _currentManipulator;
        //!
        //! Reference to RE_UI_ColorPicker
        //!
        private GameObject _uiColorPicker;
        //!
        //! go where the _uiColorPicker will be instantiated
        //!
        private GameObject _manipulatorPanel;
        //!
        //! Camera material
        //!
        private Material _camMaterial;
        //!
        //! Chroma key material
        //!
        private Material _chromaKeyCanvasMaterial;
        //!
        //! Vpet parameter for color
        //!
        private Parameter<Color> _materialColor;
        //!
        //! Input manager for handling user interactions
        //!
        private InputManager _mInputManager;
        //!
        //! UI button for Chroma Key Menue Button
        //!
        private MenuButton _chromaButton;
        //!
        //! UI button for Chroma Key Settings Menue Button
        //!
        private MenuButton _chromaSettingsButton;
        //!
        //! AR Camera background for custom material usage
        //!
        private ARCameraBackground _mCameraBg;
        //!
        //! Sliders for controlling Chroma Key Radius setting
        //!
        private Slider _radiusSlider;
        //!
        //! Sliders for controlling Chroma Key Treshlod setting
        //!
        private Slider _thresholdSlider;
        //!
        //! _Radius shader property
        //!
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        //!
        //! _Threshold shader property
        //!
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        //!
        //! _KeyColor shader property
        //!
        private static readonly int KeyColor = Shader.PropertyToID("_KeyColor");
        //!
        //! _textureY shader property
        //!
        private static readonly int TextureY = Shader.PropertyToID("_textureY");
        //!
        //! _textureCbCr shader property
        //!
        private static readonly int TextureCbCr = Shader.PropertyToID("_textureCbCr");
        //!
        //! _UnityDisplayTransform shader property
        //!
        private static readonly int UnityDisplayTransform = Shader.PropertyToID("_UnityDisplayTransform");

        //!
        //! Start method called when the script initializes
        //! @param sender callback sender
        //! @param e event reference
        //!
        protected override void Start(object sender, EventArgs e)
        {
            // Call the base class's start method
            base.Start(sender, e);

            // Load necessary resources
            LoadResources();

            // Initialize input manager
            _mInputManager = core.getManager<InputManager>();

            // Create Chroma Key buttons
            _chromaButton = new MenuButton("", ChromaKeyStart, new List<UIManager.Roles>() { UIManager.Roles.EXPERT });
            _chromaButton.setIcon("Images/5c89a3f32d66c");

            _chromaSettingsButton = new MenuButton("", ChromaKeySettings, new List<UIManager.Roles>() { UIManager.Roles.EXPERT });
            _chromaSettingsButton.setIcon("Images/settings");

            // Initialize material color parameter
            _materialColor = new Parameter<Color>(_chromaKeyCanvasMaterial.GetColor(KeyColor), "matcol");

            // Subscribe to events
            _mInputManager.cameraControlChanged += UpdateChromaKeyButton;
            _materialColor.hasChanged += UpdateColor;
        }

        //!
        //! Load necessary resources for the UI
        //!
        private void LoadResources()
        {
            _chromaKeyCanvasPrefab = Resources.Load("Prefabs/ChromaKeyCanvas") as GameObject;
            _uiColorPicker = Resources.Load<GameObject>("Prefabs/PRE_UI_ColorPicker");
            _chromaKeyCanvasMaterial = Resources.Load<Material>("Materials/ChromaKey");
        }

        //!
        //! Update the Chroma Key material color based on user input
        //! @param sender callback sender
        //! @param e event reference
        //!
        private void UpdateColor(object sender, Color a)
        {
            _chromaKeyCanvasMaterial.SetColor(KeyColor, a);
        }

        //!
        //! Cleanup method called when the script is destroyed
        //! @param sender callback sender
        //! @param e event reference
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            // Unsubscribe from input manager events
            _mInputManager.cameraControlChanged -= UpdateChromaKeyButton;
            _materialColor.hasChanged -= UpdateColor;
        }

        //!
        //! Constructor for the ChromaKeyUI class
        //!
        public ChromaKeyUI(string name, Manager manager) : base(name, manager)
        {
            
        }

        //!
        //! Method to handle Chroma Key start and stop
        //!
        private void ChromaKeyStart()
        {
            if (_chromaButtonIsOn)
            {
                // Turn off Chroma Key
                _chromaButtonIsOn = false;

                if (_chromaSettingsButtonIsOn)
                {
                    ChromaKeySettings();
                }
                // Disable custom material and remove UI
                _mCameraBg.useCustomMaterial = false;
                Object.DestroyImmediate(_chromaKeyCanvas);
                core.getManager<UIManager>().removeButton(_chromaSettingsButton);
            }
            else
            {
                // Turn on Chroma Key
                _chromaButtonIsOn = true;
                core.getManager<UIManager>().addButton(_chromaSettingsButton);

                // Instantiate UI elements
                if (Camera.main != null)  
                    _chromaKeyCanvas = Object.Instantiate(_chromaKeyCanvasPrefab);
                
                _chromaKeyCanvasSliders = _chromaKeyCanvas.transform.GetChild(2).gameObject;
                _manipulatorPanel = _chromaKeyCanvas.transform.GetChild(1).gameObject;

                // Configure ARCameraBackground and Chroma Key materials
                if (Camera.main != null) _mCameraBg = Camera.main.gameObject.GetComponent<ARCameraBackground>();
                _mCameraBg.customMaterial = Resources.Load<Material>("Materials/ChromaKey");
                _camMaterial = _mCameraBg.material;
                _mCameraBg.useCustomMaterial = true;

                _chromaKeyCanvasMaterial.SetTexture(TextureY, _camMaterial.GetTexture(TextureY));
                _chromaKeyCanvasMaterial.SetTexture(TextureCbCr, _camMaterial.GetTexture(TextureCbCr));
                _chromaKeyCanvasMaterial.SetMatrix(UnityDisplayTransform, _camMaterial.GetMatrix(UnityDisplayTransform));

            }
        }

        //!
        //! Method to handle Chroma Key settings
        //!
        private void ChromaKeySettings()
        {
            if (_chromaSettingsButtonIsOn)
            {
                // Turn off Chroma Key settings
                _chromaSettingsButtonIsOn = false;

                // Remove event listeners and UI elements
                _radiusSlider.onValueChanged.RemoveListener(RadiusSliderValueChangeCheck);
                _thresholdSlider.onValueChanged.RemoveListener(ThresholdSliderValueChangeCheck);
                Object.DestroyImmediate(_currentManipulator, true);
                _chromaKeyCanvasSliders.SetActive(false);
                _manipulatorPanel.SetActive(false);
                _radiusSlider = null;
                _thresholdSlider = null;
            }
            else
            {
                // Turn on Chroma Key settings
                _chromaSettingsButtonIsOn = true;
                _manipulatorPanel.SetActive(true);
                _chromaKeyCanvasSliders.SetActive(true);
                _currentManipulator = Object.Instantiate(_uiColorPicker, _manipulatorPanel.transform);
                _currentManipulator.GetComponent<ColorSelect>().Init(_materialColor);
                _radiusSlider = _chromaKeyCanvas.transform.GetChild(2).GetChild(1).GetComponent<Slider>();
                _thresholdSlider = _chromaKeyCanvas.transform.GetChild(2).GetChild(3).GetComponent<Slider>();
                
                _radiusSlider.value = _chromaKeyCanvasMaterial.GetFloat(Radius);
                _thresholdSlider.value = _chromaKeyCanvasMaterial.GetFloat(Threshold);
                
                _radiusSlider.onValueChanged.AddListener(RadiusSliderValueChangeCheck);
                _thresholdSlider.onValueChanged.AddListener(ThresholdSliderValueChangeCheck);
            }
        }

        //!
        //! Method to handle radius slider value change
        //! @param value is the value of the RadiusSlider
        //!
        public void RadiusSliderValueChangeCheck(float value)
        {
            _chromaKeyCanvasMaterial.SetFloat(Radius, value);
        }

        //!
        //! Method to handle threshold slider value change
        //! @param value is the value of the ThresholdSlider
        //!
        public void ThresholdSliderValueChangeCheck(float value)
        {
            _chromaKeyCanvasMaterial.SetFloat(Threshold, value);
        }

        //!
        //! Method to update Chroma Key button based on camera control
        //! @param sender callback sender
        //! @param c event reference
        //!
        private void UpdateChromaKeyButton(object sender, InputManager.CameraControl c)
        {
            if (c == InputManager.CameraControl.AR)
            {
                // Show Chroma Key button
                if (!core.getManager<UIManager>().getButtons().Contains(_chromaButton))
                    core.getManager<UIManager>().addButton(_chromaButton);
            }
            else
            {
                // Hide Chroma Key button and clean up
                _chromaButton.showHighlighted(false);
                if (_chromaSettingsButtonIsOn)
                {
                    ChromaKeySettings();
                }
                core.getManager<UIManager>().removeButton(_chromaSettingsButton);
                GameObject.DestroyImmediate(_chromaKeyCanvas, true);
                core.getManager<UIManager>().removeButton(_chromaButton);
            }
        }
    }
}