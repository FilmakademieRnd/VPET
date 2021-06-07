//! @file "SceneManagerModule.cs"
//! @brief base implementation for scene manager modules
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! class for scene manager modules
    //!
    public class SceneManagerModule : Module
    {
        //!
        //! set/get the manager of this module.
        //!
        new public SceneManager manager
        {
            get => (SceneManager)_manager;
            set => _manager = value;
        }
        //!
        //! constructor
        //! @param  name    name of the module.
        //!
        public SceneManagerModule(string name) : base(name) => name = base.name;
    }
}
