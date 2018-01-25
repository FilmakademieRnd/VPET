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
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//!
//! script receiving input from one or two (vcs) joysticks or gamevice
//!
namespace vpet
{
    public class JoystickInput : MonoBehaviour
    {
        public float speed = 1000f;
        public float speedFov = 1f;
        public float fov = 30f;
        public float aspect = 1.77777f;
                
        public bool moveCameraActive = true;
        public bool moveObjectActive = false;
        public bool rotateObjectActive = false;
        public bool scaleObjectActive = false;
        private bool hasPressedDirectionalPad = false;
        private int DPADdirection = 0;

        List<SceneObject> EditableObjectsList = new List<SceneObject>();
        List<SceneObject> EditableLightList = new List<SceneObject>();
        List<SceneObject> EditableObjects = new List<SceneObject>();
        SceneObject currselTransform;       

        private float left, right, bottom, top;

        private Transform worldTransform = null;
        private SceneObject sceneObject = null;
        private ServerAdapter serverAdapter;

        private float x_axis = 0f;
        private float y_axis = 0f;
        private float z_axis = 0f;      

        //!
        //! Cached reference to the main controller.
        //!
        private MainController mainController;
        private SceneLoader sceneLoader;

        //!
        //! called by main controller once scene has been loaded to fill lists with appropriate items
        //!
        public void initSelectionLists() {
            // object and lights lists                
            List<GameObject> sceneEditableObjectsList = sceneLoader.sceneEditableObjectsList;
            // create light list
            EditableLightList.Clear();
            foreach (GameObject g in sceneLoader.selectableLights)
            {
                if (g.GetComponent<SceneObject>() != null && g.GetComponent<SceneObject>().IsLight == true)
                    EditableLightList.Add(g.GetComponent<SceneObject>());
            }
            // sceneEditableObjects contains cameras and lights, we need to sort those out and build a new list
            EditableObjectsList.Clear();
            foreach (GameObject g in sceneEditableObjectsList)
            {
                if (g.GetComponent<CameraObject>() == null && g.GetComponent<SceneObject>().IsLight == false)
                    EditableObjectsList.Add(g.GetComponent<SceneObject>());
            }            
        }

        //!
        //! all possible inputs
        //!
        public void getButtonUpdates()
        {
            if (Input.GetButtonDown("Fire2"))
            {
                // toggle gravity
                if (moveObjectActive && mainController.getCurrentSelection())
                {
                    if (mainController.HasGravityOn())
                        mainController.getCurrentSelection().GetComponent<Rigidbody>().useGravity = false;
                    else
                        mainController.getCurrentSelection().GetComponent<Rigidbody>().useGravity = true;
                }
            }
            // enter translation mode
            else if (Input.GetButtonDown("Fire3"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                }
                // enter camera mode
                else if (moveObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.callDeselect();
                    mainController.buttonTranslationClicked(false);
                }

            }
            // enter rotation mode
            else if (Input.GetButtonDown("Fire1"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                }
                // enter camera mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.callDeselect();
                    mainController.buttonRotationClicked(false);
                }
            }
            // enter scale mode
            else if (Input.GetButtonDown("Fire0"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                }
                // directly enter scale mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                }
                // enter camera mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.callDeselect();
                    mainController.buttonScaleClicked(false);
                }

            }
            // toggle predefined bookmarks
            else if (Input.GetButtonDown("R1"))
            {                
                mainController.repositionCamera();
                mainController.resetCameraOffset();
            }

            // cycle through manipulation list                                                
            else if (Input.GetButtonDown("L1"))
            {
                if (mainController.getCurrentSelection())
                {
                    mainController.resetSelectionPosition();
                    mainController.resetSelectionRotation();
                    mainController.resetSelectionScale();
                }
            }           
            // cycle through object list                                    
            else if (Input.GetAxis("Horizontal") != 0 && hasPressedDirectionalPad == false ||
                     Input.GetAxis("Vertical") != 0 && hasPressedDirectionalPad == false)
            {
                EditableObjects = EditableObjectsList;

                if (Input.GetAxis("Horizontal") == 1 || Input.GetAxis("Vertical") == 1)
                    DPADdirection = 1;
                else DPADdirection = -1;

                hasPressedDirectionalPad = true;
                int match = 0;
                
                if (Input.GetAxis("Vertical") != 0)
                    EditableObjects = EditableLightList;                              

                // test current selection
                if (mainController.getCurrentSelection())
                {                    
                    currselTransform = mainController.getCurrentSelection().gameObject.GetComponent<SceneObject>();
                    // is current sel in objects? if not set manualy to object[0], happens when switching from assets to lights
                    match = 0;
                    match = EditableObjects.FindIndex(x => x == currselTransform);
                    if (match == -1)
                    {
                        mainController.callDeselect();
                        mainController.callSelect(GameObject.Find(EditableObjects[0].name).GetComponent<Transform>());
                    }
                    }
                else                                  
                    mainController.callSelect(EditableObjects[0].GetComponent<Transform>());                

                match = 0;
                match = EditableObjects.FindIndex(x => x == currselTransform);

                if (match != -1) {
                    // Dpad right/up end reched start over
                    if (match == EditableObjects.Count-1 && DPADdirection == 1)
                        match = -1;
                    // Dpad left/down first list entry reached
                    if (match == 0 && DPADdirection == -1)
                        match = EditableObjects.Count;
                    // all other cases
                    mainController.callDeselect();
                    mainController.callSelect(GameObject.Find(EditableObjects[match + DPADdirection].name).GetComponent<Transform>());
                }                
            }
            // Dpad reset 
            else if (Input.GetAxis("Horizontal") == 0 && (Input.GetAxis("Vertical") == 0))
            {
                hasPressedDirectionalPad = false;
            }            
        }

        //!
        //! mapping of analog joysticks
        //!
        public Vector3 getTranslation()
        {
            if (moveObjectActive || rotateObjectActive || scaleObjectActive)
                mainController.setAxisLockerXYZ();

            x_axis = Input.GetAxis("LeftStick_X") * speed;  // mapped to Joystick 1 X Axis
            z_axis = Input.GetAxis("LeftStick_Y") * speed;  // mapped to Joystick 1 Y Axis (inverted)
            y_axis = Input.GetAxis("RightStick_Y") * speed; // mapped to Joystick 1 5th Axis (inverted)

            Vector3 pos = Vector3.zero;
            pos = new Vector3(x_axis, y_axis, z_axis);           
            return pos * Time.deltaTime;
        }

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

            left = -1.0f * aspect;
            right = 1.0f * aspect;
            bottom = -1.0f;
            top = 1.0f;

            //cache reference to main Controller
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
            sceneLoader = GameObject.Find("SceneAdapter").GetComponent<SceneLoader>();            
        }
    }
}
 