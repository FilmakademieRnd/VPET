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
        SnapSelect m_menuSelector;
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
            manager.addButton(test);
            //<<<<<<<<<<<

            GameObject canvasRes = Resources.Load("Prefabs/MenuSelectorCanvas") as GameObject;
            
            GameObject menuSelectorPrefab = Resources.Load("Prefabs/MenuSelectorPrefab") as GameObject;
            GameObject buttonSelectorPrefab = Resources.Load("Prefabs/ButtonSelectorPrefab") as GameObject;

            GameObject canvas = GameObject.Instantiate(canvasRes);
            canvas.GetComponent<Canvas>().sortingOrder = 10;
            m_menuSelector = GameObject.Instantiate(menuSelectorPrefab, canvas.transform).GetComponent<SnapSelect>();
            m_menuSelector.uiSettings = manager.uiSettings;
            SnapSelect buttonSelector = GameObject.Instantiate(buttonSelectorPrefab, canvas.transform).GetComponent<SnapSelect>();
            buttonSelector.uiSettings = manager.uiSettings;


            manager.menuSelected += highlightMenuElement;

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

            foreach (MenuButton button in manager.getButtons())
            {
                if (button.iconResourceLocation.Length > 0)
                {
                    Sprite resImage = Resources.Load<Sprite>(button.iconResourceLocation);
                    if (resImage != null)
                        buttonSelector.addElement(button.caption, resImage, 0, button.action);

                }
                else if (button.caption.Length > 0)
                    buttonSelector.addElement(button.caption, 0, button.action);
                else
                {
                    Helpers.Log("Button has no caption and Icon!", Helpers.logMsgType.WARNING);
                    buttonSelector.addElement("EMPTY", 0, button.action);
                }
            }
            m_menuSelector.elementClicked += menuClicked;
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