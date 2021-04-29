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
        public static class Settings
        {
            //!
            //! Do we load scene from dump file
            //!
            public static bool loadSampleScene = true;

            //!
            //! Do we load textures
            //!
            public static bool loadTextures = true;

            //!
            //! The maximum extend of the scene
            //!
            public static Vector3 sceneBoundsMax = Vector3.positiveInfinity;
            public static Vector3 sceneBoundsMin = Vector3.negativeInfinity;
            public static float maxExtend = 1f;

            //!
            //! Light Intensity multiplicator
            //!
            public static float lightIntensityFactor = 1f;

            //!
            //! global scale of the scene
            //!
            public static float sceneScale = 1f;

        }

        //!
        //! constructor
        //! @param  name    Name of the scene manager
        //! @param  moduleType  Type of module to add to this manager 
        //!
        SceneManager(Type moduleType, CoreInterface vpetCore) : base(moduleType, vpetCore)
        {
        }
    }
}