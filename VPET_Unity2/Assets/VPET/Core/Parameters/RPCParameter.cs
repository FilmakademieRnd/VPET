using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using vpet;

namespace tracer
{
    //!
    //! RPCParameter class defining the fundamental functionality and interface
    //!
    public class RPCParameter<T> : Parameter<T>
    {
        public RPCParameter(T parameterValue, string name, ParameterObject parent = null, bool distribute = true) : base(parameterValue, name, parent, distribute) { }

        //!
        //! Action that will be executed when the parameter is evaluated.
        //!
        private Action<T> m_action;

        //!
        //! Function to set the action to be executed.
        //! 
        //! @param action The action to be set.
        //!
        public void setCall(Action<T> action)
        {
            m_action = action;
        }

        //!
        //! Function for deserializing parameter _data.
        //! 
        //! @param _data The byte _data to be deserialized and copyed to the parameters value.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void deSerialize(byte[] data, int offset)
        {
            base.deSerialize(data, offset);
            m_action.Invoke(_value);
        }

        //!
        //! Function to call the action associated with the Parameter. 
        //!
        public void Call()
        {
            InvokeHasChanged();
        }
    }

    //!
    //! RPCParameter class defining the fundamental functionality and interface
    //!
    public class RPCParameter : RPCParameter<object>
    {
        //! Simple constructor without RPC parameter.
        public RPCParameter(string name, ParameterObject parent = null, bool distribute = true) : base(parent, name, parent, distribute) { }

        //!
        //! Overrides the Parameters deserialization functionality, because we do not have a payload.
        //! 
        //! @param _data The byte _data to be deserialized and copyed to the parameters value. (not used)
        //! @param _offset The start offset in the given data array. (not used)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void deSerialize(byte[] data, int offset)
        {
            _networkLock = true;
            InvokeHasChanged();
            _networkLock = false;
        }
    }

}