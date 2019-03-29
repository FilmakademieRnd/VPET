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
	            Vector3 finalTranslation = Vector3.Scale(translation, axisLocker);

                if (currentSceneObject)
                    currentSceneObject.translate(finalTranslation);
                else
                {
                    KeyframeScript keyframeScript = currentSelection.GetComponent<KeyframeScript>();
                    if (keyframeScript)
                    {
                        currentSelection.transform.position = finalTranslation;
                        currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
                    }
                }
            }
	    }
        //!
        //! translate currently selected object with joystick
        //! @param      translation     absolute translation beeing applied on current selection
        //!
        public void translateSelectionJoystick(Vector3 translation)
        {
            if (currentSelection)
            {                
                /*if (translation.x == 0 && translation.y == 0)
                    axisLocker = new Vector3(0, 0, 1);
                else if (translation.x == 0 && translation.z == 0)
                    axisLocker = new Vector3(0, 1, 0);
                else if (translation.y == 0 && translation.z == 0)
                    axisLocker = new Vector3(1, 0, 0);
                else 
                    axisLocker = new Vector3(1, 0, 1);

                Vector3 finalTranslation = currentSelection.rotation * Vector3.Scale(Quaternion.Inverse(currentSelection.rotation) * (translation*VPETSettings.Instance.controllerSpeed), axisLocker) + currentSelection.position; 
                */
                Vector3 xTrans = translation.z * Vector3.Scale(Camera.main.transform.forward ,(Vector3.forward + Vector3.right)).normalized;
                Vector3 yTrans = new Vector3(0 , translation.y , 0);
                Vector3 zTrans = translation.x * Vector3.Scale(Camera.main.transform.right, (Vector3.forward + Vector3.right)).normalized;

                float scaleFactor = (8f * Vector3.Distance(Camera.main.transform.position, currentSelection.position)/ VPETSettings.Instance.maxExtend);

                Vector3 finalTranslation = scaleFactor * (xTrans + yTrans + zTrans) + currentSelection.position;
                if (currentSceneObject)
                    currentSceneObject.translate(finalTranslation);
                else
                {
                    KeyframeScript keyframeScript = currentSelection.GetComponent<KeyframeScript>();
                    if (keyframeScript)
                    {
                        currentSelection.transform.position = finalTranslation;
                        currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
                    }
                }
            }
        }

        public void translateSelection(Vector3 begin, Vector3 end)
        {
            if (currentSelection)
            {
                Vector3 finalTranslation =  initRotation * Vector3.Scale(inverseInitRotation * (end-begin), axisLocker) + initPosition;

                if (axisLocker != new Vector3(0, 1, 1) &&
                    axisLocker != new Vector3(1, 0, 1) &&
                    axisLocker != new Vector3(1, 1, 0))
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, currentSelection.position);
                    lineRenderer.SetPosition(1, currentSelection.position + initRotation * axisLocker * 10000);
                }
                else
                    lineRenderer.positionCount = 0;

                if (currentSceneObject)
                    currentSceneObject.translate(finalTranslation);
                else
                {
                    KeyframeScript keyframeScript = currentSelection.GetComponent<KeyframeScript>();
                    if (keyframeScript)
                    {
                        currentSelection.transform.position = finalTranslation;
                        currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
                    }
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
                Vector3 v1 = (currentSelection.position - begin).normalized;
                Vector3 v2 = (currentSelection.position - end).normalized;
                float angle = Vector3.SignedAngle(v1, v2, helperPlane.normal);
                Quaternion rotation = Quaternion.AngleAxis(angle, axisLocker);                

                lineRenderer.positionCount = 4;
                lineRenderer.SetPosition(0, currentSelection.position);
                lineRenderer.SetPosition(1, begin);
                lineRenderer.SetPosition(2, currentSelection.position);
                lineRenderer.SetPosition(3, end);                

                currentSceneObject.transform.rotation = initRotation * rotation;
            }
	    }

        //!
        //! rotate currently selected object
        //! @param      begin       last position on rotation sphere
        //! @param      end         current position on rotation sphere
        //!
        public void rotateSelectionJoystick(Vector3 end)
        {
            if (currentSelection)
            {
                end /= VPETSettings.Instance.controllerSpeed;
                end *= 5f;
                end.x *= -1;
                currentSceneObject.transform.rotation *= Quaternion.Euler(end.z, end.x, end.y);
            }                       
        }
        //!
        //! scale currently selected object via joystick
        //! @param      scale     new scale of object
        //!
        public void scaleSelection(float scale){
	        if (currentSelection){
	            if (!currentSelection.parent.GetComponent<Light>()){
                    currentSelection.localScale += (axisLocker * scale);                    
                    if (liveMode){
                        serverAdapter.SendObjectUpdate(currentSelection.GetComponent<SceneObject>(), ParameterType.SCALE);
	                }
	            }
	        }
	    }

        //!
        //! scale currently selected object
        //! @param      scale     new scale of object
        //!
        public void scaleSelection(Vector3 scale)
        {
            if (currentSelection) {
                if (axisLocker.x == 1 && axisLocker.y == 1 && axisLocker.z == 1) {
                    scale = Vector3.one * scale.x;
                }
                else {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, currentSelection.position);
                    lineRenderer.SetPosition(1, currentSelection.position + axisLocker * 10000);
                }
                if (!currentSelection.transform.parent.transform.GetComponent<Light>()) {
                    currentSelection.transform.localScale = Vector3.Scale(scale, inverseInitRotation * axisLocker) / 1000f + initScale;
                    if (liveMode)
                        serverAdapter.SendObjectUpdate(currentSelection.GetComponent<SceneObject>(), ParameterType.SCALE);
                }
            }
        }
        //!
        //! scale currently selected object with joystick input
        //! @param      scale     new scale of object
        //!
        public void scaleSelectionJoystick(Vector3 scale)
        {
            if (currentSelection)
            {
                // lights (scalemode is used for light parameters intensity and range)
                SceneObjectLight scl = currentSelection.GetComponent<SceneObjectLight>();
                if (scl)
                {
                    // set light intensity
                    scl.setLightIntensity(scl.getLightIntensity() + (scale.z * 0.5f));
                    // set gui element
                    scl.setLightRange(scl.getLightRange() + (scale.y * 0.5f));
                    if (scale.z == 0.0f)
                        UIAdapter.updateRangeSlider(scl.getLightRange());
                    else if (scale.y == 0.0f)
                        UIAdapter.updateRangeSlider(scl.getLightIntensity());
                }
                // objects
                else
                {
                    if (scale.x == 0 && scale.y == 0)
                        axisLocker = new Vector3(0, 0, 1);
                    else if (scale.x == 0 && scale.z == 0)
                        axisLocker = new Vector3(0, 1, 0);
                    else if (scale.y == 0 && scale.z == 0)
                        axisLocker = new Vector3(1, 0, 0);
                    else if (scale.x != 0 && scale.y != 0 && scale.z != 0)
                    {
                        axisLocker = new Vector3(1, 1, 1);
                        scale = Vector3.one * scale.x;
                    }
                    if (!currentSelection.transform.parent.transform.GetComponent<Light>())
                    {
                        float scaleFactor = (8f * Vector3.Distance(Camera.main.transform.position, currentSelection.position) / VPETSettings.Instance.maxExtend);
                        currentSelection.transform.localScale += Vector3.Scale(scale / currentSelection.transform.parent.lossyScale.x * scaleFactor / VPETSettings.Instance.controllerSpeed, axisLocker) / 100f;
                        if (liveMode)
                            serverAdapter.SendObjectUpdate(currentSelection.GetComponent<SceneObject>(), ParameterType.SCALE);
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
                        serverAdapter.SendObjectUpdate(currentSelection.GetComponent<SceneObject>(), ParameterType.SCALE);
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
	                this.translateSelection(begin, end);
	                return;
	            }
	            
	            if (activeMode == Mode.rotationMode){
	                this.rotateSelection(begin, end);
	                return;
	            }
	            
	            if (activeMode == Mode.scaleMode){
                    this.scaleSelection(end - begin);
                    return;
	            }
	        }
	    }


        //!
        //! make current selection receive forces (gravitation etc.) or not
        //!
        public void toggleLockSelectionKinematic(){
            if (!(currentSceneObject is SceneObjectLight || currentSceneObject is SceneObjectCamera))
            {
                Rigidbody rigidbody = currentSelection.gameObject.GetComponent<Rigidbody>();
                rigidbody.isKinematic = !currentSceneObject.globalKinematic;
                currentSceneObject.globalKinematic = !currentSceneObject.globalKinematic;
                serverAdapter.SendObjectUpdate(currentSceneObject, ParameterType.KINEMATIC);

                if (!currentSceneObject.globalKinematic)
                    rigidbody.WakeUp();
            }
	    }
	
	    //!
	    //! is the current selection receiving forces
	    //!
	    public bool currentSelectionisKinematic(){
	        if (currentSelection){
	            return currentSceneObject.GetComponent<Rigidbody>().isKinematic;
	        }
	        else{
	            return false;
	        }
	    }
}
}