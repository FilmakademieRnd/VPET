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
        //! name of the manager
        //!
        protected string name;

        //!
        //! dictionary of loaded modules
        //!
        private Dictionary<string, Module> modules;

        //!
        //! constructor
        //! @param  name    name of the manager
        //! @param  moduleType  type of modules to be loaded by this manager
        //!
        public Manager(string name, Type moduleType)
        {
            this.name = name;
            Type[] modules = helpers.AssemblyHelpers.GetAllTypes(AppDomain.CurrentDomain, moduleType);
            foreach (Type t in modules)
            {
                Module module = (Module)Activator.CreateInstance(t);
                addModule(module, module.name);
            }
        }

        //!
        //! adds a module to the manager
        //! @param  module  module to be added
        //! @param  name    name of module
        //! @return returns false if module with same name already exists, otherwise true
        //!
        protected bool addModule(Module module, string name)
        {
            if (modules.ContainsKey(name))
                return false;
            else
            {
                modules.Add(name, module);
                return true;
            }
        }

        //!
        //! get a module from the manager
        //! @param  name    name of module
        //! @return requested module or null
        //!
        protected Module getModule(string name)
        {
            Module module;
            modules.TryGetValue(name, out module);
            return module;
        }

        //!
        //! remove a module from the manager
        //! @param  name    name of module
        //! @return returns false if module does not exist, otherwise true
        //!
        protected bool removeModule(string name)
        {
            return modules.Remove(name);
        }
    }
}