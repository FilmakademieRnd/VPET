/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "PexelSelectorModule.cs"
//! @brief implementation of the VPET UnityEditorModule, handling unity editor functionality.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.11.2021

using System;
using UnityEngine;
using UnityEditor;

namespace vpet
{
    //!
    //! Module to be used for connecting the Unity editor selection mechanism to VPET.
    //!
    public class UnityEditorModule : UIManagerModule
    {
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UnityEditorModule(string name, Manager manager) : base(name, manager)
        {
            load = false;
        }

        //! 
        //! Function called when Unity initializes the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            Selection.selectionChanged += SelectFunction;
        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        ~UnityEditorModule()
        {
            Selection.selectionChanged -= SelectFunction;
        }

        //!
        //! Function to connect the Unity editor selection to the VPET GameObject selection mechanism.
        //!
        private void SelectFunction()
        {
            GameObject gameObj = Selection.activeGameObject;

            if (gameObj != null)
            {
                SceneObject sceneObj = gameObj.GetComponent<SceneObject>();

                if (sceneObj != null)
                {
                    manager.clearSelectedObject();

                    Helpers.Log("selecting: " + sceneObj.ToString());
                    manager.addSelectedObject(sceneObj);
                }
            }
            else
                manager.clearSelectedObject();
        }
    }
}