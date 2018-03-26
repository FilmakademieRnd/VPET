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
//! helper class to recursively darken certain asset parts due to inconsistancy.
//! It will darken the asset and all its child assets.
//! should not be used / be necessary in stadart use cases 
//!
namespace vpet
{
	public class assetDarkener : MonoBehaviour {
	
	    //!
	    //! factor beeing applied on the brightness of the material.
	    //! Smaller than 1 darkens the material, greater than 1 will light the material
	    //!
		public float factor = 0.67f;
	
	    //!
	    //! Use this for initialization
	    //!
		void Start () {
				this.recursiveDarkener(this.gameObject);
		}
	
	    //!
	    //! recursive function to darken material of the given object and all childs
	    //! @param    obj     gameObject on which darkening is applied
	    //!
		private void recursiveDarkener(GameObject obj)
		{
			if (obj.GetComponent<Renderer>() != null)
			{
				foreach(Material mat in obj.GetComponent<Renderer>().materials)
				{
					mat.color = new Color(factor,factor,factor);
				}
			}
			foreach (Transform child in obj.transform)
			{
				recursiveDarkener(child.gameObject);
			}
		}
}
}