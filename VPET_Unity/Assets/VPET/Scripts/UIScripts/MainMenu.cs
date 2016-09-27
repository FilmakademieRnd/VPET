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