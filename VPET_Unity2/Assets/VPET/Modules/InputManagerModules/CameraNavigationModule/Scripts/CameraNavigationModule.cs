/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
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
        //! The camera center of interest point
        //!
        private Vector3 centerOfInterest;

        //!
        //! A buffer vector storing the position offset between camera and center of interest
        //!
        private Vector3 coiOffset;

        //!
        //! A control variable for orbiting operation
        //!
        private bool stickToOrbit = false;

        //!
        //! A parameter defining how close to the edge an object can be and still act as center of interest
        //!
        private float screenTolerance = .05f;

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
        //! Destructor, cleaning up event registrations. 
        //!
        public override void Dispose()
        {
            base.Dispose();

            // Unsubscribe
            manager.pinchEvent -= CameraDolly;
            manager.twoDragEvent -= CameraOrbit;
            manager.threeDragEvent -= CameraPedestalTruck;
            UIManager uiManager = core.getManager<UIManager>();
            uiManager.selectionChanged -= SelectionUpdate;
            manager.updateCameraUICommand -= CameraUpdated;
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

            // Subscribe to camera change
            manager.updateCameraUICommand += CameraUpdated;

            // Initialize control variables
            m_selectionCenter = Vector3.zero;
            m_hasSelection = false;
        }

        //! 
        //! Function that updates the camera center of interest for new camera selection.
        //! 
        //! @param sender The input manager.
        //! @param e Not used.
        //!
        private void CameraUpdated(object sender, bool e)
        {
            // Assign arbitrary center of interest
            centerOfInterest = m_camXform.TransformPoint(Vector3.forward * 6f);

            // Store positional offset
            coiOffset = m_camXform.position - centerOfInterest;
        }


        //! 
        //! Dolly function: moves the camera forward.
        //! 
        //! @param sender The input manager.
        //! @param e The distance between the touch gesture triggering the movement.
        //!
        private void CameraDolly(object sender, float distance)
        {
            // Dolly cam
            m_camXform.Translate(0f, 0f, distance * s_dollySpeed);

            // Check if center of interest is in front of camera
            Vector3 camCoord = m_cam.WorldToViewportPoint(centerOfInterest);
            if(camCoord.z < 0)
                // Else snap to camera
                centerOfInterest = m_camXform.position;

            // Store positional offset
            coiOffset = m_camXform.position - centerOfInterest;

        }

        //! 
        //! Orbit function: rotates the camera around a pivot point.
        //! Currently the orbit point is set to a specific distance from the camera.
        //! 
        //! @param sender The input manager.
        //! @param e The delta distance from the touch gesture triggering the movement.
        //!
        private void CameraOrbit(object sender, Vector2 delta)
        {
            // Prepare the pivot point
            Vector3 pivotPoint;

            // If an object is selected
            if (m_hasSelection)
            {
                pivotPoint = m_selectionCenter;
                // Check if selection center is inside camera view
                Vector3 camCoord = m_cam.WorldToViewportPoint(m_selectionCenter);
                // If any element is negative, it out of camera
                if (camCoord.x < screenTolerance || camCoord.y < screenTolerance || camCoord.x > 1 - screenTolerance || camCoord.y > 1 - screenTolerance || camCoord.z < 0 || stickToOrbit)
                {
                    // If center of interest coincides with selection center
                    if (centerOfInterest == m_selectionCenter)
                    {
                        // It means the center of orbit was already set to an object, and needs to be reset to the center
                        centerOfInterest = m_camXform.TransformPoint(Vector3.forward * 6f);
                    }
                    pivotPoint = centerOfInterest;
                    // And it should not change until selection is changed (else orbiting pivot will jump to object as soon as it 
                    stickToOrbit = true;
                }
            }
            else
            {
                pivotPoint = centerOfInterest;
            }

            // Arc
            m_camXform.RotateAround(pivotPoint, Vector3.up, s_orbitSpeed * delta.x);
            // Tilt
            m_camXform.RotateAround(pivotPoint, m_camXform.right, -s_orbitSpeed * delta.y);

            // Update value
            centerOfInterest = pivotPoint;
            // Store positional offset
            coiOffset = m_camXform.position - centerOfInterest;

        }

        //! 
        //! Pedestal & Truck function: moves the camera vertically or horizontally.
        //! 
        //! @param sender The input manager.
        //! @param e The delta distance from the touch gesture triggering the movement.
        //!
        private void CameraPedestalTruck(object sender, Vector2 delta)
        {
            // Adjust the input
            Vector2 offset = -s_panSpeed * delta;

            // Move around
            m_camXform.Translate(offset.x, offset.y, 0);

            // If it was not orbited
            if (centerOfInterest != m_selectionCenter)
            {
                // Drag the center of interest with it
                centerOfInterest = m_camXform.position - coiOffset;
            }
        }

        //!
        //! Function called when selection has changed.
        //!
        private void SelectionUpdate(object sender, List<SceneObject> sceneObjects)
        {
            m_hasSelection = false;
            if (sceneObjects.Count < 1)
            {
                // In case of deselection, set the center of interest back to the center of the view
                // If it has been orbited (meaning center of interest coincides to selection), preserve distance to camera
                if (centerOfInterest == m_selectionCenter)
                {
                    Vector3 bufferpos = m_cam.WorldToViewportPoint(m_selectionCenter);
                    bufferpos.x = .5f;
                    bufferpos.y = .5f;
                    centerOfInterest = m_cam.ViewportToWorldPoint(bufferpos);

                    // Store positional offset
                    coiOffset = m_camXform.position - centerOfInterest;
                }
                return;
            }

            // Calculate the average position
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in sceneObjects)
                averagePos += obj.transform.position;
            averagePos /= sceneObjects.Count;

            m_selectionCenter = averagePos;
            m_hasSelection = true;

            // Reset control variable
            stickToOrbit = false;
        }

    }
}