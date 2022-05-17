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

//! @file "SceneStorageModule.cs"
//! @brief implementation of VPET scene I/O
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace vpet
{
    //!
    //! implementation VPET scene I/O
    //!
    public class SceneStorageModule : SceneManagerModule
    {
        public event EventHandler<EventArgs> sceneLoaded;
        //!
        //! constructor
        //! @param   name    Name of this module
        //!
        public SceneStorageModule(string name, Manager manager) : base(name, manager) { }

        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);

            MenuButton saveButton = new MenuButton("Save Scene", SaveScene);
            MenuButton loadButton = new MenuButton("Load Scene", LoadScene);

            core.getManager<UIManager>().addButton(saveButton);
            core.getManager<UIManager>().addButton(loadButton);
        }

        private void SaveScene()
        {
            SaveScene("VPET_default_Scene");
        }

        private void LoadScene()
        {
            LoadScene("VPET_default_Scene");
        }

        public void SaveScene(string sceneName)
        {
            SceneParserModule sceneParserModule = manager.getModule<SceneParserModule>();
            if (sceneParserModule != null)
            {
                sceneParserModule.ParseScene(true, false, true, false);

                if (manager.sceneDataHandler.headerByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".header"), manager.sceneDataHandler.headerByteDataRef);
                if (manager.sceneDataHandler.nodesByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".nodes"), manager.sceneDataHandler.nodesByteDataRef);
                if (manager.sceneDataHandler.objectsByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".objects"), manager.sceneDataHandler.objectsByteDataRef);
                if (manager.sceneDataHandler.characterByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".characters"), manager.sceneDataHandler.characterByteDataRef);
                if (manager.sceneDataHandler.texturesByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".textures"), manager.sceneDataHandler.texturesByteDataRef);
                if (manager.sceneDataHandler.materialsByteDataRef != null)
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, sceneName + ".materials"), manager.sceneDataHandler.materialsByteDataRef);

                Helpers.Log("Scene saved to " + Application.persistentDataPath);

                manager.sceneDataHandler.clearSceneByteData();
            }
        }

        public void LoadScene(string sceneName)
        {
            if (manager.sceneDataHandler != null)
            {
                string filepath = Path.Combine(Application.persistentDataPath, sceneName +".header");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.headerByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName +".nodes");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.nodesByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName +".objects");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.objectsByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName +".characters");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.characterByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName +".textures");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.texturesByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName +".materials");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.materialsByteData = File.ReadAllBytes(filepath);

                sceneLoaded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
