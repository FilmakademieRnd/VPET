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
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
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
using System.Reflection;

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
            Manager manager;

            if (!m_managerList.TryGetValue(typeof(T), out manager))
                Helpers.Log(this.GetType().ToString() + " no manager of type " + typeof(T).ToString() + " registered.", Helpers.logMsgType.ERROR);

            return (T)(object) manager;
        }

        protected List<Manager> getManagers()
        {
            return new List<Manager>(m_managerList.Values);
        }
    }
}