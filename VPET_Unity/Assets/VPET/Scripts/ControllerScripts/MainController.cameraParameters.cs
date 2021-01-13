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
using UnityEngine;
using System.Collections;

//!
//! MainController part handling the additional camera parameters
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {

        public void setCamParamFocalLength(float focalLength)
        {
            float fov = Extensions.lensToVFov(focalLength);
            serverAdapter.SendObjectUpdate(currentSceneObject, ParameterType.FOV);
            SceneObjectCamera currentCamera = (SceneObjectCamera) currentSceneObject;
            if (currentCamera)
            {
                currentCamera.fov = fov;
            }
            if (activeMode == Mode.lookThroughCamMode)
                cameraAdapter.Fov = fov;
        }

        public void setLightParamAngle(float angle)
        {
            SceneObjectLight currentLight = (SceneObjectLight) currentSceneObject;

            if (currentLight)
            {
                currentLight.setLightAngle(angle);
            }
            if (activeMode == Mode.lookThroughLightMode)
                cameraAdapter.Fov = Mathf.Min(angle, 150f);
        }


        ////! Callback: enable / disable focus visualizer of DOF component
        //public void toggleVisualizer()
        //{
        //	if (cameraAdapter.visualizeFocus) {
        //		cameraAdapter.visualizeFocus = false;
        //	} else 
        //	{
        //		cameraAdapter.visualizeFocus = true;
        //	}
        //}

        ////! Callback: enable / disable DOF component for main camera
        //public void toggleDOF()
        //{
        //	if (cameraAdapter.depthOfField) 
        //	{
        //		cameraAdapter.depthOfField = false;
        //		// disable DOF related menu buttons and hide sliders
        //		//ui.dofButtonsInteractable(false);
        //	} 
        //	else 
        //	{
        //		cameraAdapter.depthOfField = true;
        //		// enable DOF related menu buttons
        //		//ui.dofButtonsInteractable(true);
        //	}
        //}

    }
}