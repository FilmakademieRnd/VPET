using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tracer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace tracer
{

    public class ControllerTest : MonoBehaviour
    {
        [SerializeField] private GameObject selectedObject;
        [SerializeField] private GameObject mainCamera;
        [SerializeField] private GameObject crosshair;
        
        private Camera _camera;

        private Core _core;
        private UIManager _uiManager;
        private SceneManager _sceneManager;

        private List<SceneObject> _sceneObjectsList;
        private List<SceneObjectLight> _sceneObjectLightsList;
        private List<SceneObjectCamera> _sceneObjectCamerasList;

        private Vector2 _leftStickValue;
        private Vector2 _rightStickValue;
        private Vector3 _result;

        private int _selectedListObject;

        private bool _isCrossharOn;

        private readonly float _speed = 3f;

        private Ray _ray;
        private RaycastHit _hit;

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
        }

        private void SwitchToPreviousMode()
        {
            int previousMode = ((int)_currentState - 1 + System.Enum.GetValues(typeof(ControllerModes)).Length) %
                               (System.Enum.GetValues(typeof(ControllerModes)).Length);

            // Loop until a non-empty mode is found
            while (IsListEmpty((ControllerModes)previousMode))
            {
                previousMode = (previousMode - 1 + System.Enum.GetValues(typeof(ControllerModes)).Length) %
                               (System.Enum.GetValues(typeof(ControllerModes)).Length);
            }

            _currentState = (ControllerModes)previousMode;
            _selectedListObject = 0;
            ChangeSelectedObject(0);
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

        private void Awake()
        {
            mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            _camera = mainCamera.GetComponent<Camera>();
            selectedObject = mainCamera;
            _core = GameObject.FindGameObjectsWithTag("Core")[0].GetComponent<Core>();
            _sceneManager = _core.getManager<SceneManager>();
            _uiManager = _core.getManager<UIManager>();
            _sceneObjectsList = _sceneManager.simpleSceneObjectList;
            _sceneObjectLightsList = _sceneManager.sceneLightList;
            _sceneObjectCamerasList = _sceneManager.sceneCameraList;
            _core.updateEvent += TracerUpdate;
            _core.updateEvent += TracerUpdate;
        }

        private void TracerUpdate(object sender, EventArgs e)
        {
            // TODO manipulation based on selected Manipulation Moode; 
            _result = new Vector3(_leftStickValue.x, _rightStickValue.y, _leftStickValue.y) * (_speed * Time.deltaTime);
            selectedObject.transform.Translate(_result);
        }

        #region ControllerInputs

        private void OnController_North(InputValue value)
        {
            //Debug.LogError("YAAAAAAAAAAAAAAA" + value.Get<float>());
        }

        private void OnController_South(InputValue value)
        {
            if (_currentState == ControllerModes.MAIN_VIEW_MODE && _isCrossharOn)
            {
                SelectSceneObjectWithRaycastAndButton();
            }

        }

        private void OnController_East(InputValue value)
        {
            SwitchToDefaultMode();
        }

        private void OnController_West(InputValue value)
        {
            if (_currentState == ControllerModes.MAIN_VIEW_MODE)
            {
                OnOrOffCrosshair();
            }

        }

        private void OnController_Up(InputValue value)
        {
            ChangeSelectedObject(-1);
        }

        private void OnController_Down(InputValue value)
        {
            ChangeSelectedObject(1);
        }

        private void OnController_Left_Trigger(InputValue value)
        {
            SwitchToPreviousMode();
        }

        private void OnController_Right_Trigger(InputValue value)
        {
            SwitchToNextMode();
        }

        private void OnController_Left_Stick(InputValue value)
        {
            _leftStickValue = value.Get<Vector2>();
        }

        private void OnController_Right_Stick(InputValue value)
        {
            _rightStickValue = value.Get<Vector2>();
        }

        #endregion

        #region CrosshairLogic

        private void OnOrOffCrosshair()
        {
            if (!_isCrossharOn)
            {
                crosshair.gameObject.SetActive(true);
                _isCrossharOn = true;
            }
            else
            {
                crosshair.gameObject.SetActive(false);
                _isCrossharOn = false;
            }
        }

        private void SelectSceneObjectWithRaycastAndButton()
        {
            if (Physics.Raycast(_ray, out _hit ,  Mathf.Infinity))
            {
                if (_hit.transform.gameObject.GetComponent<SceneObject>())
                {
                    Debug.LogError(_hit.transform.gameObject);
                }

                /*if (_hit.transform.gameObject.GetComponent<IconUpdate>())
                {
                    Debug.LogError("Camera");
                }*/
            }
            
            /*
            public SceneObject GetSelectableAtCollider(Vector2 screenPosition)
            {
                RaycastHit hit;
                SceneObject sceneObject = null;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity))
                {
                    GameObject gameObject = hit.collider.gameObject;
                    IconUpdate iconUpdate = gameObject.GetComponent<IconUpdate>();
                    // check if an icon has been hit
                    if (iconUpdate)
                        sceneObject = iconUpdate.m_parentObject;

                    if (!sceneObject)
                        sceneObject = gameObject.GetComponent<SceneObject>();

                    if (!sceneObject)
                        sceneObject = gameObject.transform.parent.GetComponent<SceneObject>();

                }

                return sceneObject;
            }
            */


        }

        #endregion


        private void ChangeSelectedObject(int val)
        {
            switch (_currentState)
            {
                case ControllerModes.MAIN_VIEW_MODE:
                    selectedObject = mainCamera;
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

                    selectedObject = _sceneObjectsList[_selectedListObject].gameObject;
                    SelectById(selectedObject.GetComponent<SceneObject>());

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

                    selectedObject = _sceneObjectLightsList[_selectedListObject].gameObject;
                    SelectById(selectedObject.GetComponent<SceneObject>());
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

                    selectedObject = _sceneObjectCamerasList[_selectedListObject].gameObject;
                    SelectById(selectedObject.GetComponent<SceneObject>());
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

        void Update()
        {

            if (_isCrossharOn)
            {
                _ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0) );
            }



            /*switch (_currentState)
            {
                case ControllerModes.MAIN_VIEW_MODE:
                    Debug.Log("Main View Mode");
                    // Add your main view mode logic here
                    break;

                case ControllerModes.OBJECT_MODE:
                    Debug.Log("Object Mode");
                    // Add your object mode logic here
                    break;

                case ControllerModes.LIGHT_MODE:
                    Debug.Log("Light Mode");
                    // Add your light mode logic here
                    break;

                case ControllerModes.CAMERAS_MODE:
                    Debug.Log("Cameras Mode");
                    // Add your cameras mode logic here
                    break;
            }*/
        }

        private void FixedUpdate()
        {
            ManipulationLogic();
        }


        private void ManipulationLogic()
        {

        }
    }
}