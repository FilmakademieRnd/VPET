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
//! @version 0
//! @date 16.08.2022

using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace vpet
{
    //!
    //! Implementation if VPET scene I/O
    //!
    public class SceneStorageModule : SceneManagerModule
    {
        public event EventHandler<EventArgs> sceneLoaded;
        private MenuTree m_menu;

        //!
        //! constructor
        //! @param name The name of this module.
        //! @param Manager The manager (SceneManager) of this module.
        //!
        public SceneStorageModule(string name, Manager manager) : base(name, manager) { }

        //!
        //! Init and setup of the module and it's UI.
        //!
        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);

            Parameter<Action> loadButton = new Parameter<Action>(LoadScene, "Load");
            Parameter<Action> saveButton = new Parameter<Action>(SaveScene, "Save");
            Parameter<Action> loadDemoButton = new Parameter<Action>(LoadDemoScene, "Load Demo");

            m_menu = new MenuTree()
              .Begin(MenuItem.IType.VSPLIT)
                   .Begin(MenuItem.IType.HSPLIT)
                       .Add("Scene name: ")
                       .Add(manager.settings.sceneFilepath)
                   .End()
                   .Begin(MenuItem.IType.HSPLIT)
                       .Add(loadButton)
                       .Add(saveButton)
                   .End()
                   .Begin(MenuItem.IType.HSPLIT)
                       .Add(loadDemoButton)
                   .End()
             .End();

            m_menu.caption = "Load/Save";
            m_menu.iconResourceLocation = "Images/button_save";
            UIManager uiManager = core.getManager<UIManager>();
            uiManager.addMenu(m_menu);

            // add elements to start menu
            uiManager.startMenu
                .Begin(MenuItem.IType.HSPLIT)
                    .Add(loadDemoButton)
                .End();

            //uiManager.showMenu(m_menu);
        }

        //!
        //! Function that determines the current scene filepath and calls the save function.
        //!
        private void SaveScene()
        {
            SaveScene(manager.settings.sceneFilepath.value);
            core.getManager<UIManager>().hideMenu();
        }

        //!
        //! Function that determines the current scene filepath and calls the load function.
        //!
        private void LoadScene()
        {
            LoadScene(manager.settings.sceneFilepath.value);
            core.getManager<UIManager>().hideMenu();
        }

        //!
        //! Function that parses the current scene, and stores it to the persistent data path under the given name.
        //!
        //! @param sceneName The name under which the scene files will be stored.
        //!
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

        //!
        //! Function that loads and creates the scene stored with the given from the persistent data path.
        //!
        //! @param sceneName The name of the scene to be loaded.
        //!
        public void LoadScene(string sceneName)
        {
            if (manager.sceneDataHandler != null)
            {
                string filepath = Path.Combine(Application.persistentDataPath, sceneName + ".header");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.headerByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName + ".nodes");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.nodesByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName + ".objects");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.objectsByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName + ".characters");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.characterByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName + ".textures");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.texturesByteData = File.ReadAllBytes(filepath);

                filepath = Path.Combine(Application.persistentDataPath, sceneName + ".materials");
                if (File.Exists(filepath))
                    manager.sceneDataHandler.materialsByteData = File.ReadAllBytes(filepath);

                sceneLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        //!
        //! Function that starts the coroutine to load and create the scene stored with the given from the persistent data path.
        //!
        //! @param sceneName The name of the scene to be loaded.
        //!
        public void LoadDemoScene()
        {
            if (manager.sceneDataHandler != null)
                core.StartCoroutine(LoadDemoCoroutine());
        }

        //!
        //! Coproutine that loads and creates the scene stored with the given from the persistent data path.
        //!
        private IEnumerator LoadDemoCoroutine()
        {
            Dialog statusDialog = new Dialog();
            UIManager UImanager = core.getManager<UIManager>();
            UImanager.showDialog(statusDialog);
            
            core.getManager<UIManager>().hideMenu();

            statusDialog.caption = "Load Header";
            yield return null;
            manager.sceneDataHandler.headerByteData = (Resources.Load("Storage/VPETDemoSceneHeader") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Load Scene Nodes";
            yield return null;
            manager.sceneDataHandler.nodesByteData = (Resources.Load("Storage/VPETDemoSceneNodes") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Load Scene Objects";
            yield return null;
            manager.sceneDataHandler.objectsByteData = (Resources.Load("Storage/VPETDemoSceneObjects") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Load Characters";
            yield return null;
            manager.sceneDataHandler.characterByteData = (Resources.Load("Storage/VPETDemoSceneCharacters") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Load Textures";
            yield return null;
            manager.sceneDataHandler.texturesByteData = (Resources.Load("Storage/VPETDemoSceneTextures") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Load Materials";
            yield return null;
            manager.sceneDataHandler.materialsByteData = (Resources.Load("Storage/VPETDemoSceneMaterials") as TextAsset).bytes;
            statusDialog.progress += 14;

            statusDialog.caption = "Build Scene";
            yield return null;
            sceneLoaded?.Invoke(this, EventArgs.Empty);
            statusDialog.progress += 14;

            UImanager.showDialog(null);   
        }

    }

}
