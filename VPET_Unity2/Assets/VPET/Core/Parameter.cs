//! @file "parameter.cs"
//! @brief Implementation of the vpet parameter
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 16.03.2021

using System;
using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    //!
    //! Abstract parameter class defining the fundamental functionality and interface
    //!
    public abstract class AbstractParameter
    {
        //!
        //! Event emitted when parameter changed
        //!
        public event EventHandler hasChanged;

        //!
        //! Abstract definition of change function parameters
        //!
        public abstract class changeEventArgs : EventArgs{}

        //!
        //! Wrapper function to emit value changed event
        //!
        protected void callValueChanged()
        {
            if (hasChanged != null)
                hasChanged(this, null);
        }

        //!
        //! Function connecting an external event to change the parameters value
        //! @param   ev     The event to be connected
        //!
        public void connectToChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev += changeValue;
        }

        //!
        //! Function disconnecting an external event from the parameters change function
        //! @param   ev     The event to be disconnected
        //!
        public void disconnectChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev -= changeValue;
        }

        //!
        //! Abstract definition of the function called to change a parameters value
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        protected abstract void changeValue(object sender, changeEventArgs a);
    }

    //!
    //! Specific implementation for a float value
    //!
    public class FloatParameter : AbstractParameter
    {
        //!
        //! parameters float value
        //!
        private float value;

        //!
        //! definition of change function parameters
        //!
        public class floatEventArgs : changeEventArgs
        {
            public float value;
        }

        //!
        //! definition of the function called to change a float parameter value
        //! @param   sender     Object calling the change function
        //! @param   a          float value to be passed to the change function
        //!
        protected override void changeValue(object sender, changeEventArgs a)
        {
            value = ((floatEventArgs)a).value;
            callValueChanged();
        }
    }


    /*
    public class Parameter<T>
    {
        private string name;
        private T value;
        public event EventHandler hasChanged;
        public class changeEventArgs : EventArgs
        {
            public T value;
        }

        public void connectToChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev += changeValue;
        }

        public void removeFromChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev -= changeValue;
        }

        private void changeValue(object sender, changeEventArgs a)
        {
            value = a.value;
            if (hasChanged != null)
                hasChanged(this, null);
        }
    }

    */
}
