/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "UICreator2DModule.cs"
//! @brief implementation of VPET 2D UI scene creator module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.11.2021

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! implementation of VPET 2D UI scene creator module
    //!
    public class UICreator2DModule : UIManagerModule
    {

        private List<GameObject> UIParameterList = new List<GameObject>();
        public UICreator2DModuleSettings settings;
        private SceneObjectViewMenu m_sceneObjectViewMenu;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator2DModule(string name, Core core) : base(name, core)
        {

        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            manager.selectionChanged += createUI;
            settings = Resources.Load("DATA_VPET_2D_UI_Settings") as UICreator2DModuleSettings;

            m_sceneObjectViewMenu = GameObject.FindObjectOfType<SceneObjectViewMenu>();
        }

        //!
        //! Function that recreates the UI Layout.
        //! Being called when selection has changed.
        //!
        private void createUI(object sender, List<SceneObject> sceneObjects)
        {

            m_sceneObjectViewMenu.Clear();
            
            // [REVIEW]
            // UI Debug
            Debug.Log("--- Creating 2D UI ---");

            m_sceneObjectViewMenu.Init(this, sceneObjects);
        }

        private void clearUI()
        {
            m_sceneObjectViewMenu.Clear();
        }

    }
}