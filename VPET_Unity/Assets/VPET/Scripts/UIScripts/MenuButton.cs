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
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace vpet
{
	//!
	//! Simple click button inherit from Button and implement IMenuButton interface
	//!
	public class MenuButton : Button, IMenuButton
	{

		//!
		//! Reference to the menu this button is included 
		//!
		private Menu menu;
		public Menu Menu
		{
			set{ menu = value; }
		}

		//!
		//! Toggle state. ( this type of button is not toggleable )
		//!
		public bool Toggled
		{
			get { return false; }
            set { }
            }
			
		//!
		//! Adds an action to this button. This button type can hold exactly one action.
		//! @param      active      sprite displayed when clicked/toggled
		//! @param      idle      	sprite displayed in normal state
		//! @param      call      	method to be called onclick
		//!
		public void AddAction(Sprite active, Sprite idle, UnityAction call=null)
        {
			SpriteState newSprites = new SpriteState();
			newSprites.disabledSprite = idle;
			newSprites.highlightedSprite = idle;
			newSprites.pressedSprite = active;
			spriteState = newSprites;
			this.GetComponent<Image>().sprite = idle;
			if ( call != null )
			{
				interactable = true;
				onClick.AddListener( call );
			}
        }

		//!
		//! Override method. Called when button is clicked.
		//!
		public override void OnPointerClick (PointerEventData eventData)
		{
            // set active button at menu
            if ( menu) menu.ActiveButton = this.gameObject;

			// call base
			base.OnPointerClick (eventData);
		}

        public void AddHoldAction(UnityAction call)
        {
            // empty
        }

        //!
        //! Reset button state
        //!
        public void reset() {}
	}
}