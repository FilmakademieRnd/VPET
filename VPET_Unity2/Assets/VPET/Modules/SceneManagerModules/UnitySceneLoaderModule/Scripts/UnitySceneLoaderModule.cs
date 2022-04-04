//! @file "UnitySceneLoaderModule.cs"
//! @brief implementation of unity scene loader module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    //!
    //! implementation of unity scene loader module
    //!
    public class UnitySceneLoaderModule : SceneManagerModule
    {
        //!
        //! constructor
        //! @param   name    Name of this module
        //!
        public UnitySceneLoaderModule(string name, Manager manager) : base(name, manager) { }
    }
}
