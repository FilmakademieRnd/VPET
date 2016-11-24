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
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
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
				break;
			case CameraObject.CameraParameter.FOV:
				return cameraAdapter.Fov;
				break;
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

        public void setCamParamFov(float focalLength)
        {
            cameraAdapter.Fov = focalLength.lensToVFov();
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