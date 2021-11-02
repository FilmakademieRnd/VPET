using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    [CreateAssetMenu(fileName = "DATA_VPET_2D_UI_Settings", menuName = "VPET/Create UI Creator 2D Settings file", order = 1)]
    public class UICreator2DModuleSettings : ScriptableObject
    {
        public ManipulatorSelector manipulatorSelector;
        public ParameterMapping parameterMapping;
        public List<ManipulatorReference> manipulators;
    }

    [System.Serializable]
    public struct ManipulatorReference
    {
        public ManipulatorType type;
        public GameObject manipulatorPrefab;
    }

    [System.Serializable]
    public class ParameterMapping : SerializableDictionary<AbstractParameter.ParameterType, ManipulatorSetting>
    {

    }

    [System.Serializable]
    public struct ManipulatorSetting
    {
        public ManipulatorType manipulator;
        public List<Sprite> uiSprites;
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
