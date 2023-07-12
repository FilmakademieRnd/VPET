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

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-------------------------------------------------------------------------------
*/

//! @file "UICreator3DPointOnFloor.cs"
//! @brief early implementation of VPET 3D UI point on floor module
//! @author Paulo Scatena
//! @version 0
//! @date 15.02.2022

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! early implementation of VPET 3D UI point on floor module
    //!
    public class UICreator3DPointOnFloor : UIManagerModule
    {
        // Selected object to manipulate
        private SceneObject selObj;

        //Vector3 planeVec = Vector3.zero;
        Plane helperPlane;
        //GameObject manipulator;

        GameObject pointToMoveModifier;
        GameObject noClickCanvas;

        Vector3 lastHitPoint;
        Vector3 targetTranslation;
        readonly float translationDamping = 1.0f;

        //!
        //! A reference to the VPET input manager.
        //!
        private InputManager m_inputManager;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator3DPointOnFloor(string name, Manager manager) : base(name, manager)
        {
            // Disable module
            load = false;
        }

        //!
        //! Init callback for the UICreator3DPointOnFloor module.
        //!
        protected override void Init(object sender, EventArgs e)
        {
            Debug.Log("Init point on floor module");

            // Subscribe to selection change
            manager.selectionChanged += SelectionUpdate;

            // Grabbing from the input manager directly
            m_inputManager = core.getManager<InputManager>();

            // Hookup to input events
            m_inputManager.inputPressPerformed += PressStart;
            m_inputManager.inputPressEnd += PressEnd;

            // Instantiate widget
            InstantiateModifier();

            // make the plane on ground
            helperPlane = new Plane(Vector3.up, new Vector3(0,-2,0));

        }

        //!
        //! Function to prepare for transformations.
        //! Called with the start of click from InputManager
        //! @param sender callback sender
        //! @param e event reference
        //!
        private void PressStart(object sender, Vector2 point)
        {
            //Debug.Log("Press start: " + e.point.ToString());
            if (selObj == null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(point);
            if (helperPlane.Raycast(ray, out float enter))
            {
                //Get the point that is clicked
                Vector3 hitPoint = ray.GetPoint(enter);

                Debug.DrawLine(Vector3.zero, hitPoint, Color.red, 2f);

                // show gizmo
                pointToMoveModifier.transform.position = hitPoint;
                pointToMoveModifier.SetActive(true);

                //monitor move
                m_inputManager.inputMove += Move;
            }
        }

        // This for mouse drag
        // Should only operate in case of existing selection
        // But what happens if touch input is moving the object and other function change the selection
        private void Move(object sender, Vector2 point)
        {

            //Debug.Log("Moving: " + e.point.ToString());
            if (selObj == null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(point);
            if (helperPlane.Raycast(ray, out float enter))
            {
                //Get the point that is clicked
                Vector3 hitPoint = ray.GetPoint(enter);

                Debug.DrawLine(Vector3.zero, hitPoint, Color.green, 2f);

                // move manip
                pointToMoveModifier.transform.position = hitPoint;
                lastHitPoint = hitPoint;
                lastHitPoint.y = selObj.transform.position.y;
            }
        }

        // Soft translate
        IEnumerator SmoothMove()
        {
            float time = 0;
            // We create a loop to control for how many time it will run
            //while (time <= 2)
            //{
            //    time += Time.deltaTime;
            //    selObj.transform.Translate(new Vector3(Time.deltaTime, 0, 0));
            //    yield return null;
            //}
            while (time <= 3)
            {
                time += Time.deltaTime;
                Debug.Log(time);
                selObj.transform.position = Vector3.Lerp(selObj.transform.position, targetTranslation, Time.deltaTime * translationDamping);
                yield return null;
            }
        }

        //!
        //! Function to finalize manipulator operation
        //! Called with the end (cancellation) of click from InputManager
        //! @param sender callback sender
        //! @param e event reference
        //!
        private void PressEnd(object sender, Vector2 point)
        {
            //Debug.Log("Press end: " + e.point.ToString());

            // stop monitoring move
            m_inputManager.inputMove -= Move;

            pointToMoveModifier.SetActive(false);
            //noClickCanvas.SetActive(false);

            // and actually move object
            if (selObj == null)
                return;

            targetTranslation = lastHitPoint;

            // using its monobehaviour quality
            selObj.StopAllCoroutines();
            selObj.StartCoroutine(SmoothMove());
        }

        //!
        //! Updates the selection with the first selected object available
        //! Being called when selection has changed.
        //!
        private void SelectionUpdate(object sender, List<SceneObject> sceneObjects)
        {

            // Log
            //Debug.Log("Selection changed");

            if (sceneObjects.Count > 0)
            {
                // Grab object
                selObj = sceneObjects[0];
                //Debug.Log(selObj);

                // Bring up the non click screen
                noClickCanvas.SetActive(true);
            }
            //else // empty selection
            //{
            //    HideAxes();
            //    modeTRS = -1;
            //    SetManipulatorMode(null, -1);
            //}

        }

        private void InstantiateModifier()
        {
            // Click widget
            GameObject resourcePrefab = Resources.Load<GameObject>("Prefabs/PointToMoveModifier");
            pointToMoveModifier = GameObject.Instantiate(resourcePrefab);
            pointToMoveModifier.SetActive(false);

            // Anti click canvas - temporary hack
            resourcePrefab = Resources.Load<GameObject>("Prefabs/TransparentCanvas");
            noClickCanvas = GameObject.Instantiate(resourcePrefab);
            noClickCanvas.SetActive(false);
        }

    }

}