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

//! @file "UICreator2DModuleSettings.cs"
//! @brief implementation of settings class for the UICreator2DModule to load resources like prefabs
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Settings class holding reference to manipulator icon prefab and list of manipulator configs
    //!
    [CreateAssetMenu(fileName = "DATA_VPET_2D_UI_Settings", menuName = "VPET/Create UI Creator 2D Settings file", order = 1)]
    public class UICreator2DModuleSettings : ScriptableObject
    {
        public ManipulatorSelector manipulatorSelector;
        public List<ManipulatorReference> manipulators;
    }

    //!
    //! Class holding associations between parameter type, maniplator and icon
    //!
    [System.Serializable]
    public class ManipulatorReference
    {
        public AbstractParameter.ParameterType valueType;
        public ParameterType parameterType;
        public Manipulator manipulatorPrefab;
        public Sprite selectorIcon;
    }

    //!
    //! Enum of different available types of parameters
    //!
    public enum ParameterType
    {
        Position,
        Rotation,
        Scale
    }

    //!
    //! Enum of different available types of 3D UI manipolators
    //!
    public enum ManipulatorType
    {
        ValueSlider,
        Spinner,
        Button,
        Toggle,
        TextInput,
        ColorPicker
    }
}
