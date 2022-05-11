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

//! @file "CameraSelectionModule.cs"
//! @brief Implementation of the Camera selection buttons functionality 
//! @author Simon Spielmann
//! @version 0
//! @date 27.04.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class CameraSelectionModule : SceneManagerModule
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
        //! The old parent of the selected object.
        //!
        private Transform m_oldParent;
        //!
        //! The UI button for logging the camera to an object.
        //!
        private MenuButton m_cameraSelectButton;
        //!
        //! The currently selected object.
        //!
        private SceneObject m_selectedObject = null;
        //!
        //! An offset for differentiate between look through light/camera and lock object to camera.
        //!
        private Vector3 m_lockOffset = Vector3.zero;
        
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public CameraSelectionModule(string name, Manager manager) : base(name, manager)
        {        
        }

        //! 
        //! Function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);

            MenuButton cameraSelectButton = new MenuButton("Select Camera", selectNextCamera);
            cameraSelectButton.setIcon("Images/CameraIcon");
            
            core.getManager<UIManager>().addButton(cameraSelectButton);

            manager.sceneReady += copyCamera;
            core.getManager<UIManager>().selectionChanged += createButtons;
        }

        //! 
        //! Function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            manager.sceneReady -= copyCamera;
            core.getManager<UIManager>().selectionChanged -= createButtons;
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
                    sceneObjects[0].GetType() == typeof(SceneObjectPointLight))
                {
                    m_lockOffset = Vector3.zero;
                    m_cameraSelectButton = new MenuButton("Look through", lockToCamera);
                    m_cameraSelectButton.setIcon("Images/CameraIcon");
                }
                else
                {
                    m_lockOffset = new Vector3(0, 0, -5f);
                    m_cameraSelectButton = new MenuButton("Lock to Camera", lockToCamera);
                    m_cameraSelectButton.setIcon("Images/CameraIcon");
                }
                uiManager.addButton(m_cameraSelectButton);
            }
            else 
            {
                if (m_isLocked)
                    lockToCamera();

                m_selectedObject = null;
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
                    m_selectedObject.transform.parent = m_oldParent;
                    //copyCamera(this, EventArgs.Empty);
                    m_isLocked = false;
                }
                else
                {
                    Camera mainCamera = Camera.main;

                    mainCamera.transform.position = m_selectedObject.transform.position + m_lockOffset;
                    mainCamera.transform.rotation = m_selectedObject.transform.rotation;

                    m_oldParent = m_selectedObject.transform.parent;
                    m_selectedObject.transform.parent = mainCamera.transform;

                    m_isLocked = true;
                }
            }
        }

        //!
        //! The function that cycles through the available cameras in scene and set the camera main transform to these camera transform. 
        //!
        private void selectNextCamera()
        {
            m_cameraIndex++;

            if (m_isLocked)
                lockToCamera();

            if (m_cameraIndex > manager.sceneCameraList.Count - 1)
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
            if (manager.sceneCameraList.Count > 0)
            {
                Camera mainCamera = Camera.main;
                int targetDisplay = mainCamera.targetDisplay;
                float aspect = mainCamera.aspect;
                Camera newCamera = manager.sceneCameraList[m_cameraIndex].GetComponent<Camera>();
                mainCamera.enabled = false;
                mainCamera.CopyFrom(newCamera);
                mainCamera.targetDisplay = targetDisplay;
                mainCamera.aspect = aspect;
                mainCamera.enabled = true;
            }
        }
    }
}
