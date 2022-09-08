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

//! @file "MenuSelectorCreatorModule.cs"
//! @brief Implementation of the VPET MenuSelectorCreatorModule, creating menu items in the UI.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 11.03.2022

using System;
using UnityEngine;

namespace vpet
{
    public class MenuSelectorCreatorModule : UIManagerModule
    {
        //!
        //! The UI canvas for the menu and button items.
        //!
        GameObject m_canvas;
        //!
        //! The UI item implemetation for the menu buttons.
        //!
        SnapSelect m_menuSelector;
        //!
        //! The UI item implemetation for the action buttons.
        //!
        SnapSelect m_buttonSelector;
        //!
        //! The UI prefab for the menu buttons.
        //!
        GameObject m_menuSelectorPrefab;
        //!
        //! The UI prefab for the action buttons.
        //!
        GameObject m_buttonSelectorPrefab;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public MenuSelectorCreatorModule(string name, Manager manager) : base(name, manager)
        {
        }

        //!
        //! Function that creates a menu containing buttons for every registered menu.
        //!
        //! @param sender A reference to the VPET core.
        //! @param e The event arguments for the start event.
        //!
        protected override void Init(object sender, EventArgs e)
        {
            GameObject canvasRes = Resources.Load("Prefabs/MenuSelectorCanvas") as GameObject;
            m_menuSelectorPrefab = Resources.Load("Prefabs/MenuSelectorPrefab") as GameObject;
            m_buttonSelectorPrefab = Resources.Load("Prefabs/ButtonSelectorPrefab") as GameObject;

            m_canvas = GameObject.Instantiate(canvasRes);
            m_canvas.GetComponent<Canvas>().sortingOrder = 10;

            createMenus(this, EventArgs.Empty);
            createButtons(this, EventArgs.Empty);
            
            manager.buttonsUpdated += createButtons;
            manager.menusUpdated += createMenus;
        }

        //! 
        //! Function called when an Unity Start() callback is triggered
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
        }

        //! 
        //! Function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            m_menuSelector.elementClicked -= menuClicked;
            manager.menuDeselected -= deselectMenu;
            manager.buttonsUpdated -= createButtons;
            manager.menusUpdated -= createMenus;

        }

        //!
        //! Function that creates the menu UI elements based on the MenuTree items stored in the UI manager.
        //!
        private void createMenus(object sender, EventArgs e)
        {
            if (m_menuSelector != null)
            {
                m_menuSelector.elementClicked -= menuClicked;
                manager.menuDeselected -= deselectMenu;
                GameObject.Destroy(m_menuSelector.gameObject);
            }

            m_menuSelector = GameObject.Instantiate(m_menuSelectorPrefab, m_canvas.transform.GetChild(0)).GetComponent<SnapSelect>();
            m_menuSelector.uiSettings = manager.uiAppearanceSettings;
            m_menuSelector.manager = manager;
            core.getManager<UIManager>().menuDeselected += m_menuSelector.resetHighlighting;

            foreach (MenuTree menu in manager.getMenus())
            {
                if (menu.roles.Count == 0 ||
                    manager.activeRole == UIManager.Roles.EXPERT ||
                    menu.roles.Contains(manager.activeRole))
                {
                    if (menu.iconResourceLocation.Length > 0)
                    {
                        Sprite resImage = Resources.Load<Sprite>(menu.iconResourceLocation);
                        if (resImage != null)
                            m_menuSelector.addElement(/*menu.caption,*/ resImage, menu.id);
                    }
                    else if (menu.caption.Length > 0)
                        m_menuSelector.addElement(menu.caption, menu.id);
                    else
                    {
                        Helpers.Log("Menu has no caption and Icon!", Helpers.logMsgType.WARNING);
                        m_menuSelector.addElement("EMPTY", menu.id);
                    }
                }
            }

            m_menuSelector.elementClicked += menuClicked;
            manager.menuDeselected += deselectMenu;
        }

        //!
        //!
        //!
        private void deselectMenu(object sender, EventArgs e)
        {
            m_menuSelector.showHighlighted(-1);
        }


        //!
        //! Function that creates the button UI elements based on the button items stored in the UI manager.
        //!
        private void createButtons(object sender, EventArgs e)
        {
            foreach (MenuButton button in manager.getButtons())
                button.m_highlightEvent -= m_menuSelector.updateHighlight;

            if (m_buttonSelector != null)
                GameObject.Destroy(m_buttonSelector.gameObject);

            m_buttonSelector = GameObject.Instantiate(m_buttonSelectorPrefab, m_canvas.transform.GetChild(0)).GetComponent<SnapSelect>();
            m_buttonSelector.uiSettings = manager.uiAppearanceSettings;
            m_buttonSelector.manager = manager;

            foreach (MenuButton button in manager.getButtons())
            {
                if (button.roles.Count == 0 ||
                    manager.activeRole == UIManager.Roles.EXPERT ||
                    button.roles.Contains(manager.activeRole))
                {
                    if (button.iconResourceLocation.Length > 0)
                    {
                        Sprite resImage = Resources.Load<Sprite>(button.iconResourceLocation);
                        if (resImage != null)
                        {
                            m_buttonSelector.addElement(button.caption, resImage, 0, button.action, button.isToggle, button.id);
                            button.m_highlightEvent += m_buttonSelector.updateHighlight;
                            MenuButton.HighlightEventArgs args = new MenuButton.HighlightEventArgs();
                            args.id = button.id;
                            args.highlight = button.isHighlighted;
                            m_buttonSelector.updateHighlight(this, args);
                        }

                    }
                    else if (button.caption.Length > 0)
                    {
                        m_buttonSelector.addElement(button.caption, 0, button.action, button.isToggle, button.id);
                        button.m_highlightEvent += m_buttonSelector.updateHighlight;
                    }
                    else
                    {
                        Helpers.Log("Button has no caption and Icon!", Helpers.logMsgType.WARNING);
                        m_buttonSelector.addElement("EMPTY", 0, button.action, button.isToggle, button.id);
                        button.m_highlightEvent += m_buttonSelector.updateHighlight;
                    }
                }
            }

            m_buttonSelector.elementClicked += buttonClicked;

        }

        //!
        //! Function called when a button has clicked.
        //!
        //! @param sender The snapSelect triggering this function.
        //! @param id The snapSelect internal id for the corresponding button.
        //!
        private void buttonClicked(object sender, int id)
        {
            MenuButton b = manager.getButtons()[id];
            if (!b.isToggle)
                b.isHighlighted = !b.isHighlighted;
        }

        //!
        //! Function called when a menu button has clicked. Informs the UI manager to show the given MenuTree. 
        //!
        //! @param sender The snapSelect triggering this function.
        //! @param id The snapSelect internal id for the corresponding menu.
        //!
        private void menuClicked(object sender, int id)
        {
            manager.showMenu((MenuTree)manager.getMenus()[id]);
        }

    }

}