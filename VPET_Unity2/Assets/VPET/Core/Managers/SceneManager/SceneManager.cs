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

//! @file "SceneManager.cs"
//! @brief Scene Manager implementation.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
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
        private GameObject m_scnRoot;

        public ref GameObject scnRoot
        {
            get { return ref m_scnRoot; }
        }


        //! The list storing editable VPET scene objects in scene.
        private List<SceneObject> m_sceneObjects = new List<SceneObject>();

        //! The list storing selectable Unity lights in scene.
        public List<SceneObjectLight> m_sceneLightList = new List<SceneObjectLight>();

        //! The list storing Unity cameras in scene.
        public List<SceneObjectCamera> m_sceneCameraList = new List<SceneObjectCamera>();

        //!
        //! Event emitted when scene is prepared.
        //!
        public event EventHandler<EventArgs> sceneReady;

        public void emitSceneReady()
        {
            sceneReady?.Invoke(this, new EventArgs());
        }

        //!
        //! Setter and getter to List holding references to all editable VPET sceneObjects.
        //!
        public List<SceneObject> sceneObjects
        {
            get { return m_sceneObjects; }
            set { m_sceneObjects = value; }
        }
        //!
        //! The VPET SceneDataHandler, handling all VPET scene data relevant conversion.
        //!
        protected SceneDataHandler m_sceneDataHandler;
        //!
        //! A reference to the VPET SceneDataHandler.
        //!
        //! @return A reference to the VPET SceneDataHandler.
        //!
        public ref SceneDataHandler sceneDataHandler
        {
            get { return ref m_sceneDataHandler; }
        }
        
        public static class Settings
        {
            //!
            //! Do we load scene from dump file
            //!
            public static bool loadSampleScene = true;

            //!
            //! Do we load textures
            //!
            public static bool loadTextures = true;

            //!
            //! The maximum extend of the scene
            //!
            public static Vector3 sceneBoundsMax = Vector3.positiveInfinity;
            public static Vector3 sceneBoundsMin = Vector3.negativeInfinity;
            public static float maxExtend = 1f;

            //!
            //! Light Intensity multiplicator
            //!
            public static float lightIntensityFactor = 1f;

            //!
            //! global scale of the scene
            //!
            public static float sceneScale = 1f;

        }

        //!
        //! constructor
        //! @param  name    Name of the scene manager
        //! @param  moduleType  Type of module to add to this manager 
        //!
        public SceneManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_sceneDataHandler = new SceneDataHandler();

            // create scene parent if not there
            scnRoot = GameObject.Find("Scene");
            if (scnRoot == null)
            {
                scnRoot = new GameObject("VPETScene");
            }
        }


        public SceneObject getSceneObject(int id)
        {
            if (id < 1)
                return null;
            else
                return sceneObjects[id -1];
        }

        public int getSceneObjectId(SceneObject sceneObject)
        {
            return sceneObjects.IndexOf(sceneObject) + 1;  // +1 because 0 is non selectable object or background
        }

        //!
        //! Function that deletes all Unity scene content and clears the VPET scene object lists.
        //!
        public void ResetScene()
        {
            m_sceneObjects.Clear();
            m_sceneCameraList.Clear();
            m_sceneLightList.Clear();
            
            if (m_scnRoot != null)
            {
                foreach (Transform child in m_scnRoot.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }
}