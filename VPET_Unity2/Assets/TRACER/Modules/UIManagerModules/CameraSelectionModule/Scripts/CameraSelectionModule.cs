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

//! @file "CameraSelectionModule.cs"
//! @brief Implementation of the Camera selection buttons functionality 
//! @author Simon Spielmann
//! @version 0
//! @date 27.04.2022

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace tracer
{
    public class CameraSelectionModule : UIManagerModule
    {
        //!
        //! Flag determining if the camera is locked to an object.
        //!
        private bool m_isLocked = false;
        //!
        //! The index of the currently selected camera.
        //!
        private int m_cameraIndex = 0;
        //!
        //! The initial position of the selected object.
        //!
        private Vector3 m_oldPosition = Vector3.zero;
        //!
        //! The initial rotation of the selected object.
        //!
        private Quaternion m_oldRotation = Quaternion.identity;
        //!
        //! The initial rotation of the main camera.
        //!
        private Quaternion m_oldCamRotation = Quaternion.identity;
        //!
        //! The inverse initial posiotion of the main camara.
        //!
        private Quaternion m_inverseOldCamRotation = Quaternion.identity;
        //!
        //! The initial vector between the main camera and the selected object.
        //!
        private Vector3 m_positionOffset = Vector3.zero;
        //!
        //! The UI button for logging the camera to an object.
        //!
        private MenuButton m_cameraSelectButton;
        //!
        //! The currently selected object.
        //!
        private SceneObject m_selectedObject = null;
        //!
        //! A reference to the scene manager.
        //!
        private SceneManager m_sceneManager;
        //!
        //! A reference to the input manager.
        //!
        private InputManager m_inputManager;
        //!
        //! The preloaded prafab of the safe frame overlay game object.
        //!
        private GameObject m_safeFramePrefab;
        //!
        //! The instance of the the safe frame overlay.
        //!
        private GameObject m_safeFrame = null;
        //!
        //! The scaler of the safe frame.
        //!
        private Transform m_scaler = null;
        //!
        //! The text of the safe frame.
        //!
        private TextMeshProUGUI m_infoText = null;
        //!
        //! A copy of the last selected camera.
        //!
        private SceneObjectCamera m_oldSOCamera = null;

        //!
        //! safe frame button
        //!
        MenuButton m_safeFrameButton = null;

        //!
        //! Event emitted when camera operations are in action
        //!
        public event EventHandler<bool> uiCameraOperation;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the TRACER core
        //!
        public CameraSelectionModule(string name, Manager manager) : base(name, manager)
        {        
        }

        //! 
        //! Function called before Unity destroys the TRACER core.
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);

            m_sceneManager = core.getManager<SceneManager>();
            m_inputManager = core.getManager<InputManager>();

            m_safeFramePrefab = Resources.Load("Prefabs/SafeFrame") as GameObject;
            m_safeFrameButton = new MenuButton("", showSafeFrame, new List<UIManager.Roles>() { UIManager.Roles.DOP });
            m_safeFrameButton.setIcon("Images/button_safeFrames");

            MenuButton cameraSelectButton = new MenuButton("", selectNextCamera, new List<UIManager.Roles>() { UIManager.Roles.DOP });
            cameraSelectButton.setIcon("Images/button_camera");
            cameraSelectButton.isToggle = true;

            core.getManager<UIManager>().addButton(m_safeFrameButton);
            core.getManager<UIManager>().addButton(cameraSelectButton);

            m_sceneManager.sceneReady += copyCamera;
            core.getManager<UIManager>().selectionChanged += createButtons;

            core.syncEvent += updateTrigger;
            m_inputManager.cameraControlChanged += updateSafeFrame;
        }

        //! 
        //! Function called before Unity destroys the TRACER core.
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            m_sceneManager.sceneReady -= copyCamera;
            core.getManager<UIManager>().selectionChanged -= createButtons;
            core.syncEvent -= updateTrigger;
            m_inputManager.cameraControlChanged -= updateSafeFrame;
        }

        //!
        //! Function that creates the camera selection ui buttons. Called every time a scene object has been selected.
        //!
        //! @param sender The UI manager.
        //! @param sceneObjects a list of the currently selected objects.
        //!
        private void createButtons(object sender, List<SceneObject> sceneObjects)
        {
            UIManager uiManager = core.getManager<UIManager>();

            if (m_cameraSelectButton != null)
            {
                uiManager.removeButton(m_cameraSelectButton);
                m_cameraSelectButton = null;
            }

            if (sceneObjects.Count > 0)
            {
                m_selectedObject = sceneObjects[0];
                if (sceneObjects[0].GetType() == typeof(SceneObjectCamera) ||
                    sceneObjects[0].GetType() == typeof(SceneObjectDirectionalLight) ||
                    sceneObjects[0].GetType() == typeof(SceneObjectSpotLight))
                {
                    m_cameraSelectButton = new MenuButton("", lookThrough);
                    m_cameraSelectButton.setIcon("Images/button_lookTrough");
                }
                else
                {
                    m_cameraSelectButton = new MenuButton("", lockToCamera);
                    m_cameraSelectButton.setIcon("Images/button_lockToCamera");
                }
                uiManager.addButton(m_cameraSelectButton);
            }
            else 
            {
                if (m_isLocked)
                {
                    core.updateEvent -= updateLookThrough;
                    core.updateEvent -= updateLockToCamera;
                    m_isLocked = false;
                    uiCameraOperation.Invoke(this, m_isLocked);
                }

                m_selectedObject = null;
            }
        }

        //!
        //! The function that moves the main camera to the selected object and parants it to the camera.
        //!
        private void lookThrough()
        {
            if (m_selectedObject != null)
            {
                InputManager inputManager = core.getManager<InputManager>();

                if (m_isLocked)
                {
                    core.updateEvent -= updateLookThrough;
                    m_isLocked = false;
                }
                else
                {
                    Camera.main.transform.position = m_selectedObject.transform.position;
                    Camera.main.transform.rotation = m_selectedObject.transform.rotation;
                    m_oldPosition = Vector3.zero;
                    m_oldRotation = Quaternion.identity;
                    m_inverseOldCamRotation = Quaternion.identity;

                    m_positionOffset = Vector3.zero;
                    m_oldRotation = Quaternion.identity;

                    if (inputManager.cameraControl == InputManager.CameraControl.ATTITUDE)
                        inputManager.setCameraAttitudeOffsets();

                    core.updateEvent += updateLookThrough;
                    m_isLocked = true;
                }

                uiCameraOperation.Invoke(this, m_isLocked);
            }
        }

        //!
        //! The function that moves the main camera to the selected object and parants it to the camera.
        //!
        private void lockToCamera()
        {
            if (m_selectedObject != null)
            {
                if (m_isLocked)
                {
                    core.updateEvent -= updateLockToCamera;
                    m_isLocked = false;
                }
                else
                {
                    m_positionOffset = m_selectedObject.transform.position - Camera.main.transform.position;
                    m_oldPosition = m_selectedObject.transform.position;
                    m_oldRotation = m_selectedObject.transform.rotation;
                    m_oldCamRotation = Camera.main.transform.rotation;
                    m_inverseOldCamRotation = Quaternion.Inverse(Camera.main.transform.rotation);
                    core.updateEvent += updateLockToCamera;
                    m_isLocked = true;
                }

                uiCameraOperation.Invoke(this, m_isLocked);
            }
        }

        //!
        //! Toggles the safe frame overlay.
        //!
        private void showSafeFrame()
        {

            if (m_safeFrame == null)
            {
                m_safeFrame = GameObject.Instantiate(m_safeFramePrefab, Camera.main.transform);
                CanvasScaler scaler =  m_safeFrame.GetComponent<CanvasScaler>();
                float physicalDeviceScale = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi / 12f;
                scaler.scaleFactor = Screen.dpi * 0.04f * Mathf.Min(Mathf.Max(manager.settings.uiScale.value, 0.4f), 3f) * physicalDeviceScale;
                m_infoText = m_safeFrame.transform.FindDeepChild("InfoText").GetComponent<TextMeshProUGUI>();
                m_scaler = m_safeFrame.transform.Find("scaler");
                if (m_oldSOCamera)
                    updateCamera(m_oldSOCamera, null);
            }
            else
            {
                GameObject.DestroyImmediate(m_safeFrame);
                m_safeFrame = null;
            }
        }

        //!
        //! update safeFrame
        //!
        private void updateSafeFrame(object sender, InputManager.CameraControl c)
        {
            if (c == InputManager.CameraControl.AR)
            {
                m_safeFrameButton.showHighlighted(false);
                GameObject.DestroyImmediate(m_safeFrame);
                m_safeFrame = null;
                core.getManager<UIManager>().removeButton(m_safeFrameButton);

            }
            else
            {
                if(!core.getManager<UIManager>().getButtons().Contains(m_safeFrameButton))
                    core.getManager<UIManager>().addButton(m_safeFrameButton);
            }
        }

        private void updateTrigger(object sender, byte data)
        {
            if (m_oldSOCamera)
                updateCamera(m_oldSOCamera, null);
        }

        //!
        //! Function for updating the aspect ratio of the safe frame based on the currently selected camera.
        //!
        private void updateCamera(object sender, AbstractParameter parameter)
        {
            Camera cameraMain = Camera.main;
            SceneObjectCamera soCamera = (SceneObjectCamera) sender;

            cameraMain.fieldOfView = soCamera.fov.value;
            cameraMain.sensorSize = soCamera.sensorSize.value;

            if (m_safeFrame)
            {
                string camInfo = String.Format("{0}mm | f/{1} | {2}:{3}mm | {4:0.00} fps", cameraMain.focalLength, soCamera.aperture.value, cameraMain.sensorSize.x, cameraMain.sensorSize.y, 1.0f / Time.deltaTime);
                float newAspect = cameraMain.sensorSize.x / cameraMain.sensorSize.y;

                if (newAspect < cameraMain.aspect)
                    m_scaler.localScale = new Vector3(1f / cameraMain.aspect * (cameraMain.sensorSize.x / cameraMain.sensorSize.y), 1f, 1f);
                else
                    m_scaler.localScale = new Vector3(1f, cameraMain.aspect / (cameraMain.sensorSize.x / cameraMain.sensorSize.y), 1f);

                m_infoText.text = camInfo;
            }
        }

        //!
        //! The function that cycles through the available cameras in scene and set the camera main transform to these camera transform. 
        //!
        private void selectNextCamera()
        {
            m_cameraIndex++;

            if (m_isLocked)
            {
                core.updateEvent -= updateLockToCamera;
                core.updateEvent -= updateLookThrough;
                m_isLocked = false;

                uiCameraOperation?.Invoke(this, m_isLocked);
                m_cameraSelectButton.showHighlighted(false);
            }

            if (m_cameraIndex > m_sceneManager.sceneCameraList.Count - 1)
                m_cameraIndex = 0;

            // copy properties to main camera and set it use display 1 (0)
            copyCamera(this, EventArgs.Empty);

            InputManager inputManager = core.getManager<InputManager>();
            if (inputManager.cameraControl == InputManager.CameraControl.ATTITUDE)
                inputManager.setCameraAttitudeOffsets();
        }

        //!
        //! Function that copies the selected cameras attributes to the main camera.
        //!
        private void copyCamera(object sender, EventArgs e)
        {
            if (m_sceneManager.sceneCameraList.Count > 0)
            {
                if (m_oldSOCamera)
                {
                    m_oldSOCamera.hasChanged -= updateCamera;
                }
                Camera mainCamera = Camera.main;
                int targetDisplay = mainCamera.targetDisplay;
                float aspect = mainCamera.aspect;
                SceneObjectCamera soCamera = m_sceneManager.sceneCameraList[m_cameraIndex];
                Camera newCamera = soCamera.GetComponent<Camera>();
                soCamera.hasChanged += updateCamera;
                Debug.Log(soCamera.name + " Camera linked.");
                m_oldSOCamera = soCamera;
                mainCamera.enabled = false;
                mainCamera.CopyFrom(newCamera);
                mainCamera.targetDisplay = targetDisplay;
                mainCamera.aspect = aspect;
                mainCamera.enabled = true;

                updateCamera(soCamera, null);

                // announce the UI operation to the input manager
                m_inputManager.updateCameraCommand();
            }
        }

        //!
        //! Function that updates based on the main cameras transformation the selectet objects transformation by using a look through metaphor.
        //!
        private void updateLookThrough(object sender, EventArgs e)
        {
            Transform camTransform = Camera.main.transform;
            Transform objTransform = m_selectedObject.transform;

            switch (m_inputManager.cameraControl)
            {
                case InputManager.CameraControl.ATTITUDE: 
                case InputManager.CameraControl.AR:
                case InputManager.CameraControl.TOUCH:
                    Vector3 newPosition = camTransform.position - objTransform.parent.position;
                    Quaternion newRotation = camTransform.rotation * Quaternion.Inverse(objTransform.parent.rotation);
                    m_selectedObject.position.setValue(newPosition);
                    m_selectedObject.rotation.setValue(newRotation);
                    break;
                default:
                    camTransform.position = objTransform.position;
                    camTransform.rotation = objTransform.rotation;
                    break;
            }
        }

        //!
        //! Function that updates based on the main cameras transformation the selectet objects transformation by using a grab and move metaphor.
        //!
        private void updateLockToCamera(object sender, EventArgs e)
        {
            Transform camTransform = Camera.main.transform;
            Transform objTransform = m_selectedObject.transform;

            switch (m_inputManager.cameraControl)
            {
                case InputManager.CameraControl.ATTITUDE:
                case InputManager.CameraControl.AR:
                case InputManager.CameraControl.TOUCH:
                    Quaternion camRotationOffset = camTransform.rotation * m_inverseOldCamRotation;
                    Vector3 newPosition = camRotationOffset * (m_oldPosition - camTransform.position) + camTransform.position;
                    m_selectedObject.position.setValue(objTransform.parent.InverseTransformPoint(newPosition));
                    m_selectedObject.rotation.setValue(Quaternion.Inverse(objTransform.parent.rotation) * (camRotationOffset * m_oldRotation));
                    break;
                default:
                    Quaternion newRotation = objTransform.rotation * Quaternion.Inverse(m_oldRotation);
                    camTransform.position = newRotation * (- m_positionOffset) + objTransform.position;
                    camTransform.rotation = newRotation * m_oldCamRotation;
                    break;
            }
        }
    }
}
