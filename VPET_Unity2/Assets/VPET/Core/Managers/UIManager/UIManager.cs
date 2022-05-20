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

//! @file "UIManager.cs"
//! @brief Implementation of the VPET UI Manager, managing creation of UI elements.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 21.01.2022

using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    public class UIManager : Manager
    {
        //!
        //! Class containing the uiManager settings.
        //! This is persistent data saved and loaded to/from disk.
        //!
        public class uiSettings : Settings
        {
            [ShowInMenu]
            public Parameter<float> uiScale = new Parameter<float>(1f, "uiScale");
        }
        //!
        //! Getter and setter for the UI settings.
        //!
        public uiSettings settings
        {
            get { return (uiSettings)_settings; }
            set { _settings = value; }
        }
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
        //! Event emitted when button list has been updated.
        //!
        public event EventHandler<EventArgs> buttonsUpdated;
        //!
        //! Event emitted when menu list has been updated.
        //!
        public event EventHandler<EventArgs> menusUpdated;
        //!
        //! Load global VPET color names and values.
        //!
        private static VPETUISettings m_uiAppearanceSettings;
        public VPETUISettings uiAppearanceSettings { get => m_uiAppearanceSettings; }
        //!
        //! Event emitted when a MenuTree has been selected.
        //!
        public event EventHandler<MenuTree> menuSelected;
        //!
        //! Event emitted when a MenuTree has been deselected.
        //!
        public event EventHandler<EventArgs> menuDeselected;
        //!
        //! A list storing references to the menus (MenuTrees) created by the UI-Modules.
        //!
        private List<MenuTree> m_menus;
        //!
        //! A list storing references to menu buttons created by the UI-Modules.
        //!
        private List<MenuButton> m_buttons;


        //!
        //! Constructor initializing member variables.
        //!
        public UIManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_selectedObjects = new List<SceneObject>();
            m_menus = new List<MenuTree>();
            m_buttons = new List<MenuButton>();
            m_uiAppearanceSettings = Resources.Load("DATA_VPET_Colors") as VPETUISettings;
        }

        //!
        //! function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);
            settings.uiScale.hasChanged -= updateCanvasScales;
        }

        //! 
        //! Virtual function called when Unity calls it's Start function.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            base.Init(sender, e);
            CreateSettingsMenu();
        }

        //!
        //! Unity's Start callback, used for Late initialization.
        //!
        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);
            updateCanvasScales(this,0f);
            settings.uiScale.hasChanged += updateCanvasScales;
        }

        //!
        //! update canvas scale for all canvases in the scene
        //!
        private void updateCanvasScales(object sender, float e)
        {
            CanvasScaler[] canvases = GameObject.FindObjectsOfType<CanvasScaler>();

            foreach (CanvasScaler canvas in canvases)
            {
                if(!canvas.gameObject.name.Contains("MenuCanvas"))
                    // 0.04f to have default uiScale at 1f
                    // Max /Min to prevent invalid UI
                    canvas.scaleFactor = Screen.dpi * 0.04f * Mathf.Min(Mathf.Max(settings.uiScale.value, 0.4f),3f);
            }
        }

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

            menu.id = m_menus.Count - 1;
            menusUpdated?.Invoke(this, EventArgs.Empty);
        }

        //!
        //! Adds a given button to the buttonlist.
        //!
        //! @param button The button to be added to the list.
        //!
        public void addButton(MenuButton button)
        {
            if (m_buttons.Contains(button))
                Helpers.Log("Button already existing in UIManager!", Helpers.logMsgType.WARNING);
            else
                m_buttons.Add(button);

            button.id = m_buttons.Count - 1;
            buttonsUpdated?.Invoke(this, EventArgs.Empty);
        }

        //!
        //! Removes a given button to the buttonlist.
        //!
        //! @param button The button to be removed from the list.
        //!
        public void removeButton(MenuButton button)
        {
            if (!m_buttons.Contains(button))
                Helpers.Log("Button not existing in UIManager!", Helpers.logMsgType.WARNING);
            else
                m_buttons.Remove(button);

            buttonsUpdated?.Invoke(this, EventArgs.Empty);
        }

        //!
        //! Returns a reference to to list of menus.
        //!
        public ref List<MenuTree> getMenus()
        {
            return ref m_menus;
        }

        //!
        //! Returns a reference to to list of menu buttons.
        //!
        public ref List<MenuButton> getButtons()
        {
            return ref m_buttons;
        }

        //!
        //! Function to create the main settings menu out of the core and manager settings.
        //!
        private void CreateSettingsMenu()
        {
            MenuTree settingsMenu = new MenuTree();
            settingsMenu.caption = "Settings";
            settingsMenu.setIcon("Images/button_gear");

            settingsMenu = settingsMenu.Begin(MenuItem.IType.VSPLIT); // <<< start VSPLIT

            // add core settings
            if (core.settings != null)
                createMenuTreefromSettings("Core", ref settingsMenu, core.settings);

            settingsMenu = settingsMenu.Add(MenuItem.IType.SPACE);

            // add all manager settings
            foreach (Manager manager in core.getManagers())
            {
                if (manager._settings != null)
                    createMenuTreefromSettings(manager.GetType().ToString().Split('.')[1], ref settingsMenu, manager._settings);
                
                settingsMenu = settingsMenu.Add(MenuItem.IType.SPACE);
            }

            settingsMenu.End();  // <<< end VSPLIT

            addMenu(settingsMenu);
        }

        //!
        //! Function to add menu elements to a MenuTree object based on a given VPET Settings object.
        //!
        //! @ param caption A headline created defining the new section in the menu.
        //! @ param menu A reference to the menue to be extended.
        //! @ param settings The settings to be added to the menu.
        //!
        private void createMenuTreefromSettings(string caption, ref MenuTree menu, Settings settings)
        {
            menu = menu.Add(caption, true);
            menu = menu.Add(MenuItem.IType.SPACE);

            Type type = settings.GetType();
            FieldInfo[] infos = type.GetFields();

            foreach (FieldInfo info in infos)
            {
                object o = info.GetValue(settings);
                Attribute a = info.GetCustomAttribute(typeof(ShowInMenu));

                menu = menu.Begin(MenuItem.IType.HSPLIT);  // <<< start HSPLIT

                if (o.GetType().BaseType == typeof(AbstractParameter) && (a != null))
                {
                    menu = menu.Add(info.Name);
                    menu = menu.Add((AbstractParameter)info.GetValue(settings));
                }
                else if ( info.GetValue(settings).GetType().BaseType != typeof(AbstractParameter))
                {
                    menu = menu.Add(info.Name);
                    menu = menu.Add(info.GetValue(settings).ToString());
                }

                menu.End();  // <<< end HSPLIT
            }
        }

        //!
        //! Function that invokes the highlightLocked Event.
        //!
        //! @pram sceneObject The scene object to be highlighted as a payload for the event.
        //!
        public void highlightSceneObject(SceneObject sceneObject)
        {
            highlightLocked?.Invoke(this, sceneObject);
        }

        //!
        //! Function that invokes the unhighlightLocked Event.
        //!
        //! @pram sceneObject The scene object to be un-highlighted as a payload for the event.
        //!
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
            foreach (SceneObject sceneObject in m_selectedObjects)
                selectionRemoved?.Invoke(this, sceneObject);

            m_selectedObjects.Clear();
            selectionChanged?.Invoke(this, m_selectedObjects);
        }

        //!
        //! Function that invokes the signal for creating the UI for the given menu.
        //!
        public void showMenu(MenuTree menu)
        {
            menuSelected?.Invoke(this, menu);
        }

        //!
        //! Function that invokes the signal for deleting the active menu UI.
        //!
        public void hideMenu()
        {
            menuDeselected?.Invoke(this, EventArgs.Empty);
        }
    }
}