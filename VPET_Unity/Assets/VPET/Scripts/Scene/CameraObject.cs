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

//!
//! class to simple store camera values and to identify this gameobject to be a camera node
//!
namespace vpet
{
	public class CameraObject : MonoBehaviour 
	{
        //! enum listing the usable camera parameters (used by UI, Controller)
        public enum CameraParameter { FOV, LENS, APERTURE, FOCDIST, FOCSIZE };
        //!
        //! field of view (horizonal value from Katana)
        //!
        public float fov = 70f;
        //!
        //! near plane
        //!
        public float near = 0.1f;
        //!
        //! far plane
        //!
        public float far = 100000f;
        //!
        //! focus distance (in world space, meter)
        //!
        public float focDist = 1.7f;
        //!
        //! focus size
        //!
        public float focSize = 0.3f;
        //!
        //! aperture
        //!
        public float aperture = 0.5f;
    }
}