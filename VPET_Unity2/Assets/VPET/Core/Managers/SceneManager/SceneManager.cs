/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-------------------------------------------------------------------------------
*/

//! @file "SceneManager.cs"
//! @brief Scene Manager implementation.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace vpet
{
    //!
    //! class managing all scene related aspects
    //!
    public partial class SceneManager : Manager
    {
        [Serializable]
        public class SceneManagerSettings : Settings
        {
            //!
            //! Do we load scene from dump file
            //!
            public bool loadSampleScene = true;

            //!
            //! Do we load textures
            //!
            public bool loadTextures = true;

            //!
            //! global scale of the scene
            //!
            public float sceneScale = 1f;

            //!
            //! The filepath for loading and storing a scene.
            //!
            public Parameter<string> sceneFilepath;
        }

        //!
        //! Cast for accessing the settings variable with the correct type.
        //!
        public SceneManagerSettings settings { get => (SceneManagerSettings)_settings; }

        //!
        //! The maximum extend of the scene
        //!
        public Vector3 sceneBoundsMax = Vector3.positiveInfinity;
        public Vector3 sceneBoundsMin = Vector3.negativeInfinity;
        public float maxExtend = 1f;

        //!
        //! Function that returns a list containing all scene objects.
        //!
        //! @return The list containing all scene objects.
        //!
        public List<SceneObject> getAllSceneObjects()
        {
            List<SceneObject> returnvalue = new List<SceneObject>();

            foreach (Dictionary<short, ParameterObject> dict in core.parameterObjectList.Values)
            {
                foreach (ParameterObject parameterObject in dict.Values)
                {
                    SceneObject sceneObject = parameterObject as SceneObject;
                    if (sceneObject)
                        returnvalue.Add((SceneObject) parameterObject);
                }
            }
            return returnvalue;
        }
        //!
        //! The list storing selectable Unity lights in scene.
        //!
        private List<SceneObjectLight> m_sceneLightList = new List<SceneObjectLight>();
        //!
        //! Setter and getter to List holding references to all editable VPET sceneObjects.
        //!
        public List<SceneObjectLight> sceneLightList
        {
            get { return m_sceneLightList; }
            set { m_sceneLightList = value; }
        }

        //!
        //! The list storing Unity cameras in scene.
        //!
        private List<SceneObjectCamera> m_sceneCameraList = new List<SceneObjectCamera>();
        //!
        //! Setter and getter to List holding references to all editable VPET sceneObjects.
        //!
        public List<SceneObjectCamera> sceneCameraList
        {
            get { return m_sceneCameraList; }
            set { m_sceneCameraList = value; }
        }

        //!
        //! A reference to the VPET scene root.
        //!
        private GameObject m_scnRoot;

        //!
        //! The VPET SceneDataHandler, handling all VPET scene data relevant conversion.
        //!
        protected SceneDataHandler m_sceneDataHandler;
        
        //!
        //! Event emitted when scene is prepared.
        //!
        public event EventHandler<EventArgs> sceneReady;

        //!
        //! Event emitted when scene has been reseted.
        //!
        public event EventHandler<EventArgs> sceneReset;

        //!
        //! Getter returning a reference to the VPET scene root.
        //!
        //! @return A reference to the VPET scene root.
        //!
        public ref GameObject scnRoot
        {
            get { return ref m_scnRoot; }
        }

        //!
        //! A reference to the VPET SceneDataHandler.
        //!
        //! @return A reference to the VPET SceneDataHandler.
        //!
        public ref SceneDataHandler sceneDataHandler
        {
            get { return ref m_sceneDataHandler; }
        }

        //!
        //! constructor
        //! @param  name    Name of the scene manager
        //! @param  moduleType  Type of module to add to this manager 
        //!
        public SceneManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_sceneDataHandler = new SceneDataHandler();
            settings.sceneFilepath = new Parameter<string>("VPETdefaultScene", "Filepath");

            // create scene parent if not there
            scnRoot = GameObject.Find("Scene");
            if (scnRoot == null)
            {
                scnRoot = new GameObject("VPETScene");
            }
        }
   
        //!
        //! Function that emits the scene ready event. 
        //!
        public void emitSceneReady()
        {
            sceneReady?.Invoke(this, new EventArgs());
        }

        //!
        //! Function that returns a scne object based in the given ID.
        //!
        //! @param id The ID of the scene object to be returned.
        //! @return The corresponding scene object to the gevien ID.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SceneObject getSceneObject(byte sceneID, short poID)
        {
            return (SceneObject) core.getParameterObject(sceneID, poID);
        }

        //!
        //! Function that deletes all Unity scene content and clears the VPET scene object lists.
        //!
        public void ResetScene()
        {
            // remove all Unity GameObjects
            if (m_scnRoot != null)
            {
                foreach (Transform child in m_scnRoot.transform)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }

            // remove all Tracer SceneObjects
            foreach (SceneObject sceneObject in getAllSceneObjects())
                core.removeParameterObject(sceneObject);

            m_sceneCameraList.Clear();
            m_sceneLightList.Clear();


            sceneReset?.Invoke(this, EventArgs.Empty);
        }
    }
}