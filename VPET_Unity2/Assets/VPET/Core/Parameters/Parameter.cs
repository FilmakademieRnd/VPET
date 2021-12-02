/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "parameter.cs"
//! @brief Implementation of the vpet parameter
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 17.11.2021

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Parameter base class.
    //!
    public abstract class AbstractParameter
    {

        //!
        //! The unique id of this parameter.
        //!
        protected short _id;
        //!
        //! The parameters C# type.
        //!
        protected Type _type;
        //!
        //! The name of the parameter.
        //!
        protected string _name;
        //!
        //! A reference to the parameters parent object.
        //!
        protected SceneObject _parent;
        //!
        //! Definition of VPETs parameter types
        //!
        public enum ParameterType : byte { ACTION, BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, QUATERNION, COLOR, STRING, UNKNOWN = 100 }

        //!
        //! List for mapping VPET parameter types to C# types and visa versa.
        //!
        private static readonly List<Type> _paramTypes = new List<Type> { typeof(object),
                                                                          typeof(bool),
                                                                          typeof(int),
                                                                          typeof(float),
                                                                          typeof(Vector2),
                                                                          typeof(Vector3),
                                                                          typeof(Vector4),
                                                                          typeof(Quaternion),
                                                                          typeof(Color),
                                                                          typeof(string)};
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
            get => _type;
        }
        //!
        //! Getter for parameters VPET type.
        //!
        public ParameterType vpetType
        {
            get => toVPETType(_type);
        }
        //!
        //! Getter for parameters name.
        //!
        public string name
        {
            get => _name;
            protected set => _name = value;
        }
        //!
        //! Getter for parameters parent.
        //!
        public SceneObject parent
        {
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

        public abstract byte[] Serialize(int startoffset);
        public abstract void deSerialize(ref byte[] data, int offset);

    }

    //!
    //! Parameter class defining the fundamental functionality and interface
    //!
    public class Parameter<T> : AbstractParameter
    {
        //!
        //! Constructor initializing members.
        //!
        public Parameter(T value, string name, SceneObject parent = null, short id = -1)
        {
            _value = value;
            _name = name;
            _parent = parent;
            _type = typeof(T);
            _id = id;
        }

        //!
        //! The parameters value as a template.
        //!
        private T _value;

        //!
        //! Getter and setter for the parameters value. 
        //!
        public T value
        {
            get => _value;
            set { setValue(value); }
        }

        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<T> hasChanged;

        //!
        //! Abstract definition of the function called to change a parameters value.
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        public void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, v);
        }

        //!
        //! Function for serializing the parameters data.
        //! 
        //! @param startoffset The offset in bytes within the generated array at which the data should start at.
        //! @return The Parameters data serialized as a byte array.
        //! 
        public override byte[] Serialize(int startoffset)
        {
            byte[] data = null;
            ParameterType vpetType = toVPETType(_type);

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
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset+4, 4);
                        return data;
                    }
                case ParameterType.VECTOR3:
                    {
                        data = new byte[12 + startoffset];
                        Vector3 obj = (Vector3)Convert.ChangeType(_value, typeof(Vector3));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset+4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset+8, 4);
                        return data;
                    }
                case ParameterType.VECTOR4:
                    {
                        data = new byte[16 + startoffset];
                        Vector4 obj = (Vector4)Convert.ChangeType(_value, typeof(Vector4));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset+4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset+8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, startoffset+12, 4);
                        return data;
                    }
                case ParameterType.QUATERNION:
                    {
                        data = new byte[16 + startoffset];
                        Quaternion obj = (Quaternion)Convert.ChangeType(_value, typeof(Quaternion));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, startoffset+4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, startoffset+8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, startoffset+12, 4);
                        return data;
                    }
                case ParameterType.COLOR:
                    {
                        data = new byte[16 + startoffset];
                        Color obj = (Color)Convert.ChangeType(_value, typeof(Color));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.r), 0, data, startoffset, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.g), 0, data, startoffset+4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.b), 0, data, startoffset+8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.a), 0, data, startoffset+12, 4);
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
        public override void deSerialize(ref byte[] data, int offset)
        {
            ParameterType t = toVPETType(_type);
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
                                                    BitConverter.ToSingle(data, offset+4));
                    break;
                case ParameterType.VECTOR3:
                    _value = (T)(object)new Vector3(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset+4),
                                                    BitConverter.ToSingle(data, offset+8));
                    break;
                case ParameterType.VECTOR4:
                    _value = (T)(object)new Vector4(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset+4),
                                                    BitConverter.ToSingle(data, offset+8),
                                                    BitConverter.ToSingle(data, offset+12));
                    break;
                case ParameterType.QUATERNION:
                    _value = (T)(object)new Quaternion(BitConverter.ToSingle(data, offset),
                                                     BitConverter.ToSingle(data, offset+4),
                                                     BitConverter.ToSingle(data, offset+8),
                                                     BitConverter.ToSingle(data, offset+12));
                    break;
                case ParameterType.COLOR:
                    _value = (T)(object)new Color(BitConverter.ToSingle(data, offset),
                                                    BitConverter.ToSingle(data, offset+4),
                                                    BitConverter.ToSingle(data, offset+8),
                                                    BitConverter.ToSingle(data, offset+12));
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
}
