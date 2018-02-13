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
//! MainController part handling modification of current selection
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! lock axis Vector (1 if on, 0 if off)
	    //!
	    Vector3 axisLocker = new Vector3(0, 0, 0);
	
	    //!
	    //! rotation axis of current selection
	    //!
	    Vector3 rotationAxis;
	
	    //!
	    //! scale modifier initial distance between pointer origin
	    //!
	    float initialScaleDistance = float.NaN;

	    //!
	    //! axis locker
	    //!
        public void setAxisLockerXYZ() {
            axisLocker = new Vector3(1, 1, 1);
        }
	    //!
	    //! translate currently selected object
	    //! @param      translation     relative translation beeing applied on current selection
	    //!
	    public void translateSelection(Vector3 translation){
	        if (currentSelection)
	        {
	            Vector3 finalTranslation;
	            finalTranslation = currentSelection.position + new Vector3(translation.x * axisLocker.x, translation.y * axisLocker.y, translation.z * axisLocker.z);

				// print("translation.x: " + translation.x + " finalTranslation.x: " + finalTranslation.x);

	            if (currentSelection.GetComponent<SceneObject>())
	            {
	                currentSelection.GetComponent<SceneObject>().translate(finalTranslation);
	            }
	            else if (currentSelection.GetComponent<KeyframeScript>())
	            {
	                currentSelection.transform.position = finalTranslation;
	                currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
	            }
	        }
	    }
	
	    //!
	    //! rotate currently selected object
	    //! @param      begin       last position on rotation sphere
	    //! @param      end         current position on rotation sphere
	    //!
	    public void rotateSelection(Vector3 begin, Vector3 end){
	        if (currentSelection){
	            //begin and end are on a sphere around the position of the current selection
	            Vector3 v1 = begin - currentSelection.position;
	            Vector3 v2 = end - currentSelection.position;
	            rotationAxis = Vector3.Scale(Vector3.Cross(v1, v2), axisLocker);
	            currentSelection.GetComponent<SceneObject>().setArcBallRotation(Vector3.Angle(v1, v2) * 10, rotationAxis);
	        }
	    }
	
	    //!
	    //! scale currently selected object
	    //! @param      scale     new scale of object
	    //!
	    public void scaleSelection(float scale){
	        if (currentSelection){
	            if (!currentSelection.transform.parent.transform.GetComponent<Light>()){
	                currentSelection.transform.localScale = Vector3.Scale(currentSelection.transform.localScale, Vector3.one + (axisLocker * scale));
	                if (liveMode){
						serverAdapter.sendScale(currentSelection );
	                }
	            }
	        }
	    }
	
	    //!
	    //! scale currently selected object uniform on all 3 axis
	    //! @param      scale     new scale of object
	    //!
	    public void scaleSelectionUniform(float scale)
	    {
	        if (currentSelection)
	        {
	            if (!currentSelection.transform.parent.transform.GetComponent<Light>())
	            {
	                currentSelection.transform.localScale = Vector3.Scale(currentSelection.transform.localScale, Vector3.one * scale );
	                if (liveMode)
	                {
						serverAdapter.sendScale(currentSelection );
	                }
	            }
	        }
	    }
	
	    //!
	    //! move selection into space pointing away from camera (move on camera local z axis)
	    //! @param      distanceDelta     new position relative to old one
	    //!
	    public void moveSelectionAwayFromCamera(float distanceDelta){
	        if (currentSelection){
	            this.translateSelection((currentSelection.position - Camera.main.transform.position).normalized * distanceDelta * 5);
	        }
	    }
	
	    //!
	    //! this is the place where the actual modification is executed
	    //! @param      begin       last point on modifier helper
	    //! @param      end         current point on modifier helper
	    //!
	    public void pointerDrag(Vector3 begin, Vector3 end)
	    {        
	        if (!ignoreDrag){
	            if (activeMode == Mode.translationMode || activeMode == Mode.animationEditing)
	            {
	                //begin & end are on a plane already properly adjusted to fit the currently selected axis
	                this.translateSelection(end - begin);
	                return;
	            }
	            
	            if (activeMode == Mode.rotationMode){
	                this.rotateSelection(begin, end);
	                return;
	            }
	            
	            if (activeMode == Mode.scaleMode){
	                if (float.IsNaN(initialScaleDistance)){
	                    initialScaleDistance = Vector3.Distance(currentSelection.transform.position, begin);
	                }
	                else{
	                    this.scaleSelection((Vector3.Distance(currentSelection.transform.position, end) / Vector3.Distance(currentSelection.transform.position, begin)) - 1);
	                }
	                return;
	            }
	        }
	    }
	
	    //!
	    //! make current selection receive forces (gravitation etc.) or not
	    //!
	    public void toggleLockSelectionKinematic(){
	        if (!currentSelection.GetComponent<SceneObject>().isDirectionalLight && !currentSelection.GetComponent<SceneObject>().isSpotLight && !currentSelection.GetComponent<SceneObject>().isPointLight)
	        {
	            currentSelection.gameObject.GetComponent<Rigidbody>().isKinematic = !currentSelection.GetComponent<SceneObject>().lockKinematic;
	            serverAdapter.sendKinematic(currentSelection, !currentSelection.GetComponent<SceneObject>().lockKinematic);
	            currentSelection.GetComponent<SceneObject>().lockKinematic = !currentSelection.GetComponent<SceneObject>().lockKinematic;
	            if (!currentSelection.GetComponent<SceneObject>().lockKinematic){
	                currentSelection.gameObject.GetComponent<Rigidbody>().WakeUp();
	            }
	        }
	    }
	
	    //!
	    //! is the current selection receiving forces
	    //!
	    public bool currentSelectionisKinematic(){
	        if (currentSelection){
	            return currentSelection.GetComponent<SceneObject>().GetComponent<Rigidbody>().isKinematic;
	        }
	        else{
	            return false;
	        }
	    }
}
}