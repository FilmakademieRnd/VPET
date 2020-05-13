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
﻿using UnityEngine;
using System.Collections;

//!
//! MainController part handling all inputs by GUI buttons
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {

        //!
        //! click on the translation button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonTranslationClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.translationMode;
#if !SCENE_HOST
                if (joystickAdapter != null)
                {
                    joystickAdapter.moveCameraActive = false;
                    joystickAdapter.moveObjectActive = true;
                    joystickAdapter.rotateObjectActive = false;
                    joystickAdapter.scaleObjectActive = false;
                }
#endif
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! click on the rotation button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonRotationClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.rotationMode;
#if !SCENE_HOST
                if (joystickAdapter != null)
                {
                    joystickAdapter.moveCameraActive = false;
                    joystickAdapter.moveObjectActive = false;
                    joystickAdapter.rotateObjectActive = true;
                    joystickAdapter.scaleObjectActive = false;
                }
#endif
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! click on the scale button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonScaleClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.scaleMode;
#if !SCENE_HOST
                if (joystickAdapter != null)
                {
                    joystickAdapter.moveCameraActive = false;
                    joystickAdapter.moveObjectActive = false;
                    joystickAdapter.rotateObjectActive = false;
                    joystickAdapter.scaleObjectActive = true;
                }
#endif
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! click on the light intensity button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonLightSettingsClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.lightSettingsMode;
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! click on the light intensity button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonLightIntensityClicked(bool active)
	    {
	        if (active)
	        {
	            activeMode = Mode.lightSettingsMode;
	        }
	        else
	        {
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the light angle button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightAngleClicked(bool active)
	    {
	        if (active)
	        {
	            activeMode = Mode.lightSettingsMode;
	        }
	        else
	        {
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the light range button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightRangeClicked(bool active)
	    {
	        if (active)
	        {
	            activeMode = Mode.lightSettingsMode;
	        }
	        else
	        {
	            openMenu();
	        }
	    }

        //!
        //! click on the camera FOV button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonCamFOVClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.cameraSettingsMode;
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! click on the canera look through button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonLookThroughCamClicked(bool active) 
        {
            currentSelection.GetComponent<SceneObject>().hideVisualization(active);
            if (active)
            {
                activeMode = Mode.lookThroughCamMode;
                camVisualizer.SetActive(true);
            }
            else
            {
                camVisualizer.SetActive(false);
                openMenu();
            }
        }

        //!
        //! click on the canera look through button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonLookThroughLightClicked(bool active)
        {
            currentSelection.GetComponent<SceneObject>().hideVisualization(active);
            if (active)
            {
                activeMode = Mode.lookThroughLightMode;
                camVisualizer.SetActive(true);
            }
            else
            {
                camVisualizer.SetActive(false);
                openMenu();
            }
        }

        //!
        //! click on the light intensity button
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonLightColorClicked(bool active)
	    {
	        if (active)
	        {
	            activeMode = Mode.lightSettingsMode;
	        }
	        else
	        {
	            openMenu();
	        }
	
	    }

        //!
        //! click on animation edit
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonAnimationEditClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.animationEditing;
            }
            else
            {
                openMenu();
            }
        }

        //!
        //! receiving function for GUI
        //!
        public void togglePointToMove(bool active)
        {
            if (active)
            {
                activeMode = Mode.pointToMoveMode;
            }
            else
            {
                buttonTranslationClicked(true);
            }
        }


        //!
        //! receiving function for GUI, special case for moving the camera
        //!
        public void togglePointToMoveCamera(bool active)
        {
            cameraPointMove = active;
            if (active)
            {
                activeMode = Mode.pointToMoveMode;
            }
            else
            {
                activeMode = Mode.idle;
            }
        }

        //!
        //! enable/disable object link to camera
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void toggleObjectLinkCamera(bool active)
        {
            if (active)
            {
                activeMode = Mode.objectLinkCamera;
            }
            else
            {
                buttonTranslationClicked(true);
            }
        }
    }
}