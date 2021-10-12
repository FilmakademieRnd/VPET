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
//! @date 10.09.2021

using System;
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
        //! Definition of VPETs parameter types
        //!
        public enum ParameterType : byte { BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, QUATERNION, COLOR, UNKNOWN = 100 }

        //!
        //! List for mapping VPET parameter types to C# types and visa versa.
        //!
        private static readonly List<Type> _paramTypes = new List<Type> { typeof(bool),
                                                                          typeof(int),
                                                                          typeof(float),
                                                                          typeof(Vector2),
                                                                          typeof(Vector3),
                                                                          typeof(Vector4),
                                                                          typeof(Quaternion),
                                                                          typeof(Color) };
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
        //! Fuction that determines a parameters C# type from a VPET type.
        //!
        //! @param t The C# type from which the VPET type is to be determined. 
        //! @return The determined C# type.
        //!
        public static Type toCType(ParameterType t)
        {
            return _paramTypes[(int)t];
        }

        //!
        //! Fuction that determines a parameters VPET type from a C# type.
        //!
        //! @param t The VPET type from which the C# type is to be determined. 
        //! @return The determined VPET type.
        //!
        public static ParameterType toVPETType(Type t)
        {
            int idx = _paramTypes.FindIndex(item => item.Equals(t));
            if (idx == -1)
                return (ParameterType)100;
            else
                return (ParameterType)idx;
        }

    }

    //!
    //! Parameter class defining the fundamental functionality and interface
    //!
    public class Parameter<T> : AbstractParameter
    {
        //!
        //! Default constructor.
        //!
        public Parameter() { }
        //!
        //! Constructor initializing members.
        //!
        public Parameter(T value, string name, short id = -1)
        {
            _value = value;
            _name = name;
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
        //!  @return The Parameters data serialized as a byte array.
        //! 
        public byte[] serialize
        { get => Serialize(_value, _type); }

        // [REVIEW]
        // parameter should only store data and a minimal amount of functionaliy
        // --> move this to helpers, or network manager?
        public static byte[] Serialize(T value, Type t)
        {
            byte[] data;
            ParameterType vpetType = toVPETType(t);

            switch (vpetType)
            {
                case ParameterType.BOOL:
                    {
                        data = new byte[2];
                        data[1] = Convert.ToByte(value);
                        break;
                    }
                case ParameterType.INT:
                    {
                        data = new byte[5];
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt32(value)), 0, data, 1, 4);
                        break;
                    }
                case ParameterType.FLOAT:
                    {
                        data = new byte[5];
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToSingle(value)), 0, data, 1, 4);
                        break;
                    }
                case ParameterType.VECTOR2:
                    {
                        data = new byte[9];
                        Vector2 obj = (Vector2)Convert.ChangeType(value, typeof(Vector2));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, 1, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, 5, 4);
                        break;
                    }
                case ParameterType.VECTOR3:
                    {
                        data = new byte[13];
                        Vector3 obj = (Vector3)Convert.ChangeType(value, typeof(Vector3));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, 1, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, 5, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, 9, 4);
                        break;
                    }
                case ParameterType.VECTOR4:
                    {
                        data = new byte[17];
                        Vector4 obj = (Vector4)Convert.ChangeType(value, typeof(Vector4));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, 1, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, 5, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, 9, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, 13, 4);
                        break;
                    }
                case ParameterType.QUATERNION:
                    {
                        data = new byte[17];
                        Quaternion obj = (Quaternion)Convert.ChangeType(value, typeof(Quaternion));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.x), 0, data, 1, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.y), 0, data, 5, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.z), 0, data, 9, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.w), 0, data, 13, 4);
                        break;
                    }
                case ParameterType.COLOR:
                    {
                        data = new byte[17];
                        Color obj = (Color)Convert.ChangeType(value, typeof(Color));

                        Buffer.BlockCopy(BitConverter.GetBytes(obj.r), 0, data, 1, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.g), 0, data, 5, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.b), 0, data, 9, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(obj.a), 0, data, 13, 4);
                        break;
                    }
                default:
                    {
                        data = new byte[1];
                        break;
                    }

            }
            data[0] = (byte) vpetType;

            return data;
        }
    }
}
