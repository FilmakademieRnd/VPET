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
        //! reference to vpet core.
        //!
        private Core m_core;

        //!
        //! get the vpet core
        //!
        public ref Core core
        {
            get => ref m_core;
        }

        //!
        //! dictionary of loaded modules
        //!
        private Dictionary<Type, Module> m_modules;

        //!
        //! constructor
        //! @param  name    name of the manager
        //! @param  moduleType  type of modules to be loaded by this manager
        //!
        public Manager(Type moduleType, Core vpetCore)
        {
            m_modules = new Dictionary<Type, Module>();
            m_core = vpetCore;
            Type[] modules = Helpers.GetAllTypes(AppDomain.CurrentDomain, moduleType);
            foreach (Type t in modules)
            {
                Module module = (Module)Activator.CreateInstance(t, t.ToString(), this);
                addModule(module, t);
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
            if (m_modules.ContainsKey(type))
                return false;
            else
            {
                m_modules.Add(type, module);
                return true;
            }
        }

        //!
        //! get a module from the manager
        //! @tparam T The type of module to be requested.
        //! @return requested module or null if no module of this type is registered.
        //!
        public T getModule<T>()
        {
            Module module;
            if (!m_modules.TryGetValue(typeof(T), out module))
                Helpers.Log(this.GetType().ToString() + " no module of type " + typeof(T).ToString() + " registered.", Helpers.logMsgType.ERROR);
            return (T)(object) module;
        }

        //!
        //! remove a module from the manager
        //! @param  name    name of module
        //! @return returns false if module does not exist, otherwise true
        //!
        protected bool removeModule(Type type)
        {
            return m_modules.Remove(type);
        }
    }
}