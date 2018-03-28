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
    public static class Extensions
    {
        //! float extensions to convert lens -> vertical FOV and vertical FOV -> lens
        //! sensor height -> default based on fullframe 35mm chipsize (36mm x 24mm)
        //! focal multiplier -> default 1.0 for fullframe chips
        //! reference [1]: http://paulbourke.net/miscellaneous/lens/
        //! reference [2]: http://www.tawbaware.com/maxlyons/calc.htm

        //! float extension: vertical field of view to lens focal length
        public static float vFovToLens(this float fov, float sensorHeight = 24f, float focalMultiplier = 1.0f, bool floor = true)
        {
            
            float lens = 1 / ((2*Mathf.Tan(fov*Mathf.PI/360)/(sensorHeight / focalMultiplier)));
            if(floor)
            {
                lens = Mathf.Floor(lens);
            }
            return lens;
        }

        //! float extension: lens focal length to vertical field of view
        public static float lensToVFov(this float lens, float sensorHeight = 24f, float focalMultiplier = 1.0f, bool floor = false)
        {
            float vFov = (2*Mathf.Atan((sensorHeight / focalMultiplier) / (2 *  lens)) * 180 / Mathf.PI);
            if(floor)
            {
                vFov = Mathf.Floor(lens);
            }
            return vFov;
        }

		//! Calculating horizontal field of view from vertical and reverse
		//! default width, height based on 3:2 aspect ratio
		//! reference [1]: https://gist.github.com/coastwise/5951291
		//! reference [2]: https://en.wikipedia.org/wiki/Field_of_view_in_video_games

		public static float vFovToHFov(this float vFov, float width = 3f, float height = 2f)
		{
			return (2 * Mathf.Atan( Mathf.Tan(vFov * Mathf.Deg2Rad / 2) * (width / height))) * Mathf.Rad2Deg;
		}

		public static float hFovToVFov(this float hFov, float width = 3f, float height = 2f)
		{
			return (2 * Mathf.Atan( Mathf.Tan(hFov * Mathf.Deg2Rad / 2) * (height / width))) * Mathf.Rad2Deg;
		}

    }
}

