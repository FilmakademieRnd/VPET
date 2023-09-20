/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "AnimationManager.cs"
//! @brief Implementation of the TRACER Animation Manager, managing all animation.
//! @author Simon Spielmann
//! @version 0
//! @date 22.08.2022


using System;
using System.Collections.Generic;
using UnityEngine;

namespace tracer
{
    //!
    //! Class implementing the input manager, managing all user inupts and mapping.
    //!
    public class AnimationManager : Manager
    {
        private float m_time;
        public float time { get => m_time; }
        public event EventHandler<float> animationUpdate;
        public AnimationManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_time = 0;
        }
        public void timelineUpdated(float time)
        {
            m_time = time;
            animationUpdate?.Invoke(this, time);
        }
    }

}
