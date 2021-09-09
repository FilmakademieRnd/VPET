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
//! @date 16.03.2021

using System;
using System.Collections;
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
        //! The name of the parameter.
        //!
        private string _name;

        //!
        //! Getter for name.
        //!
        public string name
        {
            get => _name;
            protected set => _name = value;
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
        public Parameter() {}

        //!
        //! Constructor initializing members.
        //!
        public Parameter(T value, string name)
        {
            _value = value;
            this.name = name;
            Type type = typeof(T);

            if (type == typeof(bool))
                _type = ParameterType.BOOL;
            else if (type == typeof(int))
                _type = ParameterType.INT;
            else if (type == typeof(float))
                _type = ParameterType.FLOAT;
            else if (type == typeof(Vector2))
                _type = ParameterType.VECTOR2;
            else if (type == typeof(Vector3))
                _type = ParameterType.VECTOR3;
            else if (type == typeof(Vector4) || type == typeof(Quaternion) || type == typeof(Color) || type == typeof(Color32))
                _type = ParameterType.VECTOR4;
            else
                _type = ParameterType.UNKNOWN;

        }

        private enum ParameterType {BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, UNKNOWN}

        private ParameterType _type;

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
        //! Definition of change function parameters.
        //!
        public class TEventArgs : EventArgs
        {
            public T value;
        }
        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<TEventArgs> hasChanged;

        //!
        //! Abstract definition of the function called to change a parameters value.
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        private void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, new TEventArgs { value = _value });
        }

        public byte[] serialize()
        {
            byte[] data = new byte[0];
            switch (_type)
            {
                case ParameterType.BOOL:
                    {
                        data = new byte[2];
                        data[0] = (byte)_type;
                        data[1] = Convert.ToByte(_value);
                        break;
                    }

            }
            return data;
            //byte[] msg = new byte[18];
            //msg[0] = cID;
            //msg[1] = (byte)paramTarget;
            //msg[1] = (byte)paramType;
            //Buffer.BlockCopy(BitConverter.GetBytes((Int32)_.id), 0, msg, 2, 4);
            //Buffer.BlockCopy(BitConverter.GetBytes(locPos.x), 0, msg, 6, 4);
            //Buffer.BlockCopy(BitConverter.GetBytes(locPos.y), 0, msg, 10, 4);
            //Buffer.BlockCopy(BitConverter.GetBytes(locPos.z), 0, msg, 14, 4);
            //return msg;
        }
    }
}
