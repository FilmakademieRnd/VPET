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
//! This class provides functionality to one part (e.g. one axis arrow) of the translation, scale and rotation modifieres (objects you can grab to move/rotate/scale selected objects)
//! This class is beeing applied on each modifier part gameObject.
//! It receives modifications directly from the parent modifier container.
//! Public functions should not be called elsewhere.
//!
namespace vpet
{
	public class ModifierComponent : MonoBehaviour {
	
	    //!
	    //! Color beeing applied to the modifier part at startup time.
	    //! Used to be able to reset the color easily.
	    //!
	    private Color initialColor;
	
	    //!
	    //! Use this for initialization
	    //!
	    void Start()
	    {
	        if (this.GetComponent<Renderer>())
	        {
	            initialColor = this.GetComponent<Renderer>().material.color;
	        }
	        if (this.name != "MoveToFloorQuad")
	        {
	            this.gameObject.layer = 2; //layer "Ignore Raycast"
	        }
	    }
	
	    //!
	    //! set modifier part color to a given value
	    //! @param    rgba      color that should be applied to the modifier
	    //!
	    public void setColor(Color rgba)
	    {
	        if (this.GetComponent<Renderer>())
	        {
	            this.GetComponent<Renderer>().material.color = rgba;
	        }
	        foreach (Transform child in transform)
	            child.gameObject.GetComponent<ModifierComponent>().setColor(rgba);
	    }
	
	    //!
	    //! set modifier part color to initial value
	    //!
	    public void resetColor()
	    {
	        if (this.GetComponent<Renderer>())
	        {
	            this.GetComponent<Renderer>().material.color = initialColor;
	        }
	        foreach (Transform child in transform)
	            child.gameObject.GetComponent<ModifierComponent>().resetColor();
	    }
	
	    //!
	    //! sets the alpha of the color of this modifier part to 0.2 to make it semi-transparent.
	    //!
	    public void makeTransparent()
	    {
	        if (this.GetComponent<Renderer>())
	        {
	            this.GetComponent<Renderer>().material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0.2f);
	        }
	        foreach (Transform child in transform)
	            child.gameObject.GetComponent<ModifierComponent>().makeTransparent();
	    }
	
	    //!
	    //! hides or showes this part of the modifier
	    //! @param    visible     shall the modifier part be visible or not    
	    //!
	    public void setVisible(bool set)
	    {
	        if (this.GetComponent<Renderer>())
	        {
	            this.gameObject.GetComponent<Renderer>().enabled = set;
	        }
	        if (this.name != "MoveToFloorQuad")
	        {
	            if (set) this.gameObject.layer = 8; //layer "RenderInFront"
	            else this.gameObject.layer = 2; //layer "Ignore Raycast"
	        }
	        foreach (Transform child in transform)
	        {
	            child.gameObject.GetComponent<ModifierComponent>().setVisible(set);
	        }
	    }
}
}