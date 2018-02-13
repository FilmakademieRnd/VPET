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
using System.Xml.Serialization;

//!
//! XML Data Structure for a Animation KeyFrame
//!
namespace vpet
{
	public class XML_KeyFrame
	{
	    [XmlAttribute("inTangent")]
	    public float inT;
	
	    [XmlAttribute("outTangent")]
	    public float outT;
	
	    [XmlAttribute("time")]
	    public float t;
	
	    [XmlAttribute("value")]
	    public float v;
	
	    [XmlAttribute("tangentMode")]
	    public int m;
	
	    [XmlIgnoreAttribute]
	    public Keyframe data;
	
	    public XML_KeyFrame(float inTangent, float outTangent, float time, float value, int tangentMode)
	    {
	        this.inT = inTangent;
	        this.outT = outTangent;
	        this.t = time;
	        this.v = value;
	        this.m = tangentMode;
	    }
	   
	    public XML_KeyFrame()
	    {}
}
}