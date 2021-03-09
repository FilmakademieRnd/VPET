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
    public class VPET : CoreInterface
    {
        //!
        //! Initialization of all Managers and modules.
        //!
        void Awake()
        {
            //Create scene manager
            Manager sceneManager = new Manager(typeof(SceneManager), this);
            managerList.Add(typeof(SceneManager), sceneManager);
        }
    }
}