/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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

//! @file "SpinnerQuaternion.cs"
//! @brief implementation of a Quaternion / rotation spinner manipulator
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SpinnerQuaternion : Spinner
    {
        public delegate void spinnerEventHandler(Quaternion v);
        //!
        //! Event emitted when parameter changed.
        //!
        public event spinnerEventHandler hasChanged;

        public override void InvokeHasChanged()
        {
            hasChanged?.Invoke(Quaternion.Euler(_value));
        }

        public override void LinkToParameter(AbstractParameter abstractParam)
        {
            Parameter<Quaternion> p = (Parameter<Quaternion>)abstractParam;
            hasChanged += p.setValue;
        }
    }
}
