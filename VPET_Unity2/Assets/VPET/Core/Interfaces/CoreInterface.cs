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
    }
}