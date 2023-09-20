/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "PexelSelectorModule.cs"
//! @brief implementation of the TRACER UnityEditorModule, handling unity editor functionality.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.11.2021

using System;
using UnityEngine;
using UnityEditor;

namespace tracer
{
    //!
    //! Module to be used for connecting the Unity editor selection mechanism to TRACER.
    //!
    public class UnityEditorModule : UIManagerModule
    {
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the TRACER core
        //!
        public UnityEditorModule(string name, Manager manager) : base(name, manager)
        {
            load = false;
        }

        //! 
        //! Function called when Unity initializes the TRACER core.
        //! 
        //! @param sender A reference to the TRACER core.
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
        //! Function to connect the Unity editor selection to the TRACER GameObject selection mechanism.
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