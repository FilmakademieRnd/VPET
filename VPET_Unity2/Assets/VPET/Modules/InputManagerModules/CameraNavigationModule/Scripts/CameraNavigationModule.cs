/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "CameraNavigationModule.cs"
//! @brief implementation of VPET camera navigation features
//! @author Paulo Scatena
//! @version 0
//! @date 23.03.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation of VPET camera navigation
    //!
    public class CameraNavigationModule : InputManagerModule
    {
        //!
        //! A reference to the main camera.
        //!
        private Camera m_cam;

        //!
        //! A reference to the main camera transform.
        //!
        private Transform m_camXform;

        //!
        //! Flag to specify if there are objects selected. 
        //!
        private bool m_hasSelection;
        //!
        //! The average position of the selected objects.
        //!
        private Vector3 m_selectionCenter;

        // TODO: maybe promote these variables to configuration options
        //!
        //! The speed factor for the pan movement.
        //!
        private static readonly float s_panSpeed = .005f;
        //!
        //! The speed factor for the orbit movement.
        //!
        private static readonly float s_orbitSpeed = .15f;
        //!
        //! The speed factor for the dolly movement.
        //!
        private static readonly float s_dollySpeed = .007f;

        //!
        //! Constructor.
        //!
        //! @param name Name of this module.
        //! @param core Reference to the VPET core.
        //!
        public CameraNavigationModule(string name, Manager manager) : base(name, manager)
        {

        }

        //! 
        //! Init callback for the CameraNavigation module.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            m_cam = Camera.main;
            m_camXform = m_cam.transform;

            // Subscription to input events
            manager.pinchEvent += CameraDolly;
            manager.twoDragEvent += CameraOrbit;
            manager.threeDragEvent += CameraPedestalTruck;

            // Subscribe to selection change
            UIManager uiManager = core.getManager<UIManager>();
            uiManager.selectionChanged += SelectionUpdate;

            // Initialize control variables
            m_selectionCenter = Vector3.zero;
            m_hasSelection = false;
        }


        //! 
        //! Dolly function: moves the camera forward.
        //! 
        //! @param sender The input manager.
        //! @param e The distance between the touch gesture triggering the movement.
        //!
        private void CameraDolly(object sender, InputManager.PinchEventArgs e)
        {
            // Dolly cam
            m_camXform.Translate(0f, 0f, e.distance * s_dollySpeed);
        }

        //! 
        //! Orbit function: rotates the camera around a pivot point.
        //! Currently the orbit point is set to a specific distance from the camera.
        //! 
        //! @param sender The input manager.
        //! @param e The delta distance from the touch gesture triggering the movement.
        //!
        private void CameraOrbit(object sender, InputManager.DragEventArgs e)
        {
            Vector3 pivotPoint;
            // Set the pivot point

            // check if selection within camera
            Vector3 camCoord = m_cam.WorldToViewportPoint(m_selectionCenter);
            //Debug.Log(camCoord);

            if (m_hasSelection)
                pivotPoint = m_selectionCenter;
            // In case of no selection, pivot point is a point in front of the camera
            else
                pivotPoint = m_camXform.TransformPoint(Vector3.forward * 6f);
            // TODO: Redesign so it takes into account scene scale

            // Arc
            m_camXform.RotateAround(pivotPoint, Vector3.up, s_orbitSpeed * e.delta.x);
            // Tilt
            m_camXform.RotateAround(pivotPoint, m_camXform.right, -s_orbitSpeed * e.delta.y);
        }

        //! 
        //! Pedestal & Truck function: moves the camera vertically or horizontally.
        //! 
        //! @param sender The input manager.
        //! @param e The delta distance from the touch gesture triggering the movement.
        //!
        private void CameraPedestalTruck(object sender, InputManager.DragEventArgs e)
        {
            // Adjust the input
            Vector2 offset = -s_panSpeed * e.delta;

            // Move around
            m_camXform.Translate(offset.x, offset.y, 0);
        }

        //!
        //! Function called when selection has changed.
        //!
        private void SelectionUpdate(object sender, List<SceneObject> sceneObjects)
        {
            m_hasSelection = false;
            if (sceneObjects.Count < 1)
                return;

            // Calculate the average position
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in sceneObjects)
            {
                averagePos += obj.transform.position;
            }
            averagePos /= sceneObjects.Count;

            m_selectionCenter = averagePos;
            m_hasSelection = true;
        }

    }
}