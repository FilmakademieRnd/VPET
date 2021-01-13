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
using UnityEngine.UI;

//!
//! This object represents a circular menu with variable amount of buttons.
//!
namespace vpet
{
	public class CenterMenu : Menu
	{
	    //!
	    //! center of the circular menu
	    //!
	    public Vector2 centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);
	
	    //!
	    //! radius of the circular menu
	    //!	    
        public float radius = Screen.width / 12;

        //!
        //! position to which activated buttons are moved
        //!
        // protected Vector3 activePosition = new Vector3(Screen.height / 16 + 10.0f, Screen.height - (Screen.height / 16 + 10.0f), 0.0f);
        protected Vector3 activePosition = new Vector3(-VPETSettings.Instance.canvasHalfWidth + UI.ButtonOffset, (VPETSettings.Instance.canvasHalfHeight - UI.ButtonOffset) * VPETSettings.Instance.canvasAspectScaleFactor, 0.0f);


        public MenuButtonToggle GravityButton;

        public void SetGravityButton( bool state )
        {
            if ( GravityButton )
                GravityButton.Toggled = state;
        }

        //!
        //! animate buttons in this menu set on every update
        //!
        protected override void animatedDraw()
	    {
            /*
            // animated
            foreach ( GameObject button in this.ButtonsActive() )
            {
                button.GetComponent<RectTransform>().position = getButtonPosition(i,currentDelta);
                i++;
            }
            */
            // fade
            base.animatedDraw();
        }

        //!
        //! move button to active position
        //! @param      button      currently active button, to be moved
        //!
        public override void animateActive(GameObject button)
	    {
	        activeButton = button;
			// set directly an skip animation
	        activeButton.GetComponent<RectTransform>().localPosition = activePosition;
	        
			// hide all except of active button
			foreach (GameObject g in Buttons())
			{
				if (g != activeButton)
					g.SetActive(false);
			}

	    }

	    //!
	    //! place all available buttons on a circle around the center of this menu, with equal spacing
	    //!
	    protected override void arrange()
	    {
	        float delta = (2*Mathf.PI)/(float)ActiveButtonCount;
	
	        int i = 0;
	        foreach (GameObject button in ButtonsActive())
	        {
	            button.GetComponent<RectTransform>().position = getButtonPosition(i, delta);
	            i++;
	        }
	    }
	
	    //! 
	    //! calulate the absolute position of the Button
	    //! @param      index       the index of he button (buttons will be sorted by index in menu)
	    //! @param      delta       angle between each pair of buttons
	    //! @return     representing the new position 
	    //!
	    private Vector2 getButtonPosition(int index, float delta){
	        return  centerPoint - Vector2.Scale(new Vector2(-1,1), new Vector2(radius * Mathf.Cos((delta * index) - (Mathf.PI / 2.0f)),
	                                         radius * Mathf.Sin((delta * index) - (Mathf.PI / 2.0f))) - new Vector2(Screen.width / 40,Screen.width / 40));
	    }
    }
}