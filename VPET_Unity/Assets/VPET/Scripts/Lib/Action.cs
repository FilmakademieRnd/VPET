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
//! This object can store any modification applied to a SceneObject. It features different constructors for all types.
//! Main purpose is the use in the undo/redo Stack.
//!

namespace vpet
{
	public class Action {
	
	    //!
	    //! enum holding all possible modification types
	    //!
	    public enum Type{ translation, rotation, scale, kinematic, lightColor, lightIntensity, lightAngle, lightRange, invalid };
	
	    //!
	    //! transform of the GameObject on which the modification is applied
	    //!
	    Transform obj;
	    //!
	    //! type of modification beeing applied, selected from the Type enum
	    //!
	    Type type;
	
	
	    //!
	    //! new value of the modification beeing applied (single float)
	    //!
	    float value;
	    //!
	    //! new value of the modification beeing applied (vector3 of floats)
	    //!
	    Vector3 valueVector;
	    //!
	    //! new value of the modification beeing applied (color)
	    //!
	    Color lightColor;
	    //!
	    //! new value of the modification beeing applied (rotation quaternion)
	    //!
	    Quaternion valueQuaternion;
	
	    //!
	    //! cached link to the server adapter to send out change on execution
	    //!
		ServerAdapter serverAdapter;
	
	    //!
	    //! constructor for light angle, light intensity and light range
	    //! @param    newObj              transform of the gameObject on which modification is applied
	    //! @param    newType             type of the new modification (only lightAngle, lightIntensity and lightRange valid)
	    //! @param    newValue            new value of the modification
	    //! @param    server              reference to server adapter to communicate object changes ofer network
	    //!
	    public Action(Transform newObj, Type newType, float newValue, ServerAdapter server) 
	    {
			serverAdapter = server;
	        if (newType == Type.lightAngle ||
	           newType == Type.lightIntensity ||
	           newType == Type.lightRange)
	        {
	            obj = newObj;
	            type = newType;
	            value = newValue;
	        }
	        else 
	        {
	            type = Type.invalid;
	        }
		}
	
	
	    //!
	    //! constructor for scale and translation
	    //! @param    newObj              transform of the gameObject on which modification is applied
	    //! @param    newType             type of the new modification (only scale and translation valid)
	    //! @param    newValueVector      new Vector3 of the modification
	    //! @param    server              reference to server adapter to communicate object changes ofer network
	    //!
	    public Action(Transform newObj, Type newType, Vector3 newValueVector, ServerAdapter server) 
		{
			serverAdapter = server;
	        if (newType == Type.scale ||
	            newType == Type.translation)
	        {
	            obj = newObj;
	            type = newType;
	            valueVector = newValueVector;
	        }
	        else
	        {
	            type = Type.invalid;
	        }
	    }
	
	    //! 
	    //! constructor for rotation
	    //! @param    newObj              transform of the gameObject on which modification is applied
	    //! @param    newQuaternion       new value of rotation
	    //! @param    server              reference to server adapter to communicate object changes ofer network
	    //!
	    public Action(Transform newObj, Quaternion newQuaternion, ServerAdapter server) 
		{
			serverAdapter = server;
	        obj = newObj;
	        type = Type.rotation;
	        valueQuaternion = newQuaternion;
	    }
	
	    //! 
	    //! constructor for light color
	    //! @param    newObj              transform of the gameObject on which modification is applied
	    //! @param    newLightColor       new value of light color
	    //! @param    server              reference to server adapter to communicate object changes ofer network
	    //!
	    public Action(Transform newObj, Color newLightColor, ServerAdapter server) 
		{
			serverAdapter = server;
	        obj = newObj;
	        type = Type.lightColor;
	        lightColor = newLightColor;
	    }
	
	    //!
	    //! constructor for kinematic on/off
	    //! @param    newObj              transform of the gameObject on which modification is applied
	    //! @param    changeKinematic     new value of kinematic
	    //! @param    server              reference to server adapter to communicate object changes ofer network
	    //!
	    public Action(Transform newObj, bool changeKinematic, ServerAdapter server) 
		{
			serverAdapter = server;
	        obj = newObj;
	        type = Type.kinematic;
	    }
	
	    //!
	    //! executes the stored modification while choosing proper target values automatically based on the stored type and transform
	    //!
	    public void execute()
	    {
	        switch (type)
	        {
	            case (Type.invalid):
	                break;
	            case (Type.translation):
	                obj.position = valueVector;
					// serverAdapter.sendTranslation(obj,obj.position);
					serverAdapter.sendTranslation(obj);
	                break;
	            case (Type.rotation):
	                obj.rotation = valueQuaternion;
					//serverAdapter.sendRotation(obj,obj.rotation);
					serverAdapter.sendRotation(obj);
	                break;
	            case (Type.scale):
	                obj.localScale = valueVector;
					// serverAdapter.sendScale(obj,obj.localScale);
					serverAdapter.sendScale(obj);
	                break;
	            case (Type.kinematic):
	                if (!obj.GetComponent<Light>())
	                {
	                    obj.gameObject.GetComponent<Rigidbody>().isKinematic = !obj.GetComponent<SceneObject>().lockKinematic;
						serverAdapter.sendKinematic(obj,!obj.GetComponent<SceneObject>().lockKinematic);
	                    obj.GetComponent<SceneObject>().lockKinematic = !obj.GetComponent<SceneObject>().lockKinematic;
	                    if (!obj.GetComponent<SceneObject>().lockKinematic)
	                    {
	                        obj.gameObject.GetComponent<Rigidbody>().WakeUp();
	                    }
	                }
	                break;
	            case (Type.lightColor):
	                    obj.GetComponent<Light>().color = lightColor;
						obj.GetChild(0).GetComponent<Renderer>().material.color = lightColor;
						serverAdapter.sendLightColor(obj,obj.GetComponent<Light>());
	                break;
	            case (Type.lightIntensity):
	                    obj.GetComponent<Light>().intensity = value;
						serverAdapter.sendLightIntensity(obj,obj.GetComponent<Light>());
	                break;
	            case (Type.lightAngle):
	                    obj.GetComponent<Light>().spotAngle = value;
						serverAdapter.sendLightConeAngle(obj,obj.GetComponent<Light>());
	                break;
	            case (Type.lightRange):
	                    obj.GetComponent<Light>().range = value;
						// serverAdapter.sendLightRange(obj,value);
	                break;
	            default:
	                break;
	        }
	    }
}
}