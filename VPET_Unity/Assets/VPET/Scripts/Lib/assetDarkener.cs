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