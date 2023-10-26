/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
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


//! @file "kill.cs"
//! @brief Controller input 
//! @author Alexandru Schwartz
//! @version 0
//! @date 26.10.2023

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using tracer;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace tracer
{

    public class ControllerModule : InputManagerModule
    {
               //!
        //! Reference to the main camera GameObject
        //!
        private GameObject _mainCamera;

        //!
        //! Reference to the crosshair GameObject
        //!
        private GameObject _crosshair;

        //!
        //! Prefab for the controller canvas
        //!
        private GameObject _controllerCanvasPrefab;

        //!
        //! Reference to the instantiated controller canvas
        //!
        private GameObject _controllerCanvas;

        //!
        //! Reference to the current add selector GameObject
        //!
        private GameObject _currentAddSelector;

        //!
        //! Reference to the crosshair image
        //!
        private Image _crossHairImg;

        //!
        //! Reference to the currently selected scene object
        //!
        private SceneObject _currentSelectedSceneObject;

        //!
        //! Reference to the selector SnapSelect component
        //!
        private SnapSelect _selectorSnapSelect;

        //!
        //! Reference to the spinner SnapSelect component
        //!
        private SnapSelect _spinnerSnapSelect;

        //!
        //! Reference to the button selector SnapSelect component
        //!
        private SnapSelect _buttonSelectorPrefabSnapSelect; // (Clone)

        //!
        //! Reference to the color select component
        //!
        private ColorSelect _colorSelect;

        //!
        //! Reference to the Camera component
        //!
        private Camera _camera;

        //!
        //! Reference to the UIManager
        //!
        private UIManager _uiManager;

        //!
        //! Reference to the SceneManager
        //!
        private SceneManager _sceneManager;

        //!
        //! Reference to the SelectionModule
        //!
        private SelectionModule _selectionModule;

        //!
        //! Reference to the CameraSelectionModule
        //!
        private CameraSelectionModule _cameraSelectionModule;

        //!
        //! List of scene objects
        //!
        private List<SceneObject> _sceneObjectsList;

        //!
        //! List of scene lights
        //!
        private List<SceneObjectLight> _sceneObjectLightsList;

        //!
        //! List of scene cameras
        //!
        private List<SceneObjectCamera> _sceneObjectCamerasList;

        //!
        //! List of elements in the selector SnapSelect
        //!
        private List<SnapSelectElement> _selectorSnapSelectElementsList;

        //!
        //! List of elements in the spinner SnapSelect
        //!
        private List<SnapSelectElement> _spinnerSnapSelectElementsList;

        //!
        //! The index of the currently selected SnapSelect element in the selector
        //!
        private int _selectorCurrentSelectedSnapSelectElement = 0;

        //!
        //! The index of the currently selected SnapSelect element in the spinner
        //!
        // private int _spinnerCurrentSelectedSnapSelectElement = 0;

        //!
        //! The index of the currently selected object in the list
        //!
        private int _selectedListObject;

        //!
        //! The ID of the camera selection button
        //!
        private int _cameraSelectionButtonID;

        //!
        //! The value of the left stick on the controller
        //!
        private Vector2 _leftStickValue;

        //!
        //! The value of the right stick on the controller
        //!
        private Vector2 _rightStickValue;

        //!
        //! The resulting vector from controller input
        //!
        private Vector3 _result;

        //!
        //! Flag indicating whether the crosshair is currently visible
        //!
        private bool _isCrosshairOn;

        //!
        //! Flag indicating whether the "look through" mode is active
        //!
        private bool _lookThroughOn;

        //!
        //! Constant for movement speed
        //!
        private const float Speed = 3f;

        //!
        //! Constant for rotation speed
        //!
        private const float RptationSpeed = 100f;

        //!
        //! The ray used for raycasting
        //!
        private Ray _ray;

        //!
        //! The RaycastHit data from raycasting
        //!
        private RaycastHit _hit;

        //!
        //! The currently selected abstract parameter
        //!
        private AbstractParameter _selectedAbstractParam;

        //!
        //! Event handler for controller editing completion
        //!
        public event EventHandler<AbstractParameter> ControllerdoneEditing;


        //!
        //! Initialization method for the controller.
        //!
        protected override void Init(object sender, EventArgs e)
        {
            // Load the controller canvas prefab.
            _controllerCanvasPrefab = Resources.Load("Prefabs/ControllerCanvas") as GameObject;
            
            // Find the main camera.
            _mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            
            // Get the camera component.
            _camera = _mainCamera.GetComponent<Camera>();
            
            // Get the scene manager from the core.
            _sceneManager = core.getManager<SceneManager>();
            
            // Get the UI manager from the core.
            _uiManager = core.getManager<UIManager>();
            
            // Get the selection module from the UI manager.
            _selectionModule = _uiManager.getModule<SelectionModule>();

            // Subscribe to the ControllerdoneEditing event.
            ControllerdoneEditing += _sceneManager.getModule<UndoRedoModule>().addHistoryStep;

            // Initialize lists for scene objects, lights, and cameras.
            _sceneObjectsList = _sceneManager.simpleSceneObjectList;
            _sceneObjectLightsList = _sceneManager.sceneLightList;
            _sceneObjectCamerasList = _sceneManager.sceneCameraList;

            // Subscribe to controller button events.
            manager.buttonNorth += PressNorth;
            manager.buttonSouth += PressSouth;
            manager.buttonEast += PressEast;
            manager.buttonWest += PressWest;
            manager.buttonUp += PressUp;
            manager.buttonDown += PressDown;
            manager.buttonLeft += PressLeft;
            manager.buttonRight += PressRight;
            manager.buttonLeftTrigger += PressLeftTrigger;
            manager.buttonRightTrigger += PressRightTrigger;
            manager.buttonLeftShoulder += PressLeftShoulder;
            manager.buttonRighrShoulder += PressRightShoulder;
            manager.leftControllerStick += MoveLeftStick;
            manager.rightControllerStick += MoveRightStick;
            manager.ControllerStickCanceled += DoneEditing;

            // Subscribe to the core update event.
            core.updateEvent += TracerUpdate;

            // Subscribe to UI manager events.
            _uiManager.selectionChanged += UiManagerSelectionChanged;
            _uiManager.selectionRemoved += UiManagerSelectionRemoved;
            _uiManager.colorSelectGameObject += GetColorSelect;
        }

        //!
        //! Cleanup method for the controller.
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            // Unsubscribe from controller button events.
            manager.buttonNorth -= PressNorth;
            manager.buttonSouth -= PressSouth;
            manager.buttonEast -= PressEast;
            manager.buttonWest -= PressWest;
            manager.buttonUp -= PressUp;
            manager.buttonDown -= PressDown;
            manager.buttonLeft -= PressLeft;
            manager.buttonRight -= PressRight;
            manager.buttonLeftTrigger -= PressLeftTrigger;
            manager.buttonRightTrigger -= PressRightTrigger;
            manager.buttonLeftShoulder -= PressLeftShoulder;
            manager.buttonRighrShoulder -= PressRightShoulder;
            manager.leftControllerStick -= MoveLeftStick;
            manager.rightControllerStick -= MoveRightStick;
            manager.ControllerStickCanceled -= DoneEditing;

            // Unsubscribe from the core update event.
            core.updateEvent -= TracerUpdate;

            // Unsubscribe from UI manager events.
            _uiManager.selectionChanged -= UiManagerSelectionChanged;
            _uiManager.selectionRemoved -= UiManagerSelectionRemoved;
            _uiManager.colorSelectGameObject -= GetColorSelect;

            // Unsubscribe from the ControllerdoneEditing event.
            ControllerdoneEditing -= _sceneManager.getModule<UndoRedoModule>().addHistoryStep;
        }

        #region StateMachineLogic
        
        //!
        //! Controller modes enumeration.
        //!
        private enum ControllerModes
        {
            MAIN_VIEW_MODE,
            OBJECT_MODE,
            LIGHT_MODE,
            CAMERAS_MODE
        }

        //!
        //! The current controller state.
        //!
        private ControllerModes _currentState = ControllerModes.MAIN_VIEW_MODE;

        //!
        //! Switches to the default controller mode.
        //!
        private void SwitchToDefaultMode()
        {
            _currentState = 0;
            _selectedListObject = 0;
            ChangeSelectedObject(0);
        }

        //!
        //! Switches to the next controller mode.
        //!
        private void SwitchToNextMode()
        {
            int nextMode = ((int)_currentState + 1) % (System.Enum.GetValues(typeof(ControllerModes)).Length);

            // Loop until a non-empty mode is found
            while (IsListEmpty((ControllerModes)nextMode))
            {
                nextMode = (nextMode + 1) % (System.Enum.GetValues(typeof(ControllerModes)).Length);
            }

            _currentState = (ControllerModes)nextMode;
            _selectedListObject = 0;
            ChangeSelectedObject(0);

            if (_currentState == ControllerModes.CAMERAS_MODE)
            {
                _buttonSelectorPrefabSnapSelect = GameObject.Find("ButtonSelectorPrefab(Clone)").GetComponent<SnapSelect>();
            }
        }

        //!
        //! Switches to the previous controller mode.
        //!
        private void SwitchToPreviousMode()
        {
            int previousMode = ((int)_currentState - 1 + Enum.GetValues(typeof(ControllerModes)).Length) %
                               (Enum.GetValues(typeof(ControllerModes)).Length);

            // Loop until a non-empty mode is found
            while (IsListEmpty((ControllerModes)previousMode))
            {
                previousMode = (previousMode - 1 + Enum.GetValues(typeof(ControllerModes)).Length) %
                               (Enum.GetValues(typeof(ControllerModes)).Length);
            }

            _currentState = (ControllerModes)previousMode;
            _selectedListObject = 0;
            ChangeSelectedObject(0);

            if (_currentState == ControllerModes.CAMERAS_MODE)
            {
                _buttonSelectorPrefabSnapSelect = GameObject.Find("ButtonSelectorPrefab(Clone)").GetComponent<SnapSelect>();
            }
        }

        //!
        //! Checks if the list for a specific mode is empty.
        //!
        private bool IsListEmpty(ControllerModes mode)
        {
            switch (mode)
            {
                case ControllerModes.OBJECT_MODE:
                    return _sceneObjectsList.Count == 0;

                case ControllerModes.LIGHT_MODE:
                    return _sceneObjectLightsList.Count == 0;

                case ControllerModes.CAMERAS_MODE:
                    return _sceneObjectCamerasList.Count == 0;

                default:
                    return false;
            }
        }
        #endregion
        
        #region ControllerInputs
        //!
        //! Handles the "North" button press on the controller.
        //!
        private void PressNorth(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                core.getManager<SceneManager>().getModule<UndoRedoModule>().undoStep();
            }
        }

        //!
        //! Handles the "South" button press on the controller.
        //!
        private void PressSouth(object sender, float e)
        {
            if (_currentState == ControllerModes.MAIN_VIEW_MODE && _isCrosshairOn)
            {
                SelectSceneObjectWithRaycastAndButton();
                return;
            }

            if (_currentState == ControllerModes.CAMERAS_MODE && !_lookThroughOn)
            {
                _uiManager.getButton("CameraSelectionButton").action.Invoke();
                _uiManager.getButton("CameraSelectionButton").showHighlighted(true);

                _lookThroughOn = true;
            }
            else if (_lookThroughOn)
            {
                _uiManager.getButton("CameraSelectionButton").action.Invoke();
                _uiManager.getButton("CameraSelectionButton").showHighlighted(false);

                SwitchToDefaultMode();
                _lookThroughOn = false;
            }
        }

        //!
        //! Handles the "East" button press on the controller.
        //!
        private void PressEast(object sender, float e)
        {
            if (_lookThroughOn)
            {
                _uiManager.getButton("CameraSelectionButton").action.Invoke();
                _uiManager.getButton("CameraSelectionButton").showHighlighted(false);
                _lookThroughOn = false;
            }

            SwitchToDefaultMode();
        }

        //!
        //! Handles the "West" button press on the controller.
        //!
        private void PressWest(object sender, float e)
        {
            if (_currentState == ControllerModes.MAIN_VIEW_MODE)
            {
                OnOrOffCrosshair();
            }

            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                core.getManager<SceneManager>().getModule<UndoRedoModule>().redoStep();
            }
        }

        //!
        //! Handles the "Up" button press on the controller.
        //!
        private void PressUp(object sender, float e)
        {
            ChangeSelectedObject(-1);
        }

        //!
        //! Handles the "Down" button press on the controller.
        //!
        private void PressDown(object sender, float e)
        {
            ChangeSelectedObject(1);
        }

        //!
        //! Handles the "Left" button press on the controller.
        //!
        private void PressLeft(object sender, float e)
        {
            /*if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToPreviousSpinnerMode();
            }*/
            
            // Handle left button press.
        }

        //!
        //! Handles the "Right" button press on the controller.
        //!
        private void PressRight(object sender, float e)
        {
            
            /*if (_currentState != ControllerModes.MAIN_VIEW_MODE)
               {
                   SwitchToNextSpinnerMode();
               }*/
            
            // Handle right button press.
        }

        //!
        //! Handles the "Left Trigger" button press on the controller.
        //!
        private void PressLeftTrigger(object sender, float e)
        {
            SwitchToPreviousMode();
            _isCrosshairOn = false;
        }

        //!
        //! Handles the "Right Trigger" button press on the controller.
        //!
        private void PressRightTrigger(object sender, float e)
        {
            SwitchToNextMode();
            _isCrosshairOn = false;
        }

        //!
        //! Handles the "Left Shoulder" button press on the controller.
        //!
        private void PressLeftShoulder(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToPreviousManipulationMode();
            }
        }

        //!
        //! Handles the "Right Shoulder" button press on the controller.
        //!
        private void PressRightShoulder(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToNextManipulationMode();
            }
        }

        //!
        //! Handles the left controller stick movement.
        //!
        //! @param value The vector representing the stick movement.
        //!
        private void MoveLeftStick(object sender, Vector2 value)
        {
            _leftStickValue = value;
        }

        //!
        //! Handles the right controller stick movement.
        //!
        //! @param value The vector representing the stick movement.
        //!
        private void MoveRightStick(object sender, Vector2 value)
        {
            _rightStickValue = value;
        }

        #endregion

        public ControllerModule(string name, Manager manager) : base(name, manager)
        {
            
        }
        
        //!
        //! Tracer update function
        //!
        private void TracerUpdate(object sender, EventArgs e)
        {
            if (_isCrosshairOn)
            {
                CrosshairChangeColor();
            }

            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                // Get the camera's forward and right vectors in world space
                Vector3 cameraForward = _mainCamera.transform.forward;
                Vector3 cameraRight = _mainCamera.transform.right;

                // Normalize the vectors
                cameraForward.Normalize();
                cameraRight.Normalize();

                // Calculate the movement direction based on the camera's orientation
                Vector3 movementDirection = cameraForward * _leftStickValue.y + cameraRight * _leftStickValue.x;
                movementDirection.y = _rightStickValue.y;

                // Apply the movement
                _result = movementDirection * (Speed * Time.deltaTime);

                switch (_selectedAbstractParam.name)
                {
                    case "position":
                    case "scale":
                        Parameter<Vector3> paramVec3 = (Parameter<Vector3>)_selectedAbstractParam;
                        Vector3 valVec3 = paramVec3.value;
                        //_result = new Vector3(_leftStickValue.x, _rightStickValue.y, _leftStickValue.y) * (Speed * Time.deltaTime);
                        if (paramVec3.value + _result != paramVec3.value)
                        {
                            paramVec3.setValue(paramVec3.value + _result);
                        }
                        //_selectedAbstractParam. = _result;
                        break;
                    case "rotation":
                        Parameter<Quaternion> paramQuat = (Parameter<Quaternion>)_selectedAbstractParam;
                        Quaternion rot = paramQuat.value;
                        rot = Quaternion.Euler(_leftStickValue.x, _rightStickValue.y, _leftStickValue.y);
                        if (paramQuat.value * rot != paramQuat.value)
                        {
                            paramQuat.setValue(paramQuat.value * rot);
                        }
                        break;
                    case "sensorSize":
                        Parameter<Vector2> paramVec2 = (Parameter<Vector2>)_selectedAbstractParam;
                        //result = new Vector3(_leftStickValue.x, _rightStickValue.y, _leftStickValue.y) * (Speed * Time.deltaTime);
                        Vector2 valVec2 = new Vector2(_result.x, _result.z);
                        if (paramVec2.value + valVec2 != paramVec2.value)
                        {
                            paramVec2.setValue(paramVec2.value + valVec2);
                        }
                        break;
                    case "color":
                        _colorSelect.controllerManipulator(new Vector3(_leftStickValue.x, _leftStickValue.y, _rightStickValue.y));
                        break;
                    default:
                        Parameter<float> paramFlo = (Parameter<float>)_selectedAbstractParam;
                        paramFlo.setValue(paramFlo.value + _rightStickValue.y);
                        paramFlo.setValue(paramFlo.value + _leftStickValue.y);
                        break;
                }
            }
            else
            {
                _result = new Vector3(_leftStickValue.x, _rightStickValue.y, _leftStickValue.y) * (Speed * Time.deltaTime);
                float rotationAmount = _rightStickValue.x * RptationSpeed * Time.deltaTime;
                _mainCamera.transform.Rotate(Vector3.up, rotationAmount);
                _mainCamera.transform.Translate(_result);
            }
            //_selectedObject.transform.Translate(_result);
        }
        
        //!
        //! Handles the retrieval of the ColorSelect component.
        //!
        private void GetColorSelect(object sender, GameObject go)
        {
            _colorSelect = go.GetComponent<ColorSelect>();
        }

        
        
        #region Crosshair Logic
        
        //!
        //! If the crosshair is off, it is created and displayed. If it's already on, it is destroyed.
        //!
        private void OnOrOffCrosshair()
        {
            if (!_isCrosshairOn)
            {
                _controllerCanvas = Object.Instantiate(_controllerCanvasPrefab, _camera.transform);
                _crossHairImg = _controllerCanvas.GetComponentInChildren<Image>();
                _isCrosshairOn = true;
            }
            else
            {
                Object.DestroyImmediate(_controllerCanvas);
                _isCrosshairOn = false;
            }
        }
        
        //!
        //! If the crosshair is on, it is immediately destroyed.
        //!
        private void OffCrosshair()
        {
            if (_isCrosshairOn)
            {
                Object.DestroyImmediate(_controllerCanvas);
                _isCrosshairOn = false;
            }
        }
        
        //!
        //! Turns off the crosshair, clears the selected object in the UI manager, and initiates controller selection.
        //!
        private void SelectSceneObjectWithRaycastAndButton()
        {
            OffCrosshair();
            _uiManager.clearSelectedObject();
            manager.ControllerSelect(new Vector2(Screen.width / 2, Screen.height / 2));
        }
        
        //!
        //! This method adjusts the color and scale of the crosshair based on raycasting and the object hit.
        //!
        private void CrosshairChangeColor()
        {
            _ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            _selectionModule.isRenderActive = true;

            if (Physics.Raycast(_ray, out _hit))
            {
                if (_hit.transform.gameObject.GetComponent<SceneObject>() || _hit.transform.gameObject.GetComponent<IconUpdate>())
                {
                    _crossHairImg.color = Color.magenta;
                    _crossHairImg.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
                }
            }
            else if (_selectionModule.GetSelectableAtPixel(new Vector2(Screen.width / 2, Screen.height / 2)))
            {
                _crossHairImg.color = Color.magenta;
                _crossHairImg.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            }
            else
            {
                _crossHairImg.color = Color.green;
                _crossHairImg.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
        }
        
        #endregion



        #region Selection Logic
        //!
        //! This method adjusts the selected object based on the current controller mode and a given value.
        //!
        private void ChangeSelectedObject(int val)
        {
            OffCrosshair();
            switch (_currentState)
            {
                case ControllerModes.MAIN_VIEW_MODE:
                    _uiManager.clearSelectedObject();
                    break;

                case ControllerModes.OBJECT_MODE:
                    _selectedListObject += val;

                    if (_selectedListObject > _sceneObjectsList.Count - 1)
                    {
                        _selectedListObject = 0;
                    }
                    else if (_selectedListObject == -1)
                    {
                        _selectedListObject = _sceneObjectsList.Count - 1;
                    }

                    _currentSelectedSceneObject = _sceneObjectsList[_selectedListObject];
                    SelectById(_currentSelectedSceneObject);

                    break;

                case ControllerModes.LIGHT_MODE:
                    _selectedListObject += val;

                    if (_selectedListObject > _sceneObjectLightsList.Count - 1)
                    {
                        _selectedListObject = 0;
                    }
                    else if (_selectedListObject == -1)
                    {
                        _selectedListObject = _sceneObjectLightsList.Count - 1;
                    }

                    _currentSelectedSceneObject = _sceneObjectLightsList[_selectedListObject];
                    SelectById(_currentSelectedSceneObject);
                    break;

                case ControllerModes.CAMERAS_MODE:
                    _selectedListObject += val;

                    if (_selectedListObject > _sceneObjectCamerasList.Count - 1)
                    {
                        _selectedListObject = 0;
                    }
                    else if (_selectedListObject == -1)
                    {
                        _selectedListObject = _sceneObjectCamerasList.Count - 1;
                    }

                    _currentSelectedSceneObject = _sceneObjectCamerasList[_selectedListObject];
                    SelectById(_currentSelectedSceneObject);
                    break;
            }
        }
        
        //!
        //! This method selects an object based on its type (SceneObjectCamera, SceneObjectLight, or default).
        //!
        private void SelectById(SceneObject obj)
        {
            OffCrosshair();
            _uiManager.clearSelectedObject();

            switch (obj)
            {
                case SceneObjectCamera:
                    if (_uiManager.activeRole == UIManager.Roles.EXPERT ||
                        _uiManager.activeRole == UIManager.Roles.DOP)
                        _uiManager.addSelectedObject(obj);
                    break;
                case SceneObjectLight:
                    if (_uiManager.activeRole == UIManager.Roles.EXPERT ||
                        _uiManager.activeRole == UIManager.Roles.LIGHTING ||
                        _uiManager.activeRole == UIManager.Roles.SET)
                        _uiManager.addSelectedObject(obj);
                    break;
                default:
                    if (_uiManager.activeRole == UIManager.Roles.EXPERT ||
                        _uiManager.activeRole == UIManager.Roles.SET)
                        _uiManager.addSelectedObject(obj);
                    break;
            }
        }
        
        //!
        //! This method responds to a change in the selected objects within the UI manager.
        //!
        private void UiManagerSelectionChanged(object sender, List<SceneObject> sceneObjects)
        {
            OffCrosshair();
            if (sceneObjects.Count > 0)
            {
                switch (sceneObjects[0])
                {
                    case SceneObjectCamera:
                        _currentState = ControllerModes.CAMERAS_MODE;
                        _selectedListObject = _sceneObjectCamerasList.IndexOf((SceneObjectCamera)sceneObjects[0]);
                        _currentSelectedSceneObject = sceneObjects[0];
                        break;
                    case SceneObjectLight:
                        _currentState = ControllerModes.LIGHT_MODE;
                        _selectedListObject = _sceneObjectLightsList.IndexOf((SceneObjectLight)sceneObjects[0]);
                        _currentSelectedSceneObject = sceneObjects[0];
                        break;
                    default:
                        _currentState = ControllerModes.OBJECT_MODE;
                        _selectedListObject = _sceneObjectsList.IndexOf(sceneObjects[0]);
                        _currentSelectedSceneObject = sceneObjects[0];
                        break;
                }

                if (_controllerCanvas)
                {
                    Object.DestroyImmediate(_controllerCanvas);
                }
                GetCurrentSelector();
            }
        }
        
        //!
        //! This method responds to the removal of selected objects in the UI manager.
        //!
        private void UiManagerSelectionRemoved(object sender, SceneObject sceneObject)
        {
            OffCrosshair();
            _currentState = 0;
            _selectedListObject = 0;
            _currentSelectedSceneObject = null;
            _isCrosshairOn = false;
        }
        #endregion


        #region ManipulationModeRegion
        
        //!
        //! This method retrieves the current selector when not in MAIN_VIEW_MODE.
        //!
        private void GetCurrentSelector()
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                _selectorSnapSelect = GameObject.Find("PRE_UI_AddSelector(Clone)").GetComponent<SnapSelect>();
                _selectorSnapSelect.parameterChanged += ParamChange;
                _selectorSnapSelectElementsList = _selectorSnapSelect.elements.GroupBy(element => element.buttonID).Select(group => group.First()).ToList();
                _selectorCurrentSelectedSnapSelectElement = 0;
                _selectedAbstractParam =
                    _currentSelectedSceneObject.parameterList[_selectorCurrentSelectedSnapSelectElement];
                //_selectorSnapSelect.parameterChanged += GetCurrentParameter;
                //GetSpinner();
                // List<int> uniqueButtonIDs = _snapSelectElementsList.Select(element => element.buttonID).Distinct().ToList();
            }
        }

        //!
        //! This method responds to parameter changes in the manipulation mode.
        //!
        private void ParamChange(object sender, int manipulatorMode)
        {
            _selectorCurrentSelectedSnapSelectElement = manipulatorMode;
        }

        /*private void GetSpinner()
        {
            _spinnerSnapSelect = UI2DModule.GetManipulator().GetComponent<SnapSelect>();
            _spinnerSnapSelectElementsList = _spinnerSnapSelect.elements.GroupBy(element => element.buttonID).Select(group => group.First()).ToList();
            _spinnerCurrentSelectedSnapSelectElement = 0;
            _spinnerSnapSelect.parameterChanged += GetCurrentParameter;
        }*/
        
        //!
        //! This method switches to the next available manipulation mode.
        //!
        private void SwitchToNextManipulationMode()
        {
            _selectorCurrentSelectedSnapSelectElement = (_selectorCurrentSelectedSnapSelectElement + 1) % _selectorSnapSelectElementsList.Count;
            _selectorSnapSelectElementsList[_selectorCurrentSelectedSnapSelectElement].ControllerClick();
            
            _selectedAbstractParam =
                _currentSelectedSceneObject.parameterList[_selectorCurrentSelectedSnapSelectElement];
            //GetSpinner();
        }
        
        //!
        //! This method switches to the previous available manipulation mode.
        //!
        private void SwitchToPreviousManipulationMode()
        {
            _selectorCurrentSelectedSnapSelectElement = (_selectorCurrentSelectedSnapSelectElement - 1 + _selectorSnapSelectElementsList.Count) % _selectorSnapSelectElementsList.Count;
            _selectorSnapSelectElementsList[_selectorCurrentSelectedSnapSelectElement].ControllerClick();
            
            _selectedAbstractParam =
                _currentSelectedSceneObject.parameterList[_selectorCurrentSelectedSnapSelectElement];
            //GetSpinner();
        }

        /*private void SwitchToNextSpinnerMode()
        {
            _spinnerCurrentSelectedSnapSelectElement = (_spinnerCurrentSelectedSnapSelectElement + 1) % _spinnerSnapSelectElementsList.Count;
            _spinnerSnapSelectElementsList[_spinnerCurrentSelectedSnapSelectElement].ControllerClick();   
            Debug.LogError("Controller" + _spinnerCurrentSelectedSnapSelectElement);
        }

        private void SwitchToPreviousSpinnerMode()
        {
            _spinnerCurrentSelectedSnapSelectElement = (_spinnerCurrentSelectedSnapSelectElement - 1 + _spinnerSnapSelectElementsList.Count) % _spinnerSnapSelectElementsList.Count;
            _spinnerSnapSelectElementsList[_spinnerCurrentSelectedSnapSelectElement].ControllerClick();
            Debug.LogError("Controller" + _spinnerCurrentSelectedSnapSelectElement);
        }*/
        
        /*private void GetCurrentParameter(object sender, int param)
        {

            Debug.LogError(sender + " idk wtf is this " + param);
            //AbstractParameter abstractParam = _currentSelectedSceneObject.parameterList[param];
            //AbstractParameter.ParameterType type = abstractParam.vpetType;

        }*/
        
        //!
        //! This method invokes the doneEditing event for undo/redo when an editing operation is completed.
        //!
        private void DoneEditing(object sender, Vector2 value)
        {
            if (_selectedAbstractParam != null)
            {
                ControllerdoneEditing?.Invoke(this, _selectedAbstractParam);
            }
        }
        #endregion


        

        

    }
    
   
}