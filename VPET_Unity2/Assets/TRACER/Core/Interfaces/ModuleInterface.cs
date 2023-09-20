/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
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


//! @file "ModuleInterface.cs"
//! @brief The base implementation of the TRACER module interface.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 03.02.2022

using System;

namespace tracer
{
    //!
    //! module interface definition
    //!
    interface ModuleInterface 
    {

    }

    //!
    //! module interface implementation
    //!
    public class Module : ModuleInterface, IDisposable
    {
        //!
        //! name of the module
        //!
        protected string m_name;

        //!
        //! manager of this module
        //! assigned in addModule function in Manager.
        //!
        protected Manager m_manager;

        //!
        //! Returns a reference to the TRACER core.
        //!
        protected ref Core core { get => ref m_manager.core; }

        //!
        //! Flad determin whether a module is loaded or not.
        //!
        public bool load = true;

        //!
        //! constructor
        //! @param  name name of the module.
        //!
        public Module(string name, Manager manager)
        {
            m_name = name;
            m_manager = manager;

            m_manager.initEvent += Init;
            m_manager.startEvent += Start;
            m_manager.cleanupEvent += Cleanup;
        }

        public virtual void Dispose()
        {
            m_manager.initEvent -= Init;
            m_manager.startEvent -= Start;
            m_manager.cleanupEvent -= Cleanup;
        }

        //!
        //! Get the name of the module.
        //! @return name of the module.
        //!
        public ref string name { get => ref m_name; }

        //! 
        //! Virtual function called when Unity initializes the TRACER core.
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected virtual void Init(object sender, EventArgs e) { }
        //! 
        //! Virtual function called after the Init function.
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected virtual void Start(object sender, EventArgs e) { }
        //! 
        //! Virtual function called before Unity destroys the TRACER core.
        //! 
        //! @param sender A reference to the TRACER core.
        //! @param e Arguments for these event. 
        //! 
        protected virtual void Cleanup(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
