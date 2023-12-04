/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
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

//! @file "vpet.cs"
//! @brief VPET core implementation
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace vpet
{
    //!
    //! Central class for VPET initalization.
    //! Manages all VPETManagers and their modules.
    //!
    public class CoreInterface : MonoBehaviour
    {
        //!
        //! List of all registered VPETManagers.
        //!
        protected Dictionary<Type,Manager> m_managerList;

        //!
        //! Constructor
        //!
        public CoreInterface()
        {
            m_managerList = new Dictionary<Type, Manager>();
        }

        //!
        //! Returns the VPET manager with the given type.
        //!
        //! @tparam T The type of manager to be requested.
        //! @return The requested manager or null if not registered. 
        //!
        public T getManager<T>()
        {
            Manager manager = null;

            if (!m_managerList.TryGetValue(typeof(T), out manager))
                Helpers.Log(this.GetType().ToString() + " no manager of type " + typeof(T).ToString() + " registered.", Helpers.logMsgType.WARNING);

            return (T)(object) manager;
        }

        internal List<Manager> getManagers()
        {
            return new List<Manager>(m_managerList.Values);
        }
    }
}