/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
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

//! @file "ManipulatorSelector.cs"
//! @brief implementation of script attached to each button / selector to select a manipulator
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    public class ManipulatorSelector : MonoBehaviour
    {
        //associated button, linked in prefab
        public Button _selectionButton;

        //!
        //! Reference to VPET UI settings
        //!
        private VPETUISettings _uiSettings;


        //! 
        //! function to initialize the Selector
        //! @param module reference to the UICreator2DModule
        //! @param icon sprite to be used by this button
        //! @param index index of the associated manipulator in UICreator2DModule
        //!
        public void Init(UICreator2DModule module, VPETUISettings uiSettings, Sprite icon, int index)
        {
            _selectionButton.onClick.AddListener(() => module.createManipulator(index));
            _selectionButton.image.sprite = icon;
            _uiSettings = uiSettings;
        }

        //!
        //! function to show button highlighted / selected
        //!
        public void visualizeActive()
        {
            _selectionButton.gameObject.GetComponent<Image>().color = _uiSettings.colors.ElementSelection_Highlight;
        }

        //!
        //! function to show button idle
        //!
        public void visualizeIdle()
        {
            _selectionButton.gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}
