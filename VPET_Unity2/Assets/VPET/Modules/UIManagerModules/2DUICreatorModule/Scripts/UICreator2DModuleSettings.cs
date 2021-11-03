using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    [CreateAssetMenu(fileName = "DATA_VPET_2D_UI_Settings", menuName = "VPET/Create UI Creator 2D Settings file", order = 1)]
    public class UICreator2DModuleSettings : ScriptableObject
    {
        public ManipulatorSelector manipulatorSelector;
        public List<ManipulatorReference> manipulators;
    }

    [System.Serializable]
    public class ManipulatorReference
    {
        public AbstractParameter.ParameterType parameterType;
        public GameObject manipulatorPrefab;
        public Sprite selectorIcon;
    }

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
