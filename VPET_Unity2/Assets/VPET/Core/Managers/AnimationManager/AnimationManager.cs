/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "AnimationManager.cs"
//! @brief Implementation of the VPET Input Manager, managing all user inupts and mapping.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 22.08.2022


using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
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
