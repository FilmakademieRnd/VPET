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
//! This script should be applied to any gameObject that is not moving in the scene and should not be affected by any physics.
//! It recursively adds MeshColliders to all child objects and thereby enables all SceneObjects to collide with it.
//!
namespace vpet
{
	public class StaticHitObject : MonoBehaviour {
	
	    //!
	    //! Should mesh colliders be created for the gameObject this script is attached to.
	    //!
	    public bool createMeshColliders = true;
	
	    //!
	    //! Use this for initialization#
	    //!
		void Start () 
	    {
	        if (createMeshColliders)
	        {
	            this.recursiveAddMeshCollider(this.gameObject);
	        }
		}
	
	    //!
	    //! This function adds meshColliders to this gameObject and all child objects
	    //! Additionally it fixes the "smoothness" shader parameter by setting it to 0 which is set to 0.5 per default
	    //! @param  obj     gameObject for which to add meshColliders recursively
	    //!
	    private void recursiveAddMeshCollider(GameObject obj)
	    {
	        if (obj.GetComponent<Renderer>() != null)
	        {
	            obj.AddComponent<MeshCollider>();
	            obj.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.0f);
	            //obj.GetComponent<MeshCollider>().convex = true;
	        }
	        foreach (Transform child in obj.transform)
	        {
	            recursiveAddMeshCollider(child.gameObject);
	        }
	    }
}
}