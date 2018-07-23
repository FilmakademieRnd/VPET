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
//! This class provides functionality to the translation, scale and rotation modifieres (objects you can grab to move/rotate/scale selected objects)
//! This class is beeing applied on each modifier gameObject container containing the actual modifier parts as child objects.
//! It automatically adjusts the orientation of the modifiers for best visibility (real coordinate system is beeing ignored).
//!
namespace vpet
{
	public class Modifier : MonoBehaviour {
	
	    //!
	    //! if pause is set to true all repositioning / rotating of the modifers is avoided
	    //! pause is activated when the modifier is not visible
	    //!
	    private bool pause = true;
	
	    //!
	    //! stores the last position of the modifiers to be able to detect weather it moved.
	    //!
	    private Quaternion lastPosition;

        //!
        //! all transform modifier transforms.
        //!
        private Transform c0, c1, c2, c3, c4, c5, c6, c7, c8;

        //!
        //! Use this for initialization
        //!
        void Start () 
	    {
	        this.setVisible(false);
	        lastPosition = this.transform.rotation;

            if (this.name == "TranslateModifier")
            {
                c0 = this.transform.GetChild(0);
                c1 = this.transform.GetChild(1);
                c2 = this.transform.GetChild(2);
                c3 = this.transform.GetChild(3);
                c4 = this.transform.GetChild(4);
                c5 = this.transform.GetChild(5);
                c6 = this.transform.GetChild(6);
                c7 = this.transform.GetChild(7);
                c8 = this.transform.GetChild(8);
            }
        }
	
	    //!
	    //! Update is called once per frame
	    //!
		void Update () 
	    {
	        if (!pause && lastPosition != this.transform.rotation)
	        {
	            this.adjustOrientation();
	            lastPosition = this.transform.rotation;
	        }
		}

	    //!
	    //! hides or showes all parts of this modifier (beeing present as childs of this container gameObject)
	    //! @param    visible     shall the modifier be visible or not    
	    //!
	    public void setVisible(bool visible)
	    {
	        pause = !visible;
	        foreach (Transform child in transform)
	        {
	            child.GetComponent<ModifierComponent>().setVisible(visible);
	        }
	    }
	
	    //!
	    //! resets the color of all parts (beeing present as childs of this container gameObject) of this modifier to their default "unselected" color
	    //!
	    public void resetColors()
	    {
	        pause = false;
	        foreach (Transform child in transform)
	            child.gameObject.GetComponent<ModifierComponent>().resetColor();
	    }
	
	    //!
	    //! sets the alpha of the color of all parts (beeing present as childs of this container gameObject) of this modifier to a value < 1 (beeing defined in the ModifierComponent script).
	    //!
	    public void makeTransparent()
	    {
	        foreach (Transform child in transform)
	            child.gameObject.GetComponent<ModifierComponent>().makeTransparent();
	    }
	
	    //!
	    //! enables an external script to set pause to true
	    //!
	    public void isUsed()
	    {
	        pause = true;
	    }
	
	    //!
	    //! This function adjusts the orientation of the modifiers. This will break the real coordiante system for visibility reasons.
	    //! So if you are e.g. looking along the z-axis the z-axis modifier will not point into z direction (since it would then be behind the x and y modifiers)
	    //! but instead points towards the negative z-axis, or in other words more towards the camera.
	    //!
	    private void adjustOrientation()
	    {
            Vector3 toOther = (this.transform.position - Camera.main.transform.position).normalized;

            float rx = Vector3.SignedAngle(toOther, this.transform.forward, this.transform.up);
            float rz = Vector3.SignedAngle(toOther, this.transform.up, this.transform.right);

            Debug.Log("X: " + Vector3.SignedAngle(toOther, this.transform.forward, this.transform.up));
            Debug.Log("Z: " + Vector3.SignedAngle(toOther, this.transform.up, this.transform.right));

            if (this.name == "TranslateModifier")
	        {
                
                float shift = 0f;

                if (rx > -135 && rx < -45)
	            {
                    c0.localRotation = Quaternion.Euler(0, 180, 0);
                    shift = 0.5f;
	            }
	            else
	            {
                    c0.localRotation = Quaternion.Euler(0, 0, 0);
                    shift = -0.5f;
	            }
                c3.localPosition = new Vector3(shift, c3.localPosition.y, c3.localPosition.z);
                c4.localPosition = new Vector3(shift, c4.localPosition.y, c4.localPosition.z);
                c7.localPosition = new Vector3(shift, c7.localPosition.y, c7.localPosition.z);
                c8.localPosition = new Vector3(shift, c8.localPosition.y, c8.localPosition.z);

                if (rz > -45 && rz < 45)
	            {
                    c1.localRotation = Quaternion.Euler(180, 0, 0);
                    shift = -0.5f;
                }
	            else
	            {
                    c1.localRotation = Quaternion.Euler(0, 0, 0);
                    shift = 0.5f;
	            }
                c3.localPosition = new Vector3(c3.localPosition.x, shift, c3.localPosition.z);
                c4.localPosition = new Vector3(c4.localPosition.x, shift, c4.localPosition.z);
                c5.localPosition = new Vector3(c5.localPosition.x, shift, c5.localPosition.z);
                c6.localPosition = new Vector3(c6.localPosition.x, shift, c6.localPosition.z);

                if (rx > -45 && rx < 45)
	            {
                    c2.localRotation = Quaternion.Euler(0, 180, 0);
                    shift = -0.5f;
                }
                else
	            {
	                c2.localRotation = Quaternion.Euler(0, 0, 0);
                    shift = 0.5f;
                }
                c5.localPosition = new Vector3(c5.localPosition.x, c5.localPosition.y, shift);
                c6.localPosition = new Vector3(c6.localPosition.x, c6.localPosition.y, shift);
                c7.localPosition = new Vector3(c7.localPosition.x, c7.localPosition.y, shift);
                c8.localPosition = new Vector3(c8.localPosition.x, c8.localPosition.y, shift);
            }
	    }
}
}