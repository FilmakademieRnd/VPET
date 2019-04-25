/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

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

/*
Sample Mapping for XBox and GameVice Controller

Command 		|		Windows Xbox  mapping 		| 	
----------------|-----------------------------------|
Fire0 			|Positive Button: joystick button 0	|
Fire1 			|Positive Button: joystick button 1	|
Fire2 			|Positive Button: joystick button 2	|
Fire3 			|Positive Button: joystick button 3	|
DPAD_H  		|Joystick Axis, 6th axis 			|
DPAD_V  		|Joystick Axis, 7th axis			|
LeftStick_X 	|Joystick Axis, X Axis				|
LeftStick_Y 	|Joystick Axis, Y Axis, invert		|
RightStick_X 	|Joystick Axis, 4th axis            |
RightStick_Y 	|Joystick Axis, 5th axis, invert	|
L1				|Key or Mouse Button,				|
				|Positive Button: joystick button 4	|
L2				|Joystick Axis, 3rd axis			|			
R1				|Key or Mouse Button,				|
				|Positive Button: joystick button 5	|				
R2				|Joystick Axis, 3rd axis			|			
Settings        |Positive Button: joystick button 7 |   
				

Command 		|		iOS GameVice mapping 		 						| 
----------------|-----------------------------------------------------------|
Fire0           |Positive Button: joystick button 14, Axis: 14th (A Button) |
Fire1           |Positive Button: joystick button 13, Axis: 13th (B Button) |
Fire2           |Positive Button: joystick button 15, Axis: 15th (X Button) |
Fire3           |Positive Button: joystick button 12, Axis: 12th (Y Button) |
DPAD_H          |Positive Button: joystick button 5, Axis: 5th              |
DPAD_H_neg      |Positive Button: joystick button 7, Axis: 7th              |
DPAD_V          |Positive Button: joystick button 4, Axis: 4th              |
DPAD_V_neg      |Positive Button: joystick button 6, Axis: 6th              |
LeftStick_X     |Joystick Axis, X Axis                                      |
LeftStick_Y     |Joystick Axis, Y Axis, invert                              |
RightStick_X    |Joystick Axis, 3th axis                                    |
RightStick_Y    |Joystick Axis, 4th axis, invert                            |
L1              |Key or Mouse Button, Positive Button: joystick button 8    |
L2              |Key or Mouse Button, Positive Button: joystick button 10   |
R1              |Key or Mouse Button, Positive Button: joystick button 9    |
R2              |Key or Mouse Button, Positive Button: joystick button 11   |
Settings        |Key or Mouse Button, Positive Button: joystick button 0    |

Note: When setting the type to "Key or Mouse Button" for an input in unitys
project settings (Edit -> Project Settings... -> Input), the Axis dropdown
doesn't do anything.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_IOS && UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

//!
//! script receiving input from one or two (vcs) joystick
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
        private bool hasPressedR2 = false;
        private bool hasPressedL2 = false;
        private int DPADdirection = 0;

        // dictionary to keep track of the status of controller buttons since they
        // do not behave like the unity API says on iOS
        private IDictionary<string, float> buttonPressedState;

        private float left, right, bottom, top;
        private Transform worldTransform = null;
        private SceneObject sceneObject = null;
        private ServerAdapter serverAdapter;
        private float x_axis = 0f;
        private float y_axis = 0f;
        private float z_axis = 0f;
        private GameObject crossHair = null;
        private GameObject previousCrosshairObject = null;
        private GameObject currentCrosshairObject = null;
        public OutlineEffect outlineEffect;
        static int defaultLayermask = (1 << 0) | (1 << 13);
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Time until the deselect button works for reset
        private static float deselectHoldDuration = 1.6f;

        //!
        //! Cached reference to the main controller.
        //!
        private MainController mainController = null;
        private SceneLoader sceneLoader = null;
        private InputAdapter inputAdapter = null;
        public SceneLoader SceneLoader
        {
            set { sceneLoader = value; }
        }

        //!
        //! get current cross hair object 
        //!
        public void getcurrentCrosshairObject()
        {
            previousCrosshairObject = currentCrosshairObject;
            currentCrosshairObject = inputAdapter.cameraRaycast(screenCenter, defaultLayermask);
        }

        //!
        //! Wrapper for unity API function GetButtonDown() that does not behave like described on iOS.
        //! Using a dictionary to keep track of the status of controller buttons.
        //! A bool flag gets kept for every button name this function is called for.
        //! The function can be probably removed when the iOS unity API issue was fixed.
        //! @param      buttonName       name of a button like its defined in the Input Manager.
        //! @return     true only for the first time the function gets called for a certain button after the user stated pressing it.
        //!
        private bool buttonPressed(string buttonName)
        {
            if (Input.GetButtonDown(buttonName))
            {
                float buttonPressedTimestamp = 0f;
                if (buttonPressedState.TryGetValue(buttonName, out buttonPressedTimestamp))
                {
                    if (buttonPressedTimestamp > 0f) 
                        return false;
                    else
                    {
                        buttonPressedState[buttonName] = Time.time;
                        return true;
                    }
                }
                else
                {
                    buttonPressedState.Add(buttonName, Time.time);
                    return true;
                }
            }
            else if (Input.GetButtonUp(buttonName) && buttonPressedState.ContainsKey(buttonName))
                    buttonPressedState[buttonName] = 0f;
            return false;
        }

        //!
        //! Function to mesure for how long a button has already been pressed.
        //! In case the given button is not pressed at all ...
        //! @param      buttonName       name of a button like its defined in the Input Manager.
        //! @return     the time in seconds for how long the button has been already pressed or 0 if the button isn't pressed
        //!
        private float buttonPressedTime(string buttonName) {
            float buttonPressedTimestamp = 0f;
            if (!buttonPressedState.TryGetValue(buttonName, out buttonPressedTimestamp)) { 
                buttonPressed(buttonName);
                return buttonPressedTimestamp; // this should be an exception
            }
            else if (buttonPressedTimestamp > 0f)
                return Time.time - buttonPressedTimestamp;
            else
                return buttonPressedTimestamp;
        }

        //!
        //! all possible inputs
        //! called every frame by MoveCamera (in its update() function)
        //!
        public void getButtonUpdates()
        {
            // outline effect
            outlineEffect = Camera.main.transform.GetChild(0).GetComponent<Camera>().GetComponent<OutlineEffect>();

            if (Input.GetButton("L1"))
            {
                getcurrentCrosshairObject();
                // hide center menu
                mainController.deselect();
                // switch to camera mode
                moveCameraActive = true;
                moveObjectActive = false;
                rotateObjectActive = false;
                scaleObjectActive = false;
                // show crosshair
                crossHair.SetActive(true);

                // initial state, previousCrosshairObject == some object, currentCrosshairObject == null
                if (previousCrosshairObject != null && currentCrosshairObject == null)
                    previousCrosshairObject.GetComponent<SceneObject>().callShowNormal(previousCrosshairObject);

                // initial state, previousCrosshairObject == some object (previous), currentCrosshairObject == some object new
                else if (currentCrosshairObject != null && previousCrosshairObject != null && currentCrosshairObject != previousCrosshairObject)
                    previousCrosshairObject.GetComponent<SceneObject>().callShowNormal(previousCrosshairObject);

                // highlight current selection
                else if (currentCrosshairObject)
                {
                    outlineEffect.lineColor0 = new Color(1f, 0.9f, 0.7f);
                    currentCrosshairObject.GetComponent<SceneObject>().callShowHighlighted(currentCrosshairObject);
                }

            }
            else if (Input.GetButtonUp("L1"))
            {
                if (currentCrosshairObject != null)
                {
                    outlineEffect.lineColor0 = new Color(1.0f, 0.8f, 0.3f);
                    mainController.handleSelection(currentCrosshairObject.GetComponent<Transform>());
                }
                crossHair.SetActive(false);
            }
            // enter translation mode
            else if (buttonPressed("Fire3"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // directly enter rotation mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // enter camera mode
                else if (moveObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
            // enter rotation mode
            else if (buttonPressed("Fire1"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));

                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // enter camera mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
            // enter scale mode
            else if (buttonPressed("Fire0"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    if (mainController.getCurrentSelection().GetComponent<SceneObjectLight>())
                        mainController.UIAdapter.CenterMenu.transform.GetChild(7).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    else
                        mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(0).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    if (mainController.getCurrentSelection().GetComponent<SceneObjectLight>())
                    {
                        if (mainController.arMode)
                            mainController.UIAdapter.drawCenterMenu(layouts.LIGHT_AR);
                        else
                            mainController.UIAdapter.drawCenterMenu(layouts.LIGHT);
                        mainController.UIAdapter.CenterMenu.transform.GetChild(7).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                    else
                    {
                        mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                        mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
                // directly enter scale mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.UIAdapter.CenterMenu.transform.GetChild(1).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    if (mainController.getCurrentSelection().GetComponent<SceneObjectLight>())
                    {
                        if (mainController.arMode)
                            mainController.UIAdapter.drawCenterMenu(layouts.LIGHT_AR);
                        else
                            mainController.UIAdapter.drawCenterMenu(layouts.LIGHT);
                        mainController.UIAdapter.CenterMenu.transform.GetChild(7).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                    else
                    {
                        mainController.UIAdapter.drawCenterMenu(layouts.OBJECT);
                        mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
                // enter camera mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    if (mainController.getCurrentSelection().GetComponent<SceneObject>() is SceneObjectLight)
                    {
                        mainController.UIAdapter.CenterMenu.transform.GetChild(7).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                    else
                    {
                        mainController.UIAdapter.CenterMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
            }
            // toggle configuration window	
            else if (Input.GetButtonDown("Settings"))
            {
                mainController.UIAdapter.MainMenu.transform.GetChild(3).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // toggle predefined bookmarks
            else if (buttonPressed("R1") && !mainController.arMode)
                mainController.repositionCamera();

            // disable tracking
#if UNITY_EDITOR || UNITY_STANDALONE
            else if (Input.GetAxis("R2") < 0 && !hasPressedR2 && !mainController.arMode)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            else if (buttonPressed("R2") && !mainController.arMode)
#endif
            {
                hasPressedR2 = true;
                mainController.UIAdapter.MainMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // deselect or reset current selection
            if (!Input.GetButton("Fire2") && buttonPressedTime("Fire2") > 0f
                                   && buttonPressedTime("Fire2") < deselectHoldDuration)
            {
                mainController.deselect();
            }
            else if (Input.GetButton("Fire2") && buttonPressedTime("Fire2") > deselectHoldDuration){
                if (mainController.getCurrentSelection())
                    mainController.getCurrentSelection().GetComponent<SceneObject>().resetAll();
            }
            if (buttonPressed("Fire2")) { }

            // cycle through edit modes
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
            else if (Input.GetAxis("L2") > 0 && !hasPressedL2)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            else if (buttonPressed("L2"))
#endif
            {
                hasPressedL2 = true;
                mainController.UIAdapter.MainMenu.transform.GetChild(1).GetComponent<MenuButtonList>().OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // cycle through object list                                    
#if UNITY_EDITOR || UNITY_STANDALONE
            else if (Input.GetAxis("DPAD_H") != 0 && !hasPressedDirectionalPad ||
                     Input.GetAxis("DPAD_V") != 0 && !hasPressedDirectionalPad)
            {
                if (Input.GetAxis("DPAD_H") == 1 || Input.GetAxis("DPAD_V") == 1)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
			else if ( (buttonPressed("DPAD_H")) ||
					  (buttonPressed("DPAD_H_neg")) ||
					  (buttonPressed("DPAD_V")) ||
					  (buttonPressed("DPAD_V_neg")) )
            {
                if (Input.GetButtonDown("DPAD_H") || Input.GetButtonDown("DPAD_V"))
#endif
                    DPADdirection = 1;
                else
                    DPADdirection = -1;

                hasPressedDirectionalPad = true;
                int match = -1;
                bool cycleLights = false;
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetAxis("DPAD_V") != 0)
                    cycleLights = true;
#elif UNITY_IOS || UNITY_STANDALONE_OSX
				if (Input.GetButtonDown("DPAD_V") || Input.GetButtonDown("DPAD_V_neg") )
                    cycleLights = true;
#endif
                Transform currentSelection = mainController.getCurrentSelection();
                mainController.handleSelection();
                if (cycleLights)
                {
                    if (currentSelection)
                        match = SceneLoader.SelectableLights.IndexOf(currentSelection.gameObject);
                    if (match < 0)
                        selectLight(0);
                    else
                        selectLight(match + DPADdirection);
                }
                else
                {
                    if (currentSelection)
                        match = SceneLoader.SceneEditableObjects.IndexOf(currentSelection.gameObject);
                    if (match < 0)
                        selectObject(0);
                    else
                        selectObject(match + DPADdirection);
                }

                // reactivate last selected edit mode
                if (moveObjectActive)
                    mainController.buttonTranslationClicked(true);
                else if (rotateObjectActive)
                    mainController.buttonRotationClicked(true);
                else if (scaleObjectActive & !mainController.getCurrentSelection().GetComponent<SceneObjectLight>())
                    mainController.buttonScaleClicked(true);
            }
            // Dpad and R2 reset 
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetAxis("DPAD_H") == 0 && (Input.GetAxis("DPAD_V") == 0))
                hasPressedDirectionalPad = false;
            if (Input.GetAxis("R2") == 0)
                hasPressedR2 = false;
            if (Input.GetAxis("L2") == 0)
                hasPressedL2 = false;

#elif UNITY_IOS || UNITY_STANDALONE_OSX
            if (!buttonPressed("DPAD_H") && !buttonPressed("DPAD_V"))
                hasPressedDirectionalPad = false;
#endif
        }

        private void selectObject(int potentialIdx)
        {
            for (int offset = 0; offset < SceneLoader.SceneEditableObjects.Count; offset++)
            {
                int realIdx = mod((potentialIdx + offset * DPADdirection), SceneLoader.SceneEditableObjects.Count);
                if (!SceneLoader.SceneEditableObjects[realIdx].GetComponent<SceneObject>().locked && 
                    !SceneLoader.SceneEditableObjects[realIdx].GetComponent<SceneObjectLight>() &&
                    SceneLoader.SceneEditableObjects[realIdx].GetComponent<CameraObject>() == null)
                {
                    mainController.callSelect(SceneLoader.SceneEditableObjects[realIdx].transform);
                    break;
                }
            }
        }

        private void selectLight(int potentialIdx)
        {
            for (int offset = 0; offset < SceneLoader.SelectableLights.Count; offset++)
            {
                int realIdx = mod((potentialIdx + offset * DPADdirection),SceneLoader.SelectableLights.Count);
                if (!SceneLoader.SelectableLights[realIdx].GetComponent<SceneObject>().locked)
                {
                    mainController.callSelect(SceneLoader.SelectableLights[realIdx].transform);
                    break;
                }
            }
        }

        private int mod(int a, int b)
        {
            return (a % b + b) % b;
        }

        //!
        //! mapping of analog joysticks
        //!
        public Vector3 getTranslation()
        {
            x_axis = Input.GetAxis("LeftStick_X") * speed;  // mapped to Joystick 1 X Axis
            z_axis = Input.GetAxis("LeftStick_Y") * speed;  // mapped to Joystick 1 Y Axis (inverted)
            y_axis = Input.GetAxis("RightStick_Y") * speed; // mapped to Joystick 1 5th Axis (inverted)

            if (scaleObjectActive && Input.GetAxis("RightStick_X") != 0.0f)
            {
                x_axis = Input.GetAxis("RightStick_X") * speed;
                y_axis = x_axis;
                z_axis = x_axis;
            }

            Vector3 pos = Vector3.zero;
            pos = new Vector3(x_axis, y_axis, z_axis);
            return (VPETSettings.Instance.controllerSpeed) * pos * Time.deltaTime;
        }

        public Transform WorldTransform
        {
            set
            {
                worldTransform = value;
                sceneObject = worldTransform.GetComponent<SceneObject>();

                // HACK
                if (sceneObject == null)
                {
                    sceneObject = worldTransform.gameObject.AddComponent<SceneObject>();
                }
            }
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

            // initialize the dictionary that keeps track of the status of controller buttons
            buttonPressedState = new Dictionary<string, float>();

            //cache reference to main Controller
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
            //sceneLoader = GameObject.Find("SceneAdapter").GetComponent<SceneLoader>();
            inputAdapter = GameObject.Find("InputAdapter").GetComponent<InputAdapter>();
            crossHair = GameObject.Find("GUI/Canvas/Crosshair");
        }
    }
}




#if UNITY_IOS && UNITY_EDITOR
public class BuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            //Read XCode project file
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            string targetName = PBXProject.GetUnityTargetName();
            string targetGUID = project.TargetGuidByName(targetName);

            project.AddFrameworkToProject(targetGUID, "GameController.framework", false);

            // Write project file
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
}
#endif
