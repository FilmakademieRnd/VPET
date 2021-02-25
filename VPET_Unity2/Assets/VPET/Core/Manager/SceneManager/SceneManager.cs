//! @file "SceneManager.cs"
//! @brief scene manager implementation
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
    //! class managing all scene related aspects
    //!
    public class SceneManager : Manager
    {
        //!
        //! constructor
        //! @param  name    Name of the scene manager
        //! @param  moduleType  Type of module to add to this manager 
        //!
        SceneManager(string name, Type moduleType) : base(name, moduleType)
        {
            base.name = name;
        }
    }
}