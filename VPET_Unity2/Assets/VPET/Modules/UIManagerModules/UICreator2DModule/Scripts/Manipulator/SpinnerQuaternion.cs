using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SpinnerQuaternion : Spinner
    {
        public delegate void spinnerEventHandler(Quaternion v);
        //!
        //! Event emitted when parameter changed.
        //!
        public event spinnerEventHandler hasChanged;

        public override void InvokeHasChanged()
        {
            hasChanged?.Invoke(Quaternion.Euler(_value));
        }

        public override void LinkToParameter(AbstractParameter abstractParam)
        {
            Parameter<Quaternion> p = (Parameter<Quaternion>)abstractParam;
            hasChanged += p.setValue;
        }
    }
}
