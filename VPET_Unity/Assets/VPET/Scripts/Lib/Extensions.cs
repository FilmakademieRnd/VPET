﻿/*
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
using System.Collections.Generic;

namespace vpet
{
    public static class Textures
    {

        //!
        //! function that determines if a texture has alpha
        //! @param  texture   the texture to be checked
        //!
        public static bool hasAlpha(Texture2D texture)
        {
            TextureFormat textureFormat = texture.format;
            return (textureFormat == TextureFormat.Alpha8 ||
                textureFormat == TextureFormat.ARGB4444 ||
                textureFormat == TextureFormat.ARGB32 ||
                textureFormat == TextureFormat.DXT5 ||
                textureFormat == TextureFormat.PVRTC_RGBA2 ||
                textureFormat == TextureFormat.PVRTC_RGBA4 ||
                textureFormat == TextureFormat.ETC2_RGBA8);
        }


    }

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

        //! recursive function traversing GameObject hierarchy from Object up to main scene to find object path
        //! @param  obj         Transform of GameObject to find the path for
        //! @return     path to gameObject started at main scene, separated by "/"
        public static string getPathString(Transform obj, Transform root, string separator = "/")
        {
            if (obj.parent)
            {
                //if (obj.parent == Camera.main.transform)
                //{
                //    return getPathString(mainController.oldParent, root, separator) + separator + obj.name;
                //}
                if (obj.transform.parent == root)
                    return obj.name;
                else
                {
                    return getPathString(obj.parent, root, separator) + separator + obj.name;
                }
            }
            return obj.name;
        }

        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

    }
}

