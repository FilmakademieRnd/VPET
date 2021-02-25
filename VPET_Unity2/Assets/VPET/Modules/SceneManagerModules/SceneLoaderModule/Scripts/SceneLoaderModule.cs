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
    public class SceneLoaderModule : Module
    {
        //!
        //! constructor
        //! @param   name    Name of this module
        //!
        public SceneLoaderModule(string name) : base(name) => name = base.name;

        //! to be replaced
        public void Test()
        {
            UnitySceneLoaderModule m = (UnitySceneLoaderModule)manager.getModule(typeof(UnitySceneLoaderModule));
            manager.core.getManager(typeof(SceneManager)).getModule(typeof(UnitySceneLoaderModule));

        }

    }
}
