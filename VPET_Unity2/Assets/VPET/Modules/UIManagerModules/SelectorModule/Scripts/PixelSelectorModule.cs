using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace vpet
{

    public class SelectorModule : UIManagerModule
    {
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public SelectorModule(string name, Core core) : base(name, core)
        {
        }



        ////!
        ////! [REVIEW] Dummy function for testing selection mechanism.
        ////!
        //[ContextMenu("Gather SceneObjects")]
        //void GatherSceneObjects()
        //{
        //    m_selectedObjects.Clear();
        //    GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        //    foreach(GameObject gameObject in allObjects)
        //    {
        //        SceneObject sceneObject = gameObject.GetComponent<SceneObject>();
        //        if (sceneObject)
        //            m_selectedObjects.Add(sceneObject);
        //    }
        //    selectionChanged?.Invoke(this, new SEventArgs { value = m_selectedObjects });
        //}



    }
}
