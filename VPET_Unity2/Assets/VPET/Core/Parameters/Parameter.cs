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
        //! Flag that determines whether a Parameter will be distributed.
        //!
        public bool _distribute;
        //!
        //! The unique id of this parameter.
        //!
        protected short _id;
        //!
        //! The parameters C# type.
        //!
        protected ParameterType _type;
        //!
        //! The name of the parameter.
        //!
        [SerializeField]
        protected string _name;
        //!
        //! A reference to the parameters parent object.
        //!
        protected ParameterObject _parent;
        //!
        //! Definition of VPETs parameter types
        //!
        public enum ParameterType : byte { ACTION, BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, QUATERNION, COLOR, STRING, LIST, UNKNOWN = 100 }

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
        public abstract byte[] Serialize(int startoffset);
        //!
        //! Abstract definition of the function for deserializing parameter data.
        //! 
        //! @param data The byte data to be deserialized and copyed to the parameters value.
        //! 
        public abstract void deSerialize(ref byte[] data, int offset);
    }

    [Serializable]
    //!
    //! Parameter class defining the fundamental functionality and interface
    //!
    public class Parameter<T> : AbstractParameter, IFormattable
    {
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

            //history initialization
            _initialValue = value;
            _valueHistory = new List<T>();
            _valueHistory.Add(value);
            _currentHistoryPos = 0;
            //maximum amount of stored undo/redo (history) steps
            //[REVIEW] define globally? user adjustable?
            _maxHistory = 20;

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

        [SerializeField]
        //!
        //! The parameters value as a template.
        //!
        protected T _value;

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
        //! The initial value of the parameter at constuction time
        //!
        private T _initialValue;

        //!
        //! A list of last editings to this parameter for undo/redo functionality
        //!
        private List<T> _valueHistory;

        //!
        //! A list of last editings to this parameter for undo/redo functionality
        //!
        private int _currentHistoryPos;

        //!
        //! A list of last editings to this parameter for undo/redo functionality
        //!
        private int _maxHistory;

        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<T> hasChanged;

        //!
        //! Abstract definition of the function called to change a parameters value.
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, v);
        }

        //!
        //! adds the current value of the parameter to the history
        //!
        public void addHistoryStep(object sender, bool e)
        {
            if (_currentHistoryPos < _valueHistory.Count - 1)
                _valueHistory.RemoveRange(_currentHistoryPos + 1, (_valueHistory.Count - _currentHistoryPos - 1));
            _valueHistory.Add(_value);
            if (_valueHistory.Count <= _maxHistory)
                _currentHistoryPos++;
            else
                _valueHistory.RemoveAt(0);
        }

        //!
        //! undo the latest change to the parameter
        //! @return sucess of undo (false if no earlier versions are available)
        //!
        public bool undoStep()
        {
            if (_currentHistoryPos > 0)
            {
                _value = _valueHistory[_currentHistoryPos - 1];
                hasChanged?.Invoke(this, _value);
                _currentHistoryPos--;
                return true;
            }
            return false;
        }

        //!
        //! redo the next change to the parameter
        //! @return sucess of redo (false if no later versions are available)
        //!
        public bool redoStep()
        {
            if (_currentHistoryPos < (_valueHistory.Count - 1))
            {
                _value = _valueHistory[_currentHistoryPos + 1];
                hasChanged?.Invoke(this, _value);
                _currentHistoryPos++;
                return true;
            }
            return false;
        }

        //!
        //! reset parameter to initial value
        //!
        public void reset()
        {
            _currentHistoryPos = 0;
            _valueHistory = new List<T>();
            _valueHistory.Add(_initialValue);
            _value = _initialValue;
            hasChanged?.Invoke(this, _value);

        }

        //!
        //! Function for serializing the parameters data.
        //! 
        //! @param startoffset The offset in bytes within the generated array at which the data should start at.
        //! @return The Parameters data serialized as a byte array.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte[] Serialize(int startoffset)
        {
            byte[] data = null;
            ParameterType vpetType = _type;

            switch (vpetType)
            {
                case ParameterType.BOOL:
                    {
                        data = new byte[1 + startoffset];
                        data[1 + startoffset] = Convert.ToByte(_value);
                        return data;
                    }
                case ParameterType.INT:
                    {
                        data = new byte[4 + startoffset];
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt32(_value)), 0, data, startoffset, 4);
                        return data;
                    }
                case ParameterType.FLOAT:
                    {
                        data = new byte[4 + startoffset];
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToSingle(_value)), 0, data, startoffset, 4);
                        return data;
                    }
                case ParameterType.VECTOR2:
                    {
                        data = new byte[8 + startoffset];
                        Vector2 obj = (Vector2)Convert.ChangeType(_value, typeof(Vector2));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset + 4, 4);
                        return data;
                    }
                case ParameterType.VECTOR3:
                    {
                        data = new byte[12 + startoffset];
                        Vector3 obj = (Vector3)Convert.ChangeType(_value, typeof(Vector3));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset + 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset + 8, 4);
                        return data;
                    }
                case ParameterType.VECTOR4:
                    {
                        data = new byte[16 + startoffset];
                        Vector4 obj = (Vector4)Convert.ChangeType(_value, typeof(Vector4));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset + 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset + 8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, startoffset + 12, 4);
                        return data;
                    }
                case ParameterType.QUATERNION:
                    {
                        data = new byte[16 + startoffset];
                        Quaternion obj = (Quaternion)Convert.ChangeType(_value, typeof(Quaternion));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset + 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset + 8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, startoffset + 12, 4);
                        return data;
                    }
                case ParameterType.COLOR:
                    {
                        data = new byte[16 + startoffset];
                        Color obj = (Color)Convert.ChangeType(_value, typeof(Color));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.r), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.g), 0, data, startoffset + 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.b), 0, data, startoffset + 8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.a), 0, data, startoffset + 12, 4);
                        return data;
                    }
                case ParameterType.STRING:
                    {
                        string obj = (string)Convert.ChangeType(_value, typeof(string));
                        data = new byte[obj.Length + startoffset];

                        Buffer.BlockCopy(Encoding.UTF8.GetBytes(obj), 0, data, startoffset, obj.Length);

                        return data;
                    }
                default:
                    return data;

            }
        }

        //!
        //! Function for deserializing parameter data.
        //! 
        //! @param data The byte data to be deserialized and copyed to the parameters value.
        //! 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void deSerialize(ref byte[] data, int offset)
        {
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

        //!
        //! Function for string serialization. Used for storing parameters to disk.
        //! 
        //! @param format The format string (not used).
        //! @formatProvider The format prvider used to format the string (not used).
        //! 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return String.Format("({0}, {1})", _value.ToString(), name);
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
