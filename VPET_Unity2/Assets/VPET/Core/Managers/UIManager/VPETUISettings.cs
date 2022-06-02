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
