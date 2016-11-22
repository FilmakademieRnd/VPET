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
using System.IO;

//!
//! script holding loading and holding references to the message templates used to form messages that are send to katana
//!
namespace vpet
{
	public class KatanaTemplates : MonoBehaviour {
	
	    //!
	    //! template for light intensity and color messages
	    //!
	    public string lightIntensityColorTemplate;
	    //!
	    //! template for light position and rotation messages
	    //!
	    public string lightTransRotTemplate;
	    //!
	    //! template for light cone angle messages
	    //!
		public string lightConeAngleTemplate;
	    //!
	    //! template for light range messages
	    //!
		public string lightRangeTemplate;
	    //!
	    //! template for object position, rotation, scale messages
	    //!
		public string objTemplate;
	
		public string objTemplateQuat;
	    //!
	    //! template for camera fov
	    //!
	    public string camTemplate;
	
	    //!
		//! Use this for initialization
		//!
	    void Start ()
	    {
	        TextAsset binaryData = Resources.Load("VPET/TextTemplates/lightIntensityColorTemplate") as TextAsset;
	        lightIntensityColorTemplate = binaryData.text;
	        binaryData = Resources.Load("VPET/TextTemplates/lightTransRotTemplate") as TextAsset;
	        lightTransRotTemplate = binaryData.text;
			binaryData = Resources.Load("VPET/TextTemplates/lightConeAngleTemplate") as TextAsset;
			lightConeAngleTemplate = binaryData.text;
			binaryData = Resources.Load("VPET/TextTemplates/lightRangeTemplate") as TextAsset;
			lightRangeTemplate = binaryData.text;
			binaryData = Resources.Load("VPET/TextTemplates/objTemplate") as TextAsset;
			objTemplate = binaryData.text;
	        binaryData = Resources.Load("VPET/TextTemplates/camTemplate") as TextAsset;
	        camTemplate = binaryData.text;
			binaryData = Resources.Load("VPET/TextTemplates/objTemplateQuat") as TextAsset;
			objTemplateQuat = binaryData.text;
	    }
}
}