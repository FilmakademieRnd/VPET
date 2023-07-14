/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "ParameterObject.cs"
//! @brief Implementation of the VPET ParameterObject, collecting parameters and providing parameter update functionalities.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET ParameterObject, collecting parameters and providing parameter update functionalities.
    //!
    [System.Serializable]
    public class ParameterObject : MonoBehaviour
    {
        //!
        //! The global id counter for generating unique parameterObject IDs.
        //!
        private static short s_id = 1;
        //!
        //! The unique ID of this parameter object.
        //!
        protected short _id;
        //!
        //! The unique ID of this parameter object.
        //!
        public short id
        {
            get => _id;
        }
        //!
        //! A reference to the vpet core.
        //!
        static protected Core _core = null;
        public static Core core
        {
            get => _core;
        }
        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<AbstractParameter> hasChanged;
        //!
        //! List storing all parameters of this SceneObject.
        //!
        private List<AbstractParameter> _parameterList;
        //!
        //! Getter for parameter list
        //!
        public ref List<AbstractParameter> parameterList
        {
            get => ref _parameterList;
        }
        //!
        //! Function that emits the parameter objects hasChanged event. (Used for parameter updates)
        //!
        //! @param parameter The parameter that has changed. 
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void emitHasChanged(AbstractParameter parameter)
        {
            if (parameter._distribute)
                hasChanged?.Invoke(this, parameter);
        }
        //!
        //! Function that searches and returns a parameter of this parameter object based on a given name.
        //!
        //! @param name The name of the parameter to be returned.
        //!
        public Parameter<T> getParameter<T>(string name)
        {
            return (Parameter<T>)_parameterList.Find(parameter => parameter.name == name);
        }
        //!
        //! Initialisation
        //!
        public virtual void Awake()
        {
            _parameterList = new List<AbstractParameter>();
            _id = getSoID();

            if (_core == null)
                _core = GameObject.FindObjectOfType<Core>();

            _core.addParameterObject(this);
        }
        //!
        //! provide a unique id
        //! @return     unique id as int
        //!
        private static short getSoID()
        {
            return s_id++;
        }
    }
}
