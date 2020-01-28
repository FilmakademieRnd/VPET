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
//! MainController part handling pointToMove Interaction
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour
    {
	    //!
	    //! should point to move be applied to camera
	    //!
	    bool cameraPointMove = false;
	
	    //!
	    //! show the pointToMove widget at the specific place (but place it on top of the groundPlane)
	    //! @param      pos     new position of the pointToMove widget, y should always be 0
	    //!
	    public void showPointToMoveIdentifier(Vector3 pos){
	        if (pos != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) && (activeMode == Mode.pointToMoveMode )){
	            pointToMoveModifier.transform.position = pos + new Vector3(0, 0.01f, 0);
                pointToMoveModifier.layer = LayerMask.NameToLayer("RenderInFront");
                pointToMoveModifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, pos) / 1000) * (Camera.main.fieldOfView / 30);
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
	    //! function executing the pointToMove translation and hiding the pointToMoveIdentifier
        //!
	    public void hidePointToMoveIdentifier(Vector3 pos)
        {
	        if (pos != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) && (activeMode == Mode.pointToMoveMode))
            {
	            if (cameraPointMove)
                {
	                //use camera as target for the translation
	                Camera.main.GetComponent<MoveCamera>().smoothTranslate(new Vector3(pos.x, Camera.main.transform.position.y, pos.z));
	            }
	            else
	            {
                    //use currently selected Object as target for the translation
                    if (currentSceneObject)
                    {
                        // HACK to animate in pointTOMove mode
                        if (ui.LayoutUI == layouts.ANIMATION)
                        {
                            currentSceneObject.translate(new Vector3(pos.x, currentSelection.position.y, pos.z));
                            animationController.setKeyFrame();
                        }
                        else
                        {
                            if (currentSceneObject.isAnimatedCharacter)
                            {
                                currentSceneObject.targetTranslation = pos;
                                serverAdapter.SendObjectUpdate(this.currentSceneObject, ParameterType.CHARACTERTARGET);
                            }
                            else
                            {
                                currentSceneObject.smoothTranslate(new Vector3(pos.x, currentSelection.position.y, pos.z));
                            }
                        }
                    }
                    else // TODO: what is this for?
                    {
                        // HACK to animate in pointTOMove mode
                        if (ui.LayoutUI == layouts.ANIMATION)
                        {
                            currentSelection.GetChild(0).GetComponent<SceneObject>().translate(new Vector3(pos.x, currentSelection.position.y, pos.z));
                            animationController.setKeyFrame();
                        }
                        else
                        {
                            currentSelection.GetChild(0).GetComponent<SceneObject>().smoothTranslate(new Vector3(pos.x, currentSelection.position.y, pos.z));
                        }
                    }
	            }
	            pointToMoveModifier.GetComponent<Renderer>().enabled = false;
	        }
	    }
    }
}