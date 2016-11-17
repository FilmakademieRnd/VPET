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
            foreach (GameObject button in ButtonsActive())
            {
                button.GetComponent<RectTransform>().localPosition = getButtonPosition(i) + offset;
				i++;
			}
		}

		//!
		//! calulate the absolute position of the Button
		//! @param      index       the index of he button (buttons will be sorted by index in menu)
		//! @return     representing the new position 
		//!
		private Vector2 getButtonPosition(int index)
		{
			switch (dirToExpand)
			{
			case (direction.TOP):
				return new Vector2( 0, UI.ButtonOffset + index * UI.ButtonOffset );
			case (direction.BOTTOM):
				return new Vector2( 0, -UI.ButtonOffset - index * UI.ButtonOffset );
			case (direction.LEFT):
				return new Vector2( -UI.ButtonOffset - index * UI.ButtonOffset, 0 );
			case (direction.RIGHT):
				return new Vector2( UI.ButtonOffset + index * UI.ButtonOffset, 0 );
			}
			return new Vector2( UI.ButtonOffset + index * UI.ButtonOffset, 0);
		}
	
	}
}
