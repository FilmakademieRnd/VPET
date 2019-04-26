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

		//! return current camera parameter value for slider value
		public float getCamParamValue(CameraObject.CameraParameter param)
		{
			switch (param) 
			{
			case CameraObject.CameraParameter.LENS:
				return cameraAdapter.Fov.vFovToLens();
			case CameraObject.CameraParameter.FOV:
				return cameraAdapter.Fov;
			//case CameraObject.CameraParameter.APERTURE:
			//	return cameraAdapter.Aperture;
			//	break;
			//case CameraObject.CameraParameter.FOCDIST:
			//	return cameraAdapter.focDist;
			//	break;
			//case CameraObject.CameraParameter.FOCSIZE:
			//	return cameraAdapter.focSize;
			//	break;
			}

			return -1.0f;
		}

        public void setCamParamFocalLength(float focalLength)
        {
            float fov = Extensions.lensToVFov(focalLength);
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

            cameraAdapter.Fov = Mathf.Min(angle, 150f);
        }

        //! change the desired camera parameter (usually a callback triggered by the slider changes)
        public void setCamParamValue(CameraObject.CameraParameter param, float value)
		{
			switch (param) 
			{
			case CameraObject.CameraParameter.LENS:
				cameraAdapter.Fov = value.lensToVFov();
				break;
			case CameraObject.CameraParameter.FOV:
				cameraAdapter.Fov = value;
				break;
			//case CameraObject.CameraParameter.APERTURE:
			//	cameraAdapter.Aperture = value;
			//	break;
			//case CameraObject.CameraParameter.FOCDIST:
			//	cameraAdapter.focDist = value;
			//	break;
			//case CameraObject.CameraParameter.FOCSIZE:
			//	cameraAdapter.focSize = value;
			//	break;
			}
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