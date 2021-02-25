//! @file "ManagerInterface.cs"
//! @brief base vpet manager interface
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using System;

namespace vpet
{
    //!
    //! manager class interface definition
    //!
    interface ManagerInterface
    {

    }

    //!
    //! manager class implementation
    //!
    public class Manager : ManagerInterface
    {
        //!
        //! reference to vpet uberManager
        //!
        private CoreInterface _core;

        //!
        //! get the vpet core
        //!
        public CoreInterface core
        {
            get => _core;

        }

        //!
        //! dictionary of loaded modules
        //!
        private Dictionary<Type, Module> modules;

        //!
        //! constructor
        //! @param  name    name of the manager
        //! @param  moduleType  type of modules to be loaded by this manager
        //!
        public Manager(Type moduleType, CoreInterface vpetCore)
        {
            _core = vpetCore;
            Type[] modules = Helpers.GetAllTypes(AppDomain.CurrentDomain, moduleType);
            foreach (Type t in modules)
            {
                Module module = (Module)Activator.CreateInstance(t);
                addModule(module, moduleType);
            }
        }

        //!
        //! adds a module to the manager
        //! @param  module  module to be added
        //! @param  name    name of module
        //! @return returns false if module with same name already exists, otherwise true
        //!
        protected bool addModule(Module module, Type type)
        {
            if (modules.ContainsKey(type))
                return false;
            else
            {
                modules.Add(type, module);
                module.manager = this;
                return true;
            }
        }

        //!
        //! get a module from the manager
        //! @param  name    name of module
        //! @return requested module or null
        //!
        public Module getModule(Type type)
        {
            Module module;
            modules.TryGetValue(type, out module);
            return module;
        }

        //!
        //! remove a module from the manager
        //! @param  name    name of module
        //! @return returns false if module does not exist, otherwise true
        //!
        protected bool removeModule(Type type)
        {
            return modules.Remove(type);
        }
    }
}