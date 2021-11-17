using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace vpet
{
    public class SpinnerVec3 : Spinner
    {
        public delegate void spinnerEventHandler(Vector3 v);
        //!
        //! Event emitted when parameter changed.
        //!
        public event spinnerEventHandler hasChanged;

        public override void InvokeHasChanged()
        {
            hasChanged?.Invoke(_value);
        }

        public override void LinkToParameter(AbstractParameter abstractParam)
        {
            Parameter<Vector3> p = (Parameter<Vector3>)abstractParam;
            hasChanged += p.setValue;
        }
    }
}