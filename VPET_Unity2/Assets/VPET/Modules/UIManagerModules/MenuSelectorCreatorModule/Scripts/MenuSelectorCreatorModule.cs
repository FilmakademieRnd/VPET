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
//! @date 21.01.2022

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

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
        public MenuSelectorCreatorModule(string name, Core core) : base(name, core)
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
            // [REVIEW]
            // pls remove, only for testing...

            MenuButton test = new MenuButton("TestButton", testAction);
            test.setIcon("Images/button_frame_BG");
            manager.addButton(test);

            GameObject canvasRes = Resources.Load("Prefabs/MenuSelectorCanvas") as GameObject;
            
            GameObject menuSelectorPrefab = Resources.Load("Prefabs/MenuSelectorPrefab") as GameObject;
            GameObject buttonSelectorPrefab = Resources.Load("Prefabs/ButtonSelectorPrefab") as GameObject;

            GameObject canvas = GameObject.Instantiate(canvasRes);
            m_menuSelector = GameObject.Instantiate(menuSelectorPrefab, canvas.transform).GetComponent<SnapSelect>();
            SnapSelect buttonSelector = GameObject.Instantiate(buttonSelectorPrefab, canvas.transform).GetComponent<SnapSelect>();

            manager.menuSelected += highlightMenuElement;

            foreach (MenuTree menu in manager.getMenus())
            {
                if (menu.iconResourceLocation.Length > 0)
                {
                    Sprite resImage = Resources.Load<Sprite>(menu.iconResourceLocation);
                    if (resImage != null)
                        m_menuSelector.addElement(menu.caption, resImage);

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

        private void menuClicked(object sender, int id)
        {
            manager.showMenu((MenuTree)manager.getMenus()[id]);
        }

        private void highlightMenuElement(object sender, MenuTree t)
        {
            if (t == null)
                m_menuSelector.showHighlighted(-1);
            else
                m_menuSelector.showHighlighted(t.id);
        }

        public void testAction()
        {
            Debug.Log("Test Action!");
        }
    }

}