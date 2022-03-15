using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    [CreateAssetMenu(fileName = "DATA_VPET_Colors", menuName = "VPET/Create VPET color file", order = 1)]
    public class VPETColorSettings : ScriptableObject
    {
        public List<NamedColor> colors;
    }

    [System.Serializable]
    public class NamedColor
    {
        public string name;
        public Color color;
    }
}
