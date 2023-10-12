using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tracer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace tracer
{

    public class ControllerModule : InputManagerModule
    {

        // TODO MOVE everything from controller test here 
        // TODO in Input manager create events for all the buttons 
        private GameObject _selectedObject;
        private GameObject _mainCamera;
        private GameObject _crosshair;
        private GameObject _controllerCanvasPrefab;
        private GameObject _controllerCanvas;
        private GameObject _currentAddSelector;

        private SceneObject _currentSelectedSceneObject;

        private SnapSelect _selectorSnapSelect;
        private SnapSelect _spinnerSnapSelect;
        private SnapSelect _buttonSelectorPrefabSnapSelect;//(Clone)

        private Camera _camera;

        private UIManager _uiManager;
        private SceneManager _sceneManager;
        private InputManager _inputManager;

        private CameraSelectionModule _cameraSelectionModule;
        private SelectionModule _selectionModule;

        private List<SceneObject> _sceneObjectsList;
        private List<SceneObjectLight> _sceneObjectLightsList;
        private List<SceneObjectCamera> _sceneObjectCamerasList;
        private List<SnapSelectElement> _selectorSnapSelectElementsList; 
        private List<SnapSelectElement> _spinnerSnapSelectElementsList; 
        
        private int _selectorCurrentSelectedSnapSelectElement = 0;
        private int _spinnerCurrentSelectedSnapSelectElement = 0;
        private int _selectedListObject;

        private Vector2 _leftStickValue;
        private Vector2 _rightStickValue;
        private Vector3 _result;
        
        private bool _isCrosshairOn;
        private bool _lookThroughOn;

        private const float Speed = 3f;
        private const float RptationSpeed = 100f;

        private Ray _ray;
        private RaycastHit _hit;

        private UICreator2DModule UI2DModule;

        private AbstractParameter _selectedAbstractParam;
        
        public event EventHandler<AbstractParameter> ControllerdoneEditing;


        
        
        protected override void Init(object sender, EventArgs e)
        {
            _controllerCanvasPrefab = Resources.Load("Prefabs/ControllerCanvas") as GameObject;
            _mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            _camera = _mainCamera.GetComponent<Camera>();
            _selectedObject = _mainCamera;
            _sceneManager = core.getManager<SceneManager>();
            _uiManager = core.getManager<UIManager>();
            _inputManager = core.getManager<InputManager>();
            UI2DModule = _uiManager.getModule<UICreator2DModule>();
            _selectionModule = _uiManager.getModule<SelectionModule>();
            _cameraSelectionModule = _uiManager.getModule<CameraSelectionModule>();
            
            ControllerdoneEditing += _sceneManager.getModule<UndoRedoModule>().addHistoryStep;
            
            _sceneObjectsList = _sceneManager.simpleSceneObjectList;
            _sceneObjectLightsList = _sceneManager.sceneLightList;
            _sceneObjectCamerasList = _sceneManager.sceneCameraList;

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

            core.updateEvent += TracerUpdate;

            _uiManager.selectionChanged += UiManagerSelectionChanged;
            _uiManager.selectionRemoved += UiManagerSelectionRemoved;
            //UI2DModule.parameterChanged += GetSpinner;

        }

        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);
            
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

            core.updateEvent -= TracerUpdate;
            
            _uiManager.selectionChanged -= UiManagerSelectionChanged;
            _uiManager.selectionRemoved -= UiManagerSelectionRemoved;
            
            ControllerdoneEditing -= _sceneManager.getModule<UndoRedoModule>().addHistoryStep;
        }

        #region StateMachineLogic

        private enum ControllerModes
        {
            MAIN_VIEW_MODE,
            OBJECT_MODE,
            LIGHT_MODE,
            CAMERAS_MODE
        }

        private ControllerModes _currentState = ControllerModes.MAIN_VIEW_MODE;

        private void SwitchToDefaultMode()
        {
            _currentState = 0;
            _selectedListObject = 0;
            ChangeSelectedObject(0);
        }

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

        private void PressNorth(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                core.getManager<SceneManager>().getModule<UndoRedoModule>().undoStep();
            }
        }

        private void PressSouth(object sender, float e)
        {
            if (_currentState == ControllerModes.MAIN_VIEW_MODE && _isCrosshairOn)
            {
                SelectSceneObjectWithRaycastAndButton();
                return;
            }
            
            if (_currentState == ControllerModes.CAMERAS_MODE && !_lookThroughOn)
            {
                _cameraSelectionModule.lookThrough();
                //_buttonSelectorPrefabSnapSelect = GameObject.Find("ButtonSelectorPrefab(Clone)").GetComponent<SnapSelect>();
                //Debug.LogError(_cameraSelectionModule.cameraSelectButton().id);
                //foreach (var VARIABLE in _buttonSelectorPrefabSnapSelect.elements)
                //{
                   // Debug.LogError(VARIABLE.buttonID);
                //}
                
                //SnapSelectElement targetButton = _buttonSelectorPrefabSnapSelect.elements.FirstOrDefault(targetButton =>targetButton.buttonID == _cameraSelectionModule.cameraSelectButton().id);
                
                //targetButton.ControllerClick();
                _lookThroughOn = true;
            }
            else if (_lookThroughOn)
            {
                _cameraSelectionModule.lookThrough();
                SwitchToDefaultMode();
                _lookThroughOn = false;
            }


        }

        private void PressEast(object sender, float e)
        {
            if (_lookThroughOn)
            {
                _cameraSelectionModule.lookThrough();
                _lookThroughOn = false;
            }
            
            SwitchToDefaultMode();

        }

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

        private void PressUp(object sender, float e)
        {
            ChangeSelectedObject(-1);
        }

        private void PressDown(object sender, float e)
        {
            ChangeSelectedObject(1);
        }

        private void PressLeft(object sender, float e)
        {
            /*if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToPreviousSpinnerMode();
            }*/
        }

        private void PressRight(object sender, float e)
            {
                /*if (_currentState != ControllerModes.MAIN_VIEW_MODE)
                {
                    SwitchToNextSpinnerMode();
                }*/
            }

            private void PressLeftTrigger(object sender, float e)
        {
            SwitchToPreviousMode();
        }

        private void PressRightTrigger(object sender, float e)
        {
            SwitchToNextMode();
        }

        private void PressLeftShoulder(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToPreviousManipulationMode();
            }
            
        }

        private void PressRightShoulder(object sender, float e)
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                SwitchToNextManipulationMode();
            }
        }

        private void MoveLeftStick(object sender, Vector2 value)
        {
            _leftStickValue = value;
        }

        private void MoveRightStick(object sender, Vector2 value)
        {
            _rightStickValue = value;
        }

        #endregion

        public ControllerModule(string name, Manager manager) : base(name, manager)
        {
        }

        private void TracerUpdate(object sender, EventArgs e)
        {
            // TODO manipulation based on selected Manipulation Moode; 
            if (_isCrosshairOn)
            {
                _ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                //TODO make obj outline glow if hit and selectable
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
                        if (paramVec3.value + _result != paramVec3.value )
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
                        Vector2 valVec2 =new Vector2(_result.x, _result.z);
                        if (paramVec2.value + valVec2 != paramVec2.value )
                        {
                            paramVec2.setValue(paramVec2.value + valVec2);
                        }
                        break;
                    case "colour":
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

        #region Crosshair Logic
        private void OnOrOffCrosshair()
        {
            if (!_isCrosshairOn)
            {
                _controllerCanvas = Object.Instantiate(_controllerCanvasPrefab, _camera.transform);
                _isCrosshairOn = true;
            }
            else
            {
                Object.DestroyImmediate(_controllerCanvas);
                _isCrosshairOn = false;
            }
        }
        
        private void SelectSceneObjectWithRaycastAndButton()
        {
            _uiManager.clearSelectedObject();
            manager.ControllerSelect(new Vector2(Screen.width / 2, Screen.height / 2));
            
        }
        
        #endregion

        #region Selection Logic
        private void ChangeSelectedObject(int val)
        {
            switch (_currentState)
            {
                case ControllerModes.MAIN_VIEW_MODE:
                    _selectedObject = _mainCamera;
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

        private void SelectById(SceneObject obj)
        {
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

        private void UiManagerSelectionChanged(object sender, List<SceneObject> sceneObjects)
        {
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

        private void UiManagerSelectionRemoved(object sender, SceneObject sceneObject)
        {
            _currentState = 0;
            _selectedListObject = 0;
            _currentSelectedSceneObject = null;
            _isCrosshairOn = false;
        }
        #endregion

        #region ManipulationModeRegion
        private void GetCurrentSelector()
        {
            if (_currentState != ControllerModes.MAIN_VIEW_MODE)
            {
                _selectorSnapSelect = GameObject.Find("PRE_UI_AddSelector(Clone)").GetComponent<SnapSelect>();
                _selectorSnapSelectElementsList = _selectorSnapSelect.elements.GroupBy(element => element.buttonID).Select(group => group.First()).ToList();

                foreach (var variable in _selectorSnapSelect.elements)
                {
                    variable.clicked += SelectorSnapSelectElementClicked;
                }
                
                _selectorCurrentSelectedSnapSelectElement = 0;
                _selectedAbstractParam =
                    _currentSelectedSceneObject.parameterList[_selectorCurrentSelectedSnapSelectElement];
                //_selectorSnapSelect.parameterChanged += GetCurrentParameter;
                //GetSpinner();
                // List<int> uniqueButtonIDs = _snapSelectElementsList.Select(element => element.buttonID).Distinct().ToList();
            }
        }

        /*private void GetSpinner()
        {
            _spinnerSnapSelect = UI2DModule.GetManipulator().GetComponent<SnapSelect>();
            _spinnerSnapSelectElementsList = _spinnerSnapSelect.elements.GroupBy(element => element.buttonID).Select(group => group.First()).ToList();
            _spinnerCurrentSelectedSnapSelectElement = 0;
            _spinnerSnapSelect.parameterChanged += GetCurrentParameter;
        }*/
        private void SwitchToNextManipulationMode()
        {
            _selectorCurrentSelectedSnapSelectElement = (_selectorCurrentSelectedSnapSelectElement + 1) % _selectorSnapSelectElementsList.Count;
            _selectorSnapSelectElementsList[_selectorCurrentSelectedSnapSelectElement].ControllerClick();
            
            _selectedAbstractParam =
                _currentSelectedSceneObject.parameterList[_selectorCurrentSelectedSnapSelectElement];
            //GetSpinner();
        }
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
        
        private void SelectorSnapSelectElementClicked(object sender, SnapSelectElement element)
        {
            _selectorCurrentSelectedSnapSelectElement = element.buttonID;
//            Debug.LogError("elem click" + _selectorCurrentSelectedSnapSelectElement);

        }
        
        // doneEditing?.Invoke(this, abstractParam);  FOR UNDO REDO 
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