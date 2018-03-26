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
using UnityEngine.UI;

//!
//! Script for the main menu placed in the upper right corner of the screen.
//!
namespace vpet
{
	public class MainMenu : Menu
    {
        //!
        //! animate buttons in this menu set on every update
        //!
        protected override void animatedDraw()
	    {
            /*
            // animated motion
            foreach ( GameObject button in ButtonsActive() )
            {
                //button.GetComponent<RectTransform>().position = getButtonPosition(i, currentDelta);
                button.GetComponent<RectTransform>().localPosition = getButtonPosition(i, currentDelta);
                i++;
            }
            */
            base.animatedDraw();
        }
	
	    protected override void arrange()
	    {
	        int i = 0;
	        foreach (GameObject button in ButtonsActive())
	        {
	            button.GetComponent<RectTransform>().localPosition = getButtonPosition(i, 1f);
	            i++;
	        }
	    }
		
	    //!
	    //! calulate the absolute position of the Button
	    //! @param      index       the index of he button (buttons will be sorted by index in menu)
	    //! @param      delta       angle between each pair of buttons
	    //! @return     representing the new position 
	    //!
	    private Vector2 getButtonPosition(int index, float delta)
	    {
	        // return new Vector2(Screen.width - UI.ButtonOffset - delta * (index + 1) * UI.ButtonOffset, Screen.height - UI.ButtonOffset);
	        return new Vector2(VPETSettings.Instance.canvasHalfWidth - UI.ButtonOffset - delta * (index + 1) * UI.ButtonOffset, (VPETSettings.Instance.canvasHalfHeight - UI.ButtonOffset)*VPETSettings.Instance.canvasAspectScaleFactor);
	    }
    }
}