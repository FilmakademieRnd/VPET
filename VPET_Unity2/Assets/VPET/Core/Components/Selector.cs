using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{

    public class Selector : MonoBehaviour
    {
        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<SEventArgs> selectionChanged;

        //!
        //! Definition of change function parameters.
        //!
        public class SEventArgs : EventArgs
        {
            public List<AbstractParameter> value;
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
