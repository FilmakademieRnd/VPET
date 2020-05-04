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
using System.Collections;

namespace vpet
{
	public class SubMenu : Menu 
	{
		public enum direction { TOP, BOTTOM, LEFT, RIGHT };
	
		private direction dirToExpand = direction.TOP;
		public direction DirToExpand
		{
			set { dirToExpand = value; }
		}

		//!
		//! place all available buttons
		//!
		protected override void arrange()
		{
			int i = 0;
			if (VPETSettings.Instance.canvasAspectScaleFactor < 1)
				offset = offset - new Vector2(0,35);

			foreach (GameObject button in ButtonsActive())
            {
                button.GetComponent<RectTransform>().localPosition = getButtonPosition(i) + offset;
				i++;
			}
			Debug.Log("OFF " + offset);
		}

		//!
		//! calulate the absolute position of the Button
		//! @param      index       the index of he button (buttons will be sorted by index in menu)
		//! @return     representing the new position 
		//!
		private Vector2 getButtonPosition(int index)
		{
			int spacingVertical = UI.ButtonOffset;
			if (VPETSettings.Instance.canvasAspectScaleFactor < 1)
                spacingVertical = UI.ButtonOffset - 12;

			switch (dirToExpand)
			{
			case (direction.TOP):
				return new Vector2( 0, spacingVertical + index * spacingVertical);
			case (direction.BOTTOM):
				return new Vector2( 0, -spacingVertical - index * spacingVertical);
			case (direction.LEFT):
				return new Vector2( -UI.ButtonOffset - index * UI.ButtonOffset, 0 );
			case (direction.RIGHT):
				return new Vector2( UI.ButtonOffset + index * UI.ButtonOffset, 0 );
			}
			return new Vector2( UI.ButtonOffset + index * UI.ButtonOffset, 0);
		}

        //!
        //! override called when animation is finish and currentDelta is zero 
        //! hide also active button
        //!
        protected override void animatedDrawFinishOut()
        {
            activeButton = null;
            base.animatedDrawFinishOut();
        }

    }
}
