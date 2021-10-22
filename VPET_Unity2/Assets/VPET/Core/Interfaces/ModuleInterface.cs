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


//! @file "ModuleInterface.cs"
//! @brief The base implementation of the VPET module interface.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 19.08.2021

using System;

namespace vpet
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
    public class Module : ModuleInterface
    {
        //!
        //! name of the module
        //!
        protected string m_name;

        //!
        //! manager of this module
        //! assigned in addModule function in Manager.
        //!
        protected Core m_core;

        //!
        //! Flad determin whether a module is loaded or not.
        //!
        public bool load = true;

        //!
        //! constructor
        //! @param  name    name of the module.
        //!
        public Module(string name, Core core)
        {
            m_name = name;
            m_core = core;

            core.awakeEvent += Init;
            core.destroyEvent += Cleanup;
        }

        //!
        //! Get the name of the module.
        //! @return name of the module.
        //!
        public string name
        {
            get => m_name;
        }

        //! 
        //! Virtual function called when Unity initializes the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected virtual void Init(object sender, EventArgs e) { }
        //! 
        //! Virtual function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected virtual void Cleanup(object sender, EventArgs e) { }
    }
}
