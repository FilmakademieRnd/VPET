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
	//! Toggle button button inherit from Button and implement IMenuButton interface. If given the callback stored in onUntoggle gets called when untoggle the button.
	//!
	public class MenuButtonToggle : Button, IMenuButton
	{
		private Menu menu;
		public Menu Menu
		{
			set{ menu = value; }
		}

		private UnityAction onToggle = null;
		private UnityAction onUntoggle = null;
        private UnityAction onHold = null;
        private bool pointerDown = false;
        private float downTime = 0f;

        private bool toggled = false;
        private bool hold = false;
		public bool Toggled
        {
			get { return toggled; }
			set{ toggled = value;
				setSpriteFromToggle(); }
		}

		private Sprite highlightedSprite;

		public void AddAction(Sprite active, Sprite idle, UnityAction call)
		{
			if ( onToggle == null )
			{
				SpriteState newSprites = new SpriteState();
				newSprites.disabledSprite = idle;
				newSprites.highlightedSprite = idle;
				newSprites.pressedSprite = active;
				spriteState = newSprites;
				this.GetComponent<Image>().sprite = idle;
				highlightedSprite = idle;
				onToggle = call;
			}
			else
			{
				onUntoggle = call;
			}

			if ( onToggle == null )
			{
				interactable = false;
			}
		}

        public void AddHoldAction(UnityAction call)
        {
            onHold = call;
        }

        public void reset()
		{
            toggled = false;
			setSpriteFromToggle();
		}

		public override void OnPointerClick (PointerEventData eventData)
		{
            if (!hold)
            {
                toggled = !toggled;
                setSpriteFromToggle();

                if (toggled || onUntoggle == null)
                {
                    // set active button at menu
                    menu.ActiveButton = this.gameObject;
                    onToggle();
                }
                else
                {
                    // unset active button at menu
                    menu.ActiveButton = null;
                    onUntoggle();
                }

                // call base
                base.OnPointerClick(eventData);
            }
            else
                hold = false;
		}

        public override void OnPointerDown(PointerEventData eventData)
        {
            pointerDown = true;
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            pointerDown = false;
            downTime = 0f;
            base.OnPointerDown(eventData);
        }

        private void setSpriteFromToggle()
		{
			SpriteState spriteStateTmp = spriteState;

			if (toggled) 
			{
				spriteStateTmp.highlightedSprite = spriteStateTmp.pressedSprite;
			}
			else
			{
				spriteStateTmp.highlightedSprite= highlightedSprite;
			}
			spriteState = spriteStateTmp;

			GetComponent<Image>().sprite = spriteState.highlightedSprite;
		}


		public void setToggleState( bool toggle )
		{
			Toggled = toggle;
		}

        void Update()
        {
            if (pointerDown)
            {
                downTime += Time.deltaTime;
                if (downTime > 2f)
                {
                    if (onHold != null)
                    {
                        onHold();
                    }
                    hold = true;
                    pointerDown = false;
                    downTime = 0f;
                    reset();
                }
            }
        }
    }
}