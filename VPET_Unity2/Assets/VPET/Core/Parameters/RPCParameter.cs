using System;
using System.Runtime.CompilerServices;

namespace vpet
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
        public Action<T> action
        {
            get => m_action;
            set => m_action = value;
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
    }

    //!
    //! RPCParameter class defining the fundamental functionality and interface
    //!
    public class RPCParameter : RPCParameter<object>
    {
        public RPCParameter(string name, ParameterObject parent = null, bool distribute = true) : base(null, name, parent, distribute) { }
    }
}