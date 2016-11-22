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
using System.Collections.Generic;
using System.Xml.Serialization;

//!
//! XML Data Structure for a AnimationCurve
//!
namespace vpet
{
	public class XML_AnimationCurve
	{
	    [XmlArray("XML_KeyFrames"), XmlArrayItem("XML_KeyFrame")]
	    public List<XML_KeyFrame> XML_KeyFrames = new List<XML_KeyFrame>();
	
	    [XmlAttribute("postWrapMode")]
	    public string postWM;
	
	    [XmlAttribute("preWrapMode")]
	    public string preWM;
	
	    [XmlAttribute("typeName")]
	    public string type;
	
	    [XmlAttribute("propertyName")]
	    public string property;
	
	    [XmlIgnoreAttribute]
	    public AnimationCurve data;
	
	    public XML_AnimationCurve(List<XML_KeyFrame> XML_KeyFrames, string postWrapMode, string preWrapMode, string typeName, string propertyName)
	    {
	        this.XML_KeyFrames = XML_KeyFrames;
	        this.postWM = postWrapMode;
	        this.preWM = preWrapMode;
	        this.type = typeName;
	        this.property = propertyName;
	    }
	
	    public XML_AnimationCurve(string typeName, string propertyName)
	    {
	        this.type = typeName;
	        this.property = propertyName;
	    }
	
	    public XML_AnimationCurve()
	    {}
}
}