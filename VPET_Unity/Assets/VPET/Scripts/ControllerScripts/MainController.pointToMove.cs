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
﻿using UnityEngine;
using System.Collections;

//!
//! MainController part handling pointToMove Interaction
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! should point to move be applied to camera
	    //!
	    bool cameraPointMove = false;
	
	    //!
	    //! receiving function for GUI
	    //!
	    public void togglePointToMove()
	    {
            if (activeMode != Mode.pointToMoveMode)
            {
                serverAdapter.sendLock(currentSelection, true);
                activeMode = Mode.pointToMoveMode;
            }
            else
            {
                buttonTranslationClicked(true);
            }
            /*
	        if (activeMode == Mode.pointToMoveMode)
	        {
	            serverAdapter.sendLock(currentSelection, false);
	            activeMode = Mode.translationMode;
	            openMenu();
	        }
	        else 
	        {
	            serverAdapter.sendLock(currentSelection, true);
	            activeMode = Mode.pointToMoveMode;
	        }
            */
        }

        //!
        //! receiving function for GUI, special case for moving the camera
        //!
        public void togglePointToMoveCamera()
	    {
	        cameraPointMove = !cameraPointMove;
	        if (activeMode == Mode.pointToMoveMode)
	        {
	            if (currentSelection)
	            {
	                serverAdapter.sendLock(currentSelection, false);
	            }
	            activeMode = Mode.idle;
	        }
	        else
	        {
	            activeMode = Mode.pointToMoveMode;
	        }
	    }
	
	    //!
	    //! show the pointToMove widget at the specific place (but place it on top of the groundPlane)
	    //! @param      pos     new position of the pointToMove widget, y should always be 0
	    //!
	    public void showPointToMoveIdentifier(Vector3 pos){
	        if (pos != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) && (activeMode == Mode.pointToMoveMode )){
	            pointToMoveModifier.transform.position = pos + new Vector3(0, 0.01f, 0);
	            pointToMoveModifier.GetComponent<Renderer>().enabled = true;
	        }
	    }
	
	    //!
	    //! move the pointToMove widget to the specific place (but place it on top of the groundPlane)
	    //! @param      pos     new position of the pointToMove widget, y should always be 0
	    //!
	    public void movePointToMoveIdentifier(Vector3 pos){
	        if (pos != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) && (activeMode == Mode.pointToMoveMode)){
	            pointToMoveModifier.transform.position = pos + new Vector3(0, 0.01f, 0);
	        }
	    }
	
	    //!
	    //! show the pointToMove widget at the specific place (but place it on top of the groundPlane)
	    //! @param      pos     new position of the pointToMove widget, y should always be 0
	    //!
	    public void hidePointToMoveIdentifier(Vector3 pos){
	        if (pos != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) && (activeMode == Mode.pointToMoveMode)){
	            if (cameraPointMove){
	                //use camera as target for the translation
	                Camera.main.GetComponent<MoveCamera>().smoothTranslate(pos + new Vector3(0, Camera.main.transform.position.y, 0));
					undoRedoController.addAction();
	            }
	            else
	            {
	                //use currently selected Object as target for the translation
	                if (currentSelection.GetComponent<SceneObject>())
	                {
	                    currentSelection.GetComponent<SceneObject>().smoothTranslate(pos + new Vector3(0, currentSelection.position.y, 0));
	                }
	                else
	                {
	                    currentSelection.GetChild(0).GetComponent<SceneObject>().smoothTranslate(pos + new Vector3(0, currentSelection.position.y, 0));
	                }
	            }
	            pointToMoveModifier.GetComponent<Renderer>().enabled = false;
	        }
	    }
}
}