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
using UnityEngine.UI;
using TMPro;

namespace vpet
{
    public class MenuSelectorCreatorModule : UIManagerModule
    {
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
        protected override void Start(object sender, EventArgs e)
        {
            GameObject canvasRes = Resources.Load("Prefabs/MenuSelectorCanvas") as GameObject;
            GameObject buttonRes = Resources.Load("Prefabs/MenuSelectorButton") as GameObject;
            
            GameObject menuSelectorPrefab = Resources.Load("Prefabs/MenuSelectorPrefab") as GameObject;


            GameObject canvas = GameObject.Instantiate(canvasRes);
            SnapSelect menuSelector = GameObject.Instantiate(menuSelectorPrefab, canvas.transform).GetComponent<SnapSelect>();
            
            Transform contentTransform = canvas.transform.FindDeepChild("Content");

            List<string> menuNames = new List<string>();

            foreach (MenuTree menu in manager.getMenus())
            {
                menuNames.Add(menu.name);

                //GameObject buttonInst = GameObject.Instantiate(buttonRes, contentTransform);
                //Button button = buttonInst.GetComponent<Button>();
                //button.onClick.AddListener(() => manager.showMenu(menu));
                //TextMeshProUGUI textComponent = buttonInst.GetComponentInChildren<TextMeshProUGUI>();
                //textComponent.text = menu.name;
                //if (menu.iconResourceLocation.Length > 0)
                //{
                //    Sprite resImage = Resources.Load<Sprite>(menu.iconResourceLocation);
                //    if (resImage != null)
                //    {
                //        Image buttonImage = buttonInst.GetComponentInChildren<Image>();
                //        buttonImage.sprite = resImage;
                //    }
                //    else
                //        Helpers.Log("Menu Icon resource: " + menu.iconResourceLocation + " not found!", Helpers.logMsgType.WARNING);
                //}
            }

            menuSelector.Init(menuNames);
            menuSelector.elementClicked += menuClicked;
        }
        private void menuClicked(object sender, int id)
        {
            manager.showMenu(manager.getMenus()[id]);
        }
    }

}