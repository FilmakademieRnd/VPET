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
	    public void buttonTranslationClicked(bool active){
	        if (active)
            {
	            activeMode = Mode.translationMode;
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
	    public void buttonRotationClicked(bool active){
	        if (active)
            {
	            activeMode = Mode.rotationMode;
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
	    public void buttonScaleClicked(bool active){
	        if (active)
            {
	            activeMode = Mode.scaleMode;
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
        public void togglePointToMoveCamera()
        {
            cameraPointMove = !cameraPointMove;
            if (activeMode == Mode.pointToMoveMode)
            {
                activeMode = Mode.idle;
            }
            else
            {
                activeMode = Mode.pointToMoveMode;
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


        public void buttonAnimatorCmdClicked(int cmd)
        {
            serverAdapter.sendAnimatorCommand(currentSelection.transform, cmd);
        }

    }
}