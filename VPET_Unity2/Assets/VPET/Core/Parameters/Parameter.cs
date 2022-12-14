﻿/*
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
//! @date 01.03.2022

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
        //! Getter for unique id of this parameter.
        //!
        public short id
        {
            get => _id;
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
        public string name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _name;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set => _name = value;
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
        //! Abstract definition of the function for serializing the parameters _data.
        //! 
        //! @param startoffset The offset in bytes within the generated array at which the _data should start at.
        //! @return The Parameters _data serialized as a byte array.
        //! 
        public abstract byte[] Serialize(int startoffset);
        //!
        //! Abstract definition of the function for deserializing parameter _data.
        //! 
        //! @param _data The byte _data to be deserialized and copyed to the parameters value.
        //! 
        public abstract void deSerialize(ref byte[] data, int offset);

        //!
        //! Abstract definition of function called to copy value of other parameter
        //! @param v new value to be set. Value will be casted automatically
        //!
        public abstract void copyValue(AbstractParameter v);
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
        //! The serialized data of the parameter.
        //!
        protected byte[] _data;
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
        public ref List<Key<T>> key_List { get => ref _keyList; }
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
            _data = null;

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
            _distribute = p._distribute;
            _initialValue = p._initialValue;
            _nextIdx = 0;
            _prevIdx = 0;
            _data = null;
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
                _animationManager = ParameterObject._core.getManager<AnimationManager>();
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
        public override byte[] Serialize(int startoffset)
        {
            ParameterType vpetType = _type;

            switch (vpetType)
            {
                case ParameterType.BOOL:
                    {
                        _data ??= new byte[1 + startoffset];
                        _data[1 + startoffset] = Convert.ToByte(_value);
                        return _data;
                    }
                case ParameterType.INT:
                    {
                        _data ??= new byte[4 + startoffset];
                        Helpers.copyArray(BitConverter.GetBytes(Convert.ToInt32(_value)), 0, _data, startoffset, 4);
                        return _data;
                    }
                case ParameterType.FLOAT:
                    {
                        _data ??= new byte[4 + startoffset];
                        Helpers.copyArray(BitConverter.GetBytes(Convert.ToSingle(_value)), 0, _data, startoffset, 4);
                        return _data;
                    }
                case ParameterType.VECTOR2:
                    {
                        _data ??= new byte[8 + startoffset];
                        Vector2 obj = (Vector2)Convert.ChangeType(_value, typeof(Vector2));

                        Helpers.copyArray(BitConverter.GetBytes(obj.x), 0, _data, startoffset, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.y), 0, _data, startoffset + 4, 4);
                        return _data;
                    }
                case ParameterType.VECTOR3:
                    {
                        _data ??= new byte[12 + startoffset];
                        Vector3 obj = (Vector3)Convert.ChangeType(_value, typeof(Vector3));

                        Helpers.copyArray(BitConverter.GetBytes(obj.x), 0, _data, startoffset, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.y), 0, _data, startoffset + 4, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.z), 0, _data, startoffset + 8, 4);
                        return _data;
                    }
                case ParameterType.VECTOR4:
                    {
                        _data ??= new byte[16 + startoffset];
                        Vector4 obj = (Vector4)Convert.ChangeType(_value, typeof(Vector4));

                        Helpers.copyArray(BitConverter.GetBytes(obj.x), 0, _data, startoffset, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.y), 0, _data, startoffset + 4, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.z), 0, _data, startoffset + 8, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.w), 0, _data, startoffset + 12, 4);
                        return _data;
                    }
                case ParameterType.QUATERNION:
                    {
                        _data ??= new byte[16 + startoffset];
                        Quaternion obj = (Quaternion)Convert.ChangeType(_value, typeof(Quaternion));

                        Helpers.copyArray(BitConverter.GetBytes(obj.x), 0, _data, startoffset, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.y), 0, _data, startoffset + 4, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.z), 0, _data, startoffset + 8, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.w), 0, _data, startoffset + 12, 4);
                        return _data;
                    }
                case ParameterType.COLOR:
                    {
                        _data ??= new byte[16 + startoffset];
                        Color obj = (Color)Convert.ChangeType(_value, typeof(Color));

                        Helpers.copyArray(BitConverter.GetBytes(obj.r), 0, _data, startoffset, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.g), 0, _data, startoffset + 4, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.b), 0, _data, startoffset + 8, 4);
                        Helpers.copyArray(BitConverter.GetBytes(obj.a), 0, _data, startoffset + 12, 4);
                        return _data;
                    }
                case ParameterType.STRING:
                    {
                        string obj = (string)Convert.ChangeType(_value, typeof(string));
                        _data ??= new byte[obj.Length + startoffset];

                        Buffer.BlockCopy(Encoding.UTF8.GetBytes(obj), 0, _data, startoffset, obj.Length);

                        return _data;
                    }
                default:
                    return _data;

            }
        }

        //!
        //! Function for deserializing parameter _data.
        //! 
        //! @param _data The byte _data to be deserialized and copyed to the parameters value.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void deSerialize(ref byte[] data, int offset)
        {
            // [REVIEW]
            // Would a read from a span be faster then from byte[] ???
            //ReadOnlySpan<byte> dataSpan = new ReadOnlySpan<byte>(data);
            ParameterType t = _type;
            switch (t)
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
            hasChanged?.Invoke(this, _value);
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
