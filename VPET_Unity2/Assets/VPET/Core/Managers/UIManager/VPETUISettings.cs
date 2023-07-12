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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace vpet
{
    [CreateAssetMenu(fileName = "DATA_VPET_UI", menuName = "VPET/Create VPET UI settings file", order = 1)]
    public class VPETUISettings : ScriptableObject
    {
        public TMP_FontAsset defaultFont;
        public int defaultFontSize;
        public Colors colors;
    }
    [System.Serializable]
    public class Colors
    {
        public Color FontColor;
        public Color FontRegular;
        public Color FontHighlighted;
        public Color MenuBG;
        public Color MenuTitleBG;
        public Color DropDown_TextfieldBG;
        public Color ButtonBG;
        public Color ElementSelection_Highlight;
        public Color ElementSelection_Default;
        public Color DefaultBG;
        public Color FloatingButtonBG;
        public Color FloatingButtonIcon;
    }
}
