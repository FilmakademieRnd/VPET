/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/v-p-e-t

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
using System.Collections.Generic;
using System.Xml.Serialization;

//!
//! XML Data Structure for a AnimationClip
//!
namespace vpet
{
	public class XML_AnimationClip
	{
	    [XmlArray("XML_AnimationCurves"), XmlArrayItem("XML_AnimationCurve")]
	    public List<XML_AnimationCurve> XML_AnimationCurves = new List<XML_AnimationCurve>();
	
	    [XmlAttribute("clipName")]
	    public string name;
	
	    [XmlIgnoreAttribute]
	    public AnimationClip data;
	
	    public XML_AnimationClip()
	    {
	        name = "null";
	    }
	
	    public XML_AnimationClip(string clipName)
	    {
	        name = clipName;
	    }
	
	    public XML_AnimationClip(string clipName, AnimationClip clip)
	    {
	        name = clipName;
	        data = clip;
	    }
}
}