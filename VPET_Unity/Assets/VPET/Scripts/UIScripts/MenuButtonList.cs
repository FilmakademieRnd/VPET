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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace vpet
{
	//!
	//! List button inherit from Button and implement IMenuButton interface. On click the action is called and the button switch to the next state,
	//! i.e. change button icon and set to next action.
	//!
	public class MenuButtonList : Button, IMenuButton
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

		private List<Sprite> spritesActive = new List<Sprite>();
		private List<Sprite> spritesIdle = new List<Sprite>();
		private List<UnityAction> actions = new List<UnityAction>();

		private int spriteIdx = 0;

		//!
		//! Adds an action to this button. This button type can hold several actions which will be cycled through.
		//! @param      active      sprite displayed when clicked/toggled
		//! @param      idle      	sprite displayed in normal state
		//! @param      call      	method to be called onclick
		//!
		public void AddAction(Sprite active, Sprite idle, UnityAction call)
		{
			if ( spritesActive.Count == 0 ) // first
			{
				SpriteState newSprites = new SpriteState();
				newSprites.disabledSprite = idle;
				newSprites.highlightedSprite = idle;
				newSprites.pressedSprite = active;
				spriteState = newSprites;
				this.GetComponent<Image>().sprite = idle;
			}

			spritesActive.Add(active);
			spritesIdle.Add(idle);
			actions.Add(call);
		}


		//!
		//! Override method. Called when button is clicked.
		//!
		public override void OnPointerClick (PointerEventData eventData)
		{
			// set active button at menu
			menu.ActiveButton = this.gameObject;

			// count index
			spriteIdx = (spriteIdx+1) % spritesActive.Count;

			// change  sprite state
			SpriteState spriteStateTmp = spriteState;
			spriteStateTmp.highlightedSprite = spritesIdle[spriteIdx];
			spriteStateTmp.pressedSprite = spritesActive[spriteIdx];
			spriteState = spriteStateTmp;
			this.GetComponent<Image>().sprite = spritesIdle[spriteIdx];

			// call
			actions[spriteIdx]();

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
        public void reset()	{}

	}
}