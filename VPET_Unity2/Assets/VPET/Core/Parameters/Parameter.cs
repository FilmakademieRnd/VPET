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
        public Parameter() {}

        public Parameter(T value, string name)
        {
            _value = value;
            this.name = name;
        }

        //!
        //! Parameters member value
        //!
        private T _value;

        //!
        //! Getter and setter for value
        //!
        public T value
        {
            get => _value;
            set { setValue(value); }
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
        private void setValue(T v)
        {
            _value = v;
            hasChanged?.Invoke(this, new TEventArgs { value = _value });
        }
    }
}
