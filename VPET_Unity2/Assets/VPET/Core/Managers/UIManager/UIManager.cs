using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace vpet
{
    public class UIManager : Manager
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
        //! Constructor initializing member variables.
        //!
        public UIManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_selectedObjects = new List<SceneObject>();
        }

        //!
        //! Definition of change function parameters.
        //!
        public class SEventArgs : EventArgs
        {
            public List<SceneObject> value;
        }
    }
}