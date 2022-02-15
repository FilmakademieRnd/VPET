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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "UIManager.cs"
//! @brief Implementation of the VPET UI Manager, managing creation of UI elements.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 21.01.2022

using System.Collections.Generic;
using System;
using UnityEngine;

namespace vpet
{
    public class UIManager : Manager
    {
        //!
        //! The list containing currently selected scene objects.
        //!
        private List<SceneObject> m_selectedObjects;

        //!
        //! Event emitted when the scene selection has changed.
        //!
        public event EventHandler<List<SceneObject>> selectionChanged;
        //!
        //! Event emitted when a sceneObject has been added to a selection.
        //!
        public event EventHandler<SceneObject> selectionAdded;
        //!
        //! Event emitted when a sceneObject has been removed from a selection.
        //!
        public event EventHandler<SceneObject> selectionRemoved;
        //!
        //! Event emitted to highlight a scene object.
        //!
        public event EventHandler<SceneObject> highlightLocked;
        //!
        //! Event emitted to unhighlight a scene object.
        //!
        public event EventHandler<SceneObject> unhighlightLocked;
        //!
        //! Event emitted when a MenuTree has been selected.
        //!
        public event EventHandler<MenuTree> menuSelected;

        //!
        //! A list storing references to the menus (MenuTrees) created by the UI-Modules.
        //!
        private List<MenuTree> m_menus;

        //!
        //! Adds a given menu to the menulist.
        //!
        //! @param menu The menue to be added to the list.
        //!
        public void addMenu(MenuTree menu)
        {
            if (m_menus.Contains(menu))
                Helpers.Log("Menu already existing in UIManager!", Helpers.logMsgType.WARNING);
            else
                m_menus.Add(menu);
        }

        //!
        //! Returns a reference to to list of menus.
        //!
        public ref List<MenuTree> getMenus()
        {
            return ref m_menus;
        }

        //!
        //! Constructor initializing member variables.
        //!
        public UIManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_selectedObjects = new List<SceneObject>();
            m_menus = new List<MenuTree>();
        }

        public void highlightSceneObject(SceneObject sceneObject)
        {
            highlightLocked?.Invoke(this, sceneObject);
        }

        public void unhighlightSceneObject(SceneObject sceneObject)
        {
            unhighlightLocked?.Invoke(this, sceneObject);
        }

        //!
        //! Function that adds a sceneObject to the selected objects list.
        //!
        //! @ param sceneObject The selected scene object to be added.
        //!
        public void addSelectedObject(SceneObject sceneObject)
        {
            if (!sceneObject._lock)
            {
                m_selectedObjects.Add(sceneObject);

                selectionChanged?.Invoke(this, m_selectedObjects);
                selectionAdded?.Invoke(this, sceneObject);
            }
        }

        //!
        //! Function that removes a sceneObject to the selected objects list.
        //!
        //! @ param sceneObject The selected scene object to be removed.
        //!
        public void removeSelectedObject(SceneObject sceneObject)
        {
            m_selectedObjects.Remove(sceneObject);

            selectionChanged?.Invoke(this, m_selectedObjects);
            selectionRemoved?.Invoke(this, sceneObject);
        }

        //!
        //! Function that clears the selected objects list.
        //!
        public void clearSelectedObject()
        {
            foreach(SceneObject sceneObject in m_selectedObjects)
                selectionRemoved?.Invoke(this, sceneObject);

            m_selectedObjects.Clear();
            selectionChanged?.Invoke(this, m_selectedObjects);
        }

        public void showMenu(MenuTree menu)
        {
            menuSelected?.Invoke(this, menu);
        }
    }
}