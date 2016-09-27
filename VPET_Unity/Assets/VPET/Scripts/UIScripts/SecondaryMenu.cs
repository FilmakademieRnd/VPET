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


// delegate void callback();

//!
//! Script for the main menu placed in the upper right corner of the screen.
//!
namespace vpet
{
	public class SecondaryMenu : Menu
	{
        // temporary store
        public MenuButton TranlationButton;
        public MenuButton LinkToCamButton;
        public MenuButton PointToShoot;

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
					// HACK use event instead
					/*
					if ( button.GetComponent<TimeLineWidget>())
					{
						button.GetComponent<TimeLineWidget>().initMappingValues();
					}
					*/
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