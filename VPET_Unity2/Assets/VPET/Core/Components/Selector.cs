using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace vpet
{

    public class Selector : MonoBehaviour
    {
        //!
        //! The List holding the currently selected objects.
        //!
        private List<SceneObject> m_selectedObjects;

        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<SEventArgs> selectionChanged;

        //!
        //! Definition of change function parameters.
        //!
        public class SEventArgs : EventArgs
        {
            public List<SceneObject> value;
        }

        //!
        //! [REVIEW] Dummy function for testing selection mechanism.
        //!
        [ContextMenu("Gather SceneObjects")]
        void GatherSceneObjects()
        {
            m_selectedObjects.Clear();
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach(GameObject gameObject in allObjects)
            {
                SceneObject sceneObject = gameObject.GetComponent<SceneObject>();
                if (sceneObject)
                    m_selectedObjects.Add(sceneObject);
            }
            selectionChanged?.Invoke(this, new SEventArgs { value = m_selectedObjects });
        }

        void Awake()
        {
            m_selectedObjects = new List<SceneObject>();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
