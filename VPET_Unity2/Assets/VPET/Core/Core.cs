//! @file "core.cs"
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
    public class Core : CoreInterface
    {
        //!
        //! Reference to the selection component.
        //!
        private Selector _selector;

        //!
        //! Getter for the selector.
        //!
        public Selector selector { get => _selector; }

        //!
        //! Initialization of all Managers and modules.
        //!
        void Awake()
        {
            // Create network manager
            NetworkManager networkManager = new NetworkManager(typeof(NetworkManagerModule), this);
            m_managerList.Add(typeof(NetworkManager), networkManager);

            //Create scene manager
            SceneManager sceneManager = new SceneManager(typeof(SceneManagerModule), this);
            m_managerList.Add(typeof(SceneManager), sceneManager);

            //Create UI manager
            UIManager uiManager = new UIManager(typeof(UIManagerModule), this);
            m_managerList.Add(typeof(UIManager), sceneManager);

            //Initializing core components
            //Create Selector
            _selector = new Selector();

        }
    }
}