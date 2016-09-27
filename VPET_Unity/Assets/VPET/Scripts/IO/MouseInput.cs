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
//! script receiving mouse & keyboard inputs and converting it to touch gestures & states
//! this script is only enabled when beeing in unity editor (for debugging)
//!
namespace vpet
{
	public class MouseInput : MonoBehaviour 
	{
	
		//!
		//! cached reference to human input controller
		//!
		private InputAdapter inputAdapter;
		public InputAdapter InputAdapterXXX
		{
			set { inputAdapter = value; }
		}
	
	    //!
	    //! is pointer down & moving
	    //!
	    private bool drag = false;
	
	    //!
	    //! store start mouse position
	    //!
	    private Vector3 startMousePosition = Vector3.zero;
	
	    //!
	    //! cache pointer to MoveCamera class
	    //!
	    private MoveCamera moveCamera = null;
	
	    
	    //!
	    //! Use this for initialization
	    //!
	    void Start()
	    {
	        moveCamera = Camera.main.transform.GetComponent<MoveCamera>();
	
	        inputAdapter = GameObject.Find("InputAdapter").GetComponent<InputAdapter>();
	
	    }
		
	    //!
		//! Update is called once per frame
	    //!
	    void Update()
	    {
	        //single mouse gesture
	        if (Input.GetMouseButtonDown(0))
	        {
	            //left mouse Button down
	            if (Input.GetKey(KeyCode.Alpha3))
	            {
					inputAdapter.threePointerStarted(Input.mousePosition);
	            }
	            else if (Input.GetKey(KeyCode.Alpha2))
	            {
					inputAdapter.twoPointerStarted(Input.mousePosition);
	            }
	            else
	            {
					inputAdapter.singlePointerStarted(Input.mousePosition);
	            }
	            startMousePosition = Input.mousePosition;
	            drag = true;
	        }
	        else if (Input.GetMouseButtonUp(0))
	        {
	            //left mouse Button up
	            if (Input.GetKey(KeyCode.Alpha3))
	            {
					inputAdapter.threePointerEnded(Input.mousePosition);
	            }
	            else if (Input.GetKey(KeyCode.Alpha2))
	            {
					inputAdapter.twoPointerEnded(Input.mousePosition);
	            }
	            else
	            {
					inputAdapter.singlePointerEnded(Input.mousePosition);
	            }
	            drag = false;
	        }
	        else if (drag)
	        {
	            //left mouse Button down and mouse moving
	            if (Input.GetKey(KeyCode.Alpha3))
	            {
					inputAdapter.threePointerDrag(Input.mousePosition);
	            }
	            else if (Input.GetKey(KeyCode.Alpha2))
	            {
					inputAdapter.twoPointerDrag(Input.mousePosition);
	            }
	            else if ( Input.GetKey( KeyCode.Alpha1 ) )
	            {
	                // set rotation
	                Vector3 _mouseDelta = Input.mousePosition-startMousePosition;
	                Quaternion _rotation =   Quaternion.Euler( -_mouseDelta.y*Time.deltaTime, _mouseDelta.x*Time.deltaTime, 0 );
	                moveCamera.transform.localRotation *= _rotation ;
	            }
	            else 
	            {
					inputAdapter.singlePointerDrag(Input.mousePosition);
	            }
	        }
	    }
}
}