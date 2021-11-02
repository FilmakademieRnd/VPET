using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    public class ManipulatorSelector : MonoBehaviour
    {
        public Button selectionButton;

        public void Init(SceneObjectViewMenu sceneObjectMenu, Sprite icon, int index)
        {
            selectionButton.onClick.AddListener(() => sceneObjectMenu.SelectManipulator(index));
            selectionButton.image.sprite = icon;
        }
    }
}
