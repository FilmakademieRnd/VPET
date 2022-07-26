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

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


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
        //! The list storing editable VPET scene objects in scene.
        //!
        private List<SceneObject> m_sceneObjects = new List<SceneObject>();
        //!
        //! Setter and getter to List holding references to all editable VPET sceneObjects.
        //!
        public List<SceneObject> sceneObjects
        {
            get { return m_sceneObjects; }
            set { m_sceneObjects = value; }
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
        public SceneObject getSceneObject(int id)
        {
            if (id < 1)
                return null;
            else
                return sceneObjects[id -1];
        }

        //!
        //! Function that returns a id based in the given scne object.
        //!
        //! @param scneObject The scne object of which the ID will be returned. 
        //! @return The corresponding ID to the gevien scene object.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getSceneObjectId(ref SceneObject sceneObject)
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