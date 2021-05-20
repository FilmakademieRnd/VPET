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
        protected Dictionary<Type,Manager> managerList;

        //!
        //! get a manager from the core
        //! @param  name    name of manager
        //! @return requested manager or null
        //!
        public Manager getManager(Type type)
        {
            Manager manager;
            managerList.TryGetValue(type, out manager);
            return manager;
        }
    }
}