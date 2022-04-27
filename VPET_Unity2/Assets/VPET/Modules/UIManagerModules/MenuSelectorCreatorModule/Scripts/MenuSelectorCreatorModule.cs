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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        protected override void Start(object sender, EventArgs e)
        {
            // [REVIEW]
            // pls remove, only for testing...
            //>>>>>>>>>>>
            MenuButton test = new MenuButton("TestButton", testAction);
            test.setIcon("Images/button_frame_BG");
            //manager.addButton(test);
            //<<<<<<<<<<<

            GameObject canvasRes = Resources.Load("Prefabs/MenuSelectorCanvas") as GameObject;
            m_menuSelectorPrefab = Resources.Load("Prefabs/MenuSelectorPrefab") as GameObject;
            m_buttonSelectorPrefab = Resources.Load("Prefabs/ButtonSelectorPrefab") as GameObject;

            m_canvas = GameObject.Instantiate(canvasRes);
            m_canvas.GetComponent<Canvas>().sortingOrder = 10;
            m_canvas.GetComponent<CanvasScaler>().scaleFactor = Screen.dpi * core.settings.uiScale.value;

            manager.menuSelected += highlightMenuElement;

            createMenus(this, EventArgs.Empty);
            createButtons(this, EventArgs.Empty);
            
            
            manager.buttonsUpdated += createButtons;
            manager.menusUpdated += createMenus;
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

            manager.menuSelected -= highlightMenuElement;
            m_menuSelector.elementClicked -= menuClicked;
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
                GameObject.Destroy(m_menuSelector.gameObject);
            }

            m_menuSelector = GameObject.Instantiate(m_menuSelectorPrefab, m_canvas.transform).GetComponent<SnapSelect>();
            m_menuSelector.uiSettings = manager.uiSettings;

            foreach (MenuTree menu in manager.getMenus())
            {
                if (menu.iconResourceLocation.Length > 0)
                {
                    Sprite resImage = Resources.Load<Sprite>(menu.iconResourceLocation);
                    if (resImage != null)
                        m_menuSelector.addElement(/*menu.caption,*/ resImage);

                }
                else if (menu.caption.Length > 0)
                    m_menuSelector.addElement(menu.caption);
                else
                {
                    Helpers.Log("Menu has no caption and Icon!", Helpers.logMsgType.WARNING);
                    m_menuSelector.addElement("EMPTY");
                }
            }
            m_menuSelector.elementClicked += menuClicked;
        }

        //!
        //! Function that creates the button UI elements based on the button items stored in the UI manager.
        //!
        private void createButtons(object sender, EventArgs e)
        {
            if (m_buttonSelector != null)
                GameObject.Destroy(m_buttonSelector.gameObject);

            m_buttonSelector = GameObject.Instantiate(m_buttonSelectorPrefab, m_canvas.transform).GetComponent<SnapSelect>();
            m_buttonSelector.uiSettings = manager.uiSettings;

            foreach (MenuButton button in manager.getButtons())
            {
                if (button.iconResourceLocation.Length > 0)
                {
                    Sprite resImage = Resources.Load<Sprite>(button.iconResourceLocation);
                    if (resImage != null)
                        m_buttonSelector.addElement(button.caption, resImage, 0, button.action);

                }
                else if (button.caption.Length > 0)
                    m_buttonSelector.addElement(button.caption, 0, button.action);
                else
                {
                    Helpers.Log("Button has no caption and Icon!", Helpers.logMsgType.WARNING);
                    m_buttonSelector.addElement("EMPTY", 0, button.action);
                }
            }
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

        //!
        //! Function called when the UI manager selects a menu.
        //!
        //! @param sender The UI manager triggering this function.
        //! @param t The selected menu.
        //!
        private void highlightMenuElement(object sender, MenuTree t)
        {
            if (t == null)
                m_menuSelector.showHighlighted(-1);
            else
                m_menuSelector.showHighlighted(t.id);
        }

        //! [REVIEW]
        //! Just for testing, pls remove!!
        public void testAction()
        {
            Debug.Log("Test Action!");
        }
    }

}