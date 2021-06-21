//! @file "ModuleInterface.cs"
//! @brief base implementation of vpet modules
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;

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
        //! constructor
        //! @param  name    name of the module.
        //!
        public Module(string name, Core core)
        {
            m_name = name;
            m_core = core;
        }

        //!
        //! get the name of the module.
        //! @return name of the module.
        //!
        public string name
        {
            get => m_name;
        }
    }
}
