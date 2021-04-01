//! @file "parameter.cs"
//! @brief Implementation of the vpet parameter
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 16.03.2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Parameter class defining the fundamental functionality and interface
    //!
    public class Parameter<T>
    {
        public Parameter() {}

        public Parameter(T value)
        {
            _value = value;
        }

        //!
        //! Parameters member value
        //!
        private T _value;

        //!
        //! Getter for value
        //!
        public T value
        {
            get => _value;
        }

        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<TEventArgs> hasChanged;

        //!
        //! Definition of change function parameters.
        //!
        public class TEventArgs : EventArgs
        {
            public T value;
        }


        //!
        //! Abstract definition of the function called to change a parameters value.
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        public void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, new TEventArgs { value = _value });
        }
    }
}
