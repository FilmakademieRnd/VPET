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
using UnityEngine.EventSystems;
using System.Collections;

//!
//! script attached to any keyframe representing sphere
//!
namespace vpet
{
	public class KeyframeScript : MonoBehaviour {
	
	    //!
	    //! index of this keyframe in the animation curve
	    //!
	    public int keyFrameNumber = -1;
	    
	    //!
	    //! reference to asociated animation clip
	    //!
	    public AnimationClip clip;
	    
	    //!
	    //! reference to the runtime representation of the animation data
	    //!
	    public AnimationData animData;
	    
	    //!
	    //! sceneObject connected this animation is connected to
	    //!
	    public GameObject connectedObject;
		
	    //!
	    //! updates the saved animation curve's keyframe with the current values of the keyframe in the GUI
	    //! this is called when repositioning the keyframe
	    //!
	    public void updateKeyInCurve()
	    {
	        AnimationCurve transXcurve = animData.getAnimationCurve(clip, "m_LocalPosition.x");
	        AnimationCurve transYcurve = animData.getAnimationCurve(clip, "m_LocalPosition.y");
	        AnimationCurve transZcurve = animData.getAnimationCurve(clip, "m_LocalPosition.z");
	
	        transXcurve.MoveKey(keyFrameNumber, new Keyframe(transXcurve.keys[keyFrameNumber].time, this.transform.position.x, transXcurve.keys[keyFrameNumber].inTangent, transXcurve.keys[keyFrameNumber].outTangent));
	        transYcurve.MoveKey(keyFrameNumber, new Keyframe(transYcurve.keys[keyFrameNumber].time, this.transform.position.y, transYcurve.keys[keyFrameNumber].inTangent, transYcurve.keys[keyFrameNumber].outTangent));
	        transZcurve.MoveKey(keyFrameNumber, new Keyframe(transZcurve.keys[keyFrameNumber].time, this.transform.position.z, transZcurve.keys[keyFrameNumber].inTangent, transZcurve.keys[keyFrameNumber].outTangent));
	
	        animData.changeAnimationCurve(clip, typeof(Transform), "m_LocalPosition.x", transXcurve);
	        animData.changeAnimationCurve(clip, typeof(Transform), "m_LocalPosition.y", transYcurve);
	        animData.changeAnimationCurve(clip, typeof(Transform), "m_LocalPosition.z", transZcurve);
	    }
}
}