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


// delegate void callback();

//!
//! Script for the main menu placed in the upper right corner of the screen.
//!
namespace vpet
{
	public class SecondaryMenu : Menu
	{
		private int space = 0;
		private int width = 0;
	
	    protected override void arrange()
	    {
			// calculate total width
			width = 2 * UI.ButtonOffset;
			foreach (GameObject button in ButtonsActive())
			{
				if (button.GetComponent<IMenuButton> () == null)  // other objects (e.g. timeline
				{
					width += (int)(button.GetComponent<RectTransform> ().sizeDelta.x );
				} 
				else
				{
					width += UI.ButtonOffset;
				}
			}

	        int i = 0;
			space = 0;


	        foreach (GameObject button in ButtonsActive())
	        {
	            i++;
				if (button.GetComponent<IMenuButton>() == null ) // other objects (e.g. timeline
				{
					button.GetComponent<RectTransform>().localPosition = getButtonPosition(i, (int)( button.GetComponent<RectTransform>().sizeDelta.x / 2));	
					space = (int)( (button.GetComponent<RectTransform>().sizeDelta.x  ) );
				}
				else
				{
					button.GetComponent<RectTransform>().localPosition = getButtonPosition(i);	
				}
	        }

			// trigger ui change to update non-button objects
			UI.OnUIChanged.Invoke();

		}
	
	    //!
	    //! calulate the absolute position of the Button
	    //! @param      index       the index of he button (buttons will be sorted by index in menu)
	    //! @return     representing the new position 
	    //!
		private Vector2 getButtonPosition(int index, int shiftX = 0)
	    {
	        // return new Vector2(Screen.width / 2 - buttonCount * UI.ButtonOffset / 2 + index * UI.ButtonOffset, UI.ButtonOffset);
			return new Vector2(-width / 2  + space +  shiftX + index * UI.ButtonOffset, (-VPETSettings.Instance.canvasHalfHeight + UI.ButtonOffset) * VPETSettings.Instance.canvasAspectScaleFactor);
	    }
    }
}