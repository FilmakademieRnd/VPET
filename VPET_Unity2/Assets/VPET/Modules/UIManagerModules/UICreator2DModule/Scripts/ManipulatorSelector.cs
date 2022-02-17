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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
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
        public Button selectionButton;

        //! 
        //! function to initialize the Selector
        //! @param module reference to the UICreator2DModule
        //! @param icon sprite to be used by this button
        //! @param index index of the associated manipulator in UICreator2DModule
        //!
        public void Init(UICreator2DModule module, Sprite icon, int index)
        {
            selectionButton.onClick.AddListener(() => module.createManipulator(index));
            selectionButton.image.sprite = icon;
        }

        //!
        //! function to show button highlighted / selected
        //!
        public void visualizeActive()
        {
            selectionButton.gameObject.GetComponent<Image>().color = selectionButton.colors.selectedColor;
        }

        //!
        //! function to show button idle
        //!
        public void visualizeIdle()
        {
            selectionButton.gameObject.GetComponent<Image>().color = selectionButton.colors.normalColor;
        }
    }
}
