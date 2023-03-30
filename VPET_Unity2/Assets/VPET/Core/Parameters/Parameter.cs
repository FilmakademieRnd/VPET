/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "parameter.cs"
//! @brief Implementation of the vpet parameter
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.02.2023

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace vpet
{

    //!
    //! Parameter base class.
    //!
    [Serializable]
    public abstract class AbstractParameter
    {
        //!
        //! The name of the parameter.
        //!
        [SerializeField]
        protected string _name;
        //!
        //! List for mapping VPET parameter types to C# types and visa versa.
        //!
        private static readonly List<Type> _paramTypes = new List<Type> { typeof(Action),
                                                                          typeof(bool),
                                                                          typeof(int),
                                                                          typeof(float),
                                                                          typeof(Vector2),
                                                                          typeof(Vector3),
                                                                          typeof(Vector4),
                                                                          typeof(Quaternion),
                                                                          typeof(Color),
                                                                          typeof(string),
                                                                          typeof(int)};
        //!
        //! Definition of VPETs parameter types
        //!
        public enum ParameterType : byte { ACTION, BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, QUATERNION, COLOR, STRING, LIST, UNKNOWN = 100 }
        //!
        //! The parameters C# type.
        //!
        [SerializeField]
        protected ParameterType _type;
        //!
        //! A reference to the parameters parent object.
        //!
        protected ParameterObject _parent;
        //!
        //! The unique id of this parameter.
        //!
        protected short _id;
        //!
        //! Flag that determines whether a Parameter will be distributed.
        //!
        public bool _distribute;
        //!
        //! Flag that determines whether a Parameter will be networl locked.
        //!
        protected bool _networkLock;
        //!
        //! Getter for unique id of this parameter.
        //!
        public short id
        {
            get => _id;
        }
        //!
        //! Flag that determines whether a Parameter will be locked for network comunication.
        //!
        public bool isNetworkLocked
        {
            get => _networkLock;
        }
        //!
        //! Getter for parameters C# type.
        //!
        public Type cType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => toCType(_type);
        }
        //!
        //! Getter for parameters VPET type.
        //!
        public ParameterType vpetType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _type;
        }
        //!
        //! Getter for parameters name.
        //!
        public ref string name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _name;
        }
        //!
        //! Getter for parameters parent.
        //!
        public ParameterObject parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _parent;
        }

        //!
        //! abstract reset function for the abstract parameter
        //!
        public abstract void reset();

        //!
        //! Fuction that determines a parameters C# type from a VPET type.
        //!
        //! @param t The C# type from which the VPET type is to be determined. 
        //! @return The determined C# type.
        //!
        protected static Type toCType(ParameterType t)
        {
            return _paramTypes[(int)t];
        }
        //!
        //! Fuction that determines a parameters VPET type from a C# type.
        //!
        //! @param t The VPET type from which the C# type is to be determined. 
        //! @return The determined VPET type.
        //!
        protected static ParameterType toVPETType(Type t)
        {
            int idx = _paramTypes.FindIndex(item => item.Equals(t));
            if (idx == -1)
                return (ParameterType)100;
            else
                return (ParameterType)idx;
        }
        //!
        //! Abstract definition of the function for serializing the parameters data.
        //! 
        //! @param startoffset The offset in bytes within the generated array at which the data should start at.
        //! @return The Parameters data serialized as a byte array.
        //! 
        public abstract void Serialize(Span<byte> targetSpan);
        //!
        //! Abstract definition of the function for deserializing parameter data.
        //! 
        //! @param data The byte data to be deserialized and copyed to the parameters value.
        //! 
        public abstract void deSerialize(byte[] data, int offset);

        //!
        //! Abstract definition of function called to copy value of other parameter
        //! @param v new value to be set. Value will be casted automatically
        //!
        public abstract void copyValue(AbstractParameter v);

        public abstract int dataSize();
    }

    [Serializable]
    //!
    //! Parameter class defining the fundamental functionality and interface
    //!
    public class Parameter<T> : AbstractParameter
    {
        [SerializeField]
        //!
        //! The parameters value as a template.
        //!
        protected T _value;

        [SerializeField]
        //!
        //! The initial value of the parameter at constuction time.
        //!
        protected T _initialValue;
        //!
        //! The size of the serialized data of the parameter.
        //!
        protected short _dataSize = -1;
        //!
        //! Getter for the size of the serialized data of the parameter.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int dataSize()
        {
            switch (_type)
            {
                case ParameterType.STRING:
                    return ((string)Convert.ChangeType(_value, typeof(string))).Length;
                default:
                    return _dataSize;
            }
        }
        //!
        //! The next and the previous active keyframe index (for animation).
        //!
        private int _nextIdx, _prevIdx;
        //!
        //! The list of keyframes (for animation).
        //!
        private List<Key<T>> _keyList = null;
        //!
        //! A reference to the key list (for animation).
        //!
        public ref List<Key<T>> _key_List { get => ref _keyList; }
        //!
        //! A reference to the Animation Manager.
        //!
        private AnimationManager _animationManager = null;
        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<T> hasChanged;

        //!
        //! The paramters constructor, initializing members.
        //!
        //! @param value The value of the parameder as the defined type T.
        //! @param name The parameters name.
        //! @param name The parameters parent ParameterObject.
        //! @param name Flag that determines whether a Parameter will be distributed.
        //!
        public Parameter(T value, string name, ParameterObject parent = null, bool distribute = true)
        {
            _value = value;
            _name = name;
            _parent = parent;
            _type = toVPETType(typeof(T));
            _distribute = distribute;
            _initialValue = value;
            _nextIdx = 0;
            _prevIdx = 0;

            // initialize data size
            switch (_type)
            {
                case ParameterType.BOOL:
                    _dataSize = 1;
                    break;
                case ParameterType.INT:
                case ParameterType.FLOAT:
                    _dataSize = 4;
                    break;
                case ParameterType.VECTOR2:
                    _dataSize = 8;
                    break;
                case ParameterType.VECTOR3:
                    _dataSize = 12;
                    break;
                case ParameterType.VECTOR4:
                case ParameterType.QUATERNION:
                case ParameterType.COLOR:
                    _dataSize = 16;
                    break;
                default:
                    _dataSize = -1;
                    break;
            }

            // check parent
            if (parent)
            {
                _id = (short)_parent.parameterList.Count;
                _parent.parameterList.Add(this);
            }
            else
            {
                _id = -1;
                _distribute = false;
            }
        }

        //!
        //! Copy Constructor
        //! @param p source parameter to copy values from
        //!
        public Parameter(Parameter<T> p)
        {
            _value = p._value;
            _name = p._name;
            _parent = p._parent;
            _id = p._id;
            _type = p._type;
            _dataSize = p._dataSize;
            _distribute = p._distribute;
            _initialValue = p._initialValue;
            _nextIdx = 0;
            _prevIdx = 0;
            _keyList = p._keyList;
            _animationManager = p._animationManager;

            if (_keyList != null && _animationManager != null)
                _animationManager.animationUpdate += updateValue;
        }

        //!
        //! Destructor
        //!
        ~Parameter()
        {
            clearKeys();
        }

        //!
        //! Getter and setter for the parameters value. 
        //!
        public T value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { setValue(value); }
        }

        //!
        //! function called to change a parameters value.
        //! @param   v new value to be set
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, v);
        }

        //!
        //! function called to copy value of other parameter
        //! might break if parameter types do not match 
        //! @param p parameter to copy value from
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void copyValue(AbstractParameter p)
        {
            try
            {
                _value = ((Parameter<T>)p).value;
                hasChanged?.Invoke(this, ((Parameter<T>)p).value);
            }
            catch
            {
                Debug.Log("Could not cast parameter while executing copyValue() for parameter " + this.name + " from " + this.parent.name);
            }
        }

        //!
        //! reset parameter to initial value
        //!
        public override void reset()
        {
            if (!EqualityComparer<T>.Default.Equals(_value, _initialValue))
            {
                _value = _initialValue;
                hasChanged?.Invoke(this, _value);
            }
        }

        /////////////////////////////////////////////////////////
        /////////////////////// Animation ///////////////////////
        /////////////////////////////////////////////////////////

        //!
        //! Initializes the parameters animation functionality,
        //!
        private void initAnimation()
        {
            _keyList ??= new List<Key<T>>();

            if (_animationManager == null)
            {
                _animationManager = ParameterObject.core.getManager<AnimationManager>();
                _animationManager.animationUpdate += updateValue;
            }
        }

        //!
        //! Insert a given key element to the parameters key list, at the corresponding index.
        //!
        //! @param key The key to be added to the parameters key list.
        //!
        public void addKey(Key<T> key)
        {
            if (!isAnimated())
                initAnimation();

            int i = findNextKeyIndex(key);
            if (i == -1)
            {
                int i2 = _keyList.IndexOf(key);
                if (i2 > -1)
                    _keyList[i2].value = key.value;
                else
                    _keyList.Add(key);
            }
            else
                _keyList.Insert(i, key);

        }

        //!
        //! Revove a given key element from the parameters key list.
        //!
        //! @param key The key to be removed from the parameters key list.
        //!
        public void removeKey(Key<T> key)
        {
            if (_keyList != null)
            {
                _keyList.Remove(key);
                if (_keyList.Count == 0)
                    _animationManager.animationUpdate -= updateValue;
            }
        }

        //!
        //! Create and insert a new key element to the parameters key list, 
        //! based on the current parameter value and Animation Manager time.
        //!
        public void setKey()
        {
            addKey(new Key<T>(_animationManager.time, value));
        }

        //!
        //! Clear the parameters key list and disable the animation functionality.
        //!
        private void clearKeys()
        {
            if (_animationManager != null)
                _animationManager.animationUpdate -= updateValue;

            if (_keyList != null)
                _keyList.Clear();
        }

        //!
        //! Calculate the parameters value based on the keylist and given time.
        //!
        //! @param o A reference to the Animation Manager.
        //! @param time The given time used to calulate the parameters new value.
        //!
        private void updateValue(object o, float time)
        {
            if (isAnimated())
            {
                // current time is NOT in between the two active keys
                if (time < _keyList[_prevIdx].time || time > _keyList[_nextIdx].time)
                {
                    int i = findNextKeyIndex(time);
                    // current time is bigger than all keys in list
                    if (i == -1)
                        _nextIdx = _prevIdx = _keyList.Count - 1;
                    else
                    {
                        // current time is smaller than all keys in list
                        if (i == 0)
                            _nextIdx = _prevIdx = 0;
                        // current time is somewhere between all keys in list
                        else
                        {
                            _nextIdx = i;
                            _prevIdx = i - 1;
                        }
                    }
                }
                value = interpolate(time);
            }
        }

        //!
        //! Function for searching the next bigger key index in the key list.
        //!
        //! @param key The key on which the index is to be searched.
        //! @return The next bigger index in the keylist.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int findNextKeyIndex(Key<T> key)
        {
            return _keyList.FindIndex(i => i.time > key.time);
        }

        //!
        //! Function for searching the next bigger key index in the key list.
        //!
        //! @param time The time on which the index is to be searched.
        //! @return The next bigger index in the keylist.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int findNextKeyIndex(float time)
        {
            return _keyList.FindIndex(i => i.time >= time);
        }

        //!
        //! Function that returns the current animation state of the parameter.
        //!
        //! @return The current animation state of the parameter.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isAnimated()
        {
            if (_keyList != null)
                return _keyList.Count > 0;
            else
                return false;
        }

        //!
        //! Function that interpolates the current parameter value based on a given
        //! time and the previous and next time indices.
        //!
        //! @parameter time The given time used to interpolate the parameters value.
        //! @return The interpolated parameter value.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T interpolate(float time)
        {
            switch (_type)
            {
                case ParameterType.FLOAT:
                    float inBetween = (time - _keyList[_prevIdx].time) / (_keyList[_prevIdx].time - _keyList[_nextIdx].time);
                    float s1 = 1.0f - (_keyList[_nextIdx].time - inBetween) / (_keyList[_nextIdx].time - _keyList[_prevIdx].time);
                    return (T)(object)(((float)(object)_keyList[_prevIdx].value) * (1.0f - s1) + ((float)(object)_keyList[_nextIdx].value) * s1);
                default:
                    return default(T);
            }
        }

          /////////////////////////////////////////////////////////////
         /////////////////////// Serialisation ///////////////////////
        /////////////////////////////////////////////////////////////

        //!
        //! Function for serializing the parameters data.
        //! 
        //! @param startoffset The offset in bytes within the generated array at which the data should start at.
        //! @return The Parameters data serialized as a byte array.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Serialize(Span<byte> targetSpan)
        {
            switch (_type)
            {
                case ParameterType.BOOL:
                    {
                        targetSpan[0] = Convert.ToByte(_value);
                        break;
                    }
                case ParameterType.INT:
                    {
                        BitConverter.TryWriteBytes(targetSpan, Convert.ToInt32(_value));
                        break;
                    }
                case ParameterType.FLOAT:
                    {
                        BitConverter.TryWriteBytes(targetSpan, Convert.ToSingle(_value));
                        break;
                    }
                case ParameterType.VECTOR2:
                    {
                        Vector2 obj = (Vector2)Convert.ChangeType(_value, typeof(Vector2));

                        BitConverter.TryWriteBytes(targetSpan.Slice(0, 4), obj.x);
                        BitConverter.TryWriteBytes(targetSpan.Slice(4, 4), obj.y);

                        break;
                    }
                case ParameterType.VECTOR3:
                    {
                        Vector3 obj = (Vector3)Convert.ChangeType(_value, typeof(Vector3));

                        BitConverter.TryWriteBytes(targetSpan.Slice(0, 4), obj.x);
                        BitConverter.TryWriteBytes(targetSpan.Slice(4, 4), obj.y);
                        BitConverter.TryWriteBytes(targetSpan.Slice(8, 4), obj.z);

                        break;
                    }
                case ParameterType.VECTOR4:
                    {
                        Vector4 obj = (Vector4)Convert.ChangeType(_value, typeof(Vector4));

                        BitConverter.TryWriteBytes(targetSpan.Slice(0, 4), obj.x);
                        BitConverter.TryWriteBytes(targetSpan.Slice(4, 4), obj.y);
                        BitConverter.TryWriteBytes(targetSpan.Slice(8, 4), obj.z);
                        BitConverter.TryWriteBytes(targetSpan.Slice(12, 4), obj.w);

                        break;
                    }
                case ParameterType.QUATERNION:
                    {
                        Quaternion obj = (Quaternion)Convert.ChangeType(_value, typeof(Quaternion));

                        BitConverter.TryWriteBytes(targetSpan.Slice(0, 4), obj.x);
                        BitConverter.TryWriteBytes(targetSpan.Slice(4, 4), obj.y);
                        BitConverter.TryWriteBytes(targetSpan.Slice(8, 4), obj.z);
                        BitConverter.TryWriteBytes(targetSpan.Slice(12, 4), obj.w);

                        break;
                    }
                case ParameterType.COLOR:
                    {
                        Color obj = (Color)Convert.ChangeType(_value, typeof(Color));

                        BitConverter.TryWriteBytes(targetSpan.Slice(0, 4), obj.r);
                        BitConverter.TryWriteBytes(targetSpan.Slice(4, 4), obj.g);
                        BitConverter.TryWriteBytes(targetSpan.Slice(8, 4), obj.b);
                        BitConverter.TryWriteBytes(targetSpan.Slice(12, 4), obj.a);

                        break;
                    }
                case ParameterType.STRING:
                    {
                        string obj = (string)Convert.ChangeType(_value, typeof(string));
                        targetSpan = Encoding.UTF8.GetBytes(obj);

                        break;
                    }

            }
        }

        //!
        //! Function for deserializing parameter _data.
        //! 
        //! @param _data The byte _data to be deserialized and copyed to the parameters value.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void deSerialize(byte[] data, int offset)
        {
            // [REVIEW]
            // Would a read from a span be faster then from byte[] ???
            //ReadOnlySpan<byte> dataSpan = new ReadOnlySpan<byte>(data);
            switch (_type)
            {
                case ParameterType.BOOL:
                    _value = (T)(object)BitConverter.ToBoolean(data, offset);
                    break;
                case ParameterType.INT:
                    _value = (T)(object)BitConverter.ToInt32(data, offset);
                    break;
                case ParameterType.FLOAT:
                    _value = (T)(object)BitConverter.ToSingle(data, offset);
                    break;
                case ParameterType.VECTOR2:
                    _value = (T)(object)new Vector2(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset + 4));
                    break;
                case ParameterType.VECTOR3:
                    _value = (T)(object)new Vector3(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset + 4),
                                                    BitConverter.ToSingle(data, offset + 8));
                    break;
                case ParameterType.VECTOR4:
                    _value = (T)(object)new Vector4(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset + 4),
                                                    BitConverter.ToSingle(data, offset + 8),
                                                    BitConverter.ToSingle(data, offset + 12));
                    break;
                case ParameterType.QUATERNION:
                    _value = (T)(object)new Quaternion(BitConverter.ToSingle(data, offset),
                                                     BitConverter.ToSingle(data, offset + 4),
                                                     BitConverter.ToSingle(data, offset + 8),
                                                     BitConverter.ToSingle(data, offset + 12));
                    break;
                case ParameterType.COLOR:
                    _value = (T)(object)new Color(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset + 4),
                                                    BitConverter.ToSingle(data, offset + 8),
                                                    BitConverter.ToSingle(data, offset + 12));
                    break;
                case ParameterType.STRING:
                    _value = (T)(object)new string(Encoding.UTF8.GetString(data));
                    break;
                default:
                    return;
            }
            _networkLock = true;
                hasChanged?.Invoke(this, _value);
            _networkLock = false;
        }
    }

    public class RPCParameter<T> : Parameter<T>
    {
        public RPCParameter(T parameterValue, string name, ParameterObject parent = null, bool distribute = true) : base(parameterValue, name, parent, distribute)
        {

        }
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

    [Serializable]
    //!
    //! ListParameter class defining the fundamental functionality and interface
    //!
    public class ListParameter : Parameter<int>
    {
        //!
        //! The ListParamters constructor, initializing members.
        //!
        //! @param parameterList The list of parameders with the given type T.
        //! @param name The parameters name.
        //! @param name The parameters parent ParameterObject.
        //! @param name Flag that determines whether a Parameter will be distributed.
        //!
        public ListParameter(List<AbstractParameter> parameterList, string name, ParameterObject parent = null, bool distribute = true) : base(0, name, parent, distribute)
        {
            _parameterList = parameterList;
            _type = ParameterType.LIST;
        }

        //!
        //! Constructor initializing members.
        //!
        public ListParameter(string name, ParameterObject parent = null) : this(new List<AbstractParameter>(), name, parent)
        { }

        [SerializeField]
        //!
        //! The ListParameters parameter list.
        //!
        private List<AbstractParameter> _parameterList;

        //!
        //! Getter and setter for the parameter list.
        //!
        public List<AbstractParameter> parameterList
        {
            set => _parameterList = value;
            get => _parameterList;
        }

        //!
        //! The function called to change a parameter in the parameter list.
        //! @param idx The list index of the parameter to be replaced.
        //! @param p The new parameter.
        //!
        public void setParameter(int idx, AbstractParameter p)
        {
            if (idx < _parameterList.Count)
            {
                _parameterList[idx] = p;
            }
            else
                Helpers.Log("Parameter index for" + p.name + "exceeds length of list " + this.name, Helpers.logMsgType.WARNING);
        }

        //!
        //! The function for adding a parameter to the parameter list.
        //! @param p The parameter to be added to the parameter list.
        //!
        public void addParameter(AbstractParameter p)
        {
            if (!_parameterList.Contains(p))
            {
                _parameterList.Add(p);
            }
            else
                Helpers.Log("Parameter " + p.name + " already exists in list " + this.name, Helpers.logMsgType.WARNING);
        }

        //!
        //! The function for removing a parameter from the parameter list.
        //! @param p The parameter to be femoved from the parameter list.
        //!
        public void removeParameter(AbstractParameter p)
        {
            if (_parameterList.Contains(p))
            {
                _parameterList.Remove(p);
            }
            else
                Helpers.Log("Parameter " + p.name + " does not exists in list " + this.name, Helpers.logMsgType.WARNING);
        }

        //!
        //! Function for selecting the active index.
        //!
        //! @param idx The new active index to be selected.
        //!
        public void select(int idx)
        {
            setValue(idx);
        }
    }
}
