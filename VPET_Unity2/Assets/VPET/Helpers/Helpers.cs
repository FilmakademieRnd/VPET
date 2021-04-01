//! @file "AssemblyHelpers.cs"
//! @brief additional helper functions for assembly handling
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation class for assembly helpers
    //!
    public class Helpers
    {
        //!
        //! global id counter for generating unique sceneObject IDs
        //!
        private static int s_id = 0;

        //!
        //! provide a unique id
        //! @return     unique id as int
        //!
        public static int getUniqueID()
        {
            return s_id++;
        }

        //!
        //! Types for the debug message logs
        //!
        public enum logMsgType
        {
            NONE,
            WARNING,
            ERROR
        }
        //!
        //! Function for message loggin in VPET
        //! @param objName the name of the script or object sending the message
        //! @param msg the message to be logged
        //!
        public static void Log(string objName, string msg, logMsgType type = 0)
        {
            string log = "VPET: " + msg + " in " + objName;

            switch (type)
            {
                case logMsgType.WARNING:
                    Debug.LogWarning(log);
                    break;
                case logMsgType.ERROR:
                    Debug.LogError(log);
                    break;
                default:
                    Debug.Log(log);
                    break;
            }
        }

        //!
        //! searches and returns types in an assembly
        //! @param  appDomain   domain to be searched in for assemblies
        //! @param  type    type to be searched
        //! @return array of found types
        //!
        public static Type[] GetAllTypes(AppDomain appDomain, Type type)
        {
            var result = new List<Type>();
            var assemblies = appDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsSubclassOf(type))
                        result.Add(t);
                }
            }
            return result.ToArray();
        }
    }
}
