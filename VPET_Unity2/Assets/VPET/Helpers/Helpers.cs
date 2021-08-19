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

//! @file "SceneDataDefinition.cs"
//! @brief definition of VPET helpers class.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 26.04.2021

using System.Collections.Generic;
using System;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation for VPET helpers.
    //!
    public static class Helpers
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
        public static void Log(string msg, logMsgType type = 0)
        {
            string log = "VPET: " + msg;

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

        //!
        //! Searches and returns a child transform in a tree of transforms by name
        //!
        //! @param aParent The Transform of the parent game object.
        //! @param name The name of the child to be searched for.
        //! @return Retuens the transform of the child if exist, null otherwise. 
        //!
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}
