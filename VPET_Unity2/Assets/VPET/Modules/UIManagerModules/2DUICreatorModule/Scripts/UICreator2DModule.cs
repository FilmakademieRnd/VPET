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
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "UICreator2DModule.cs"
//! @brief implementation of VPET 2D UI scene creator module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 14.07.2021

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! implementation of VPET 2D UI scene creator module
    //!
    public class UICreator2DModule : UIManagerModule
    {

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator2DModule(string name, Core core) : base(name, core)
        {

        }

        protected override void Init(object sender, EventArgs e)
        {
            manager.selectionChanged += createUI;
        }


        //!
        //! Function that recreates the UI Layout.
        //! Being called when selection has changed.
        //!
        private void createUI(object sender, UIManager.SEventArgs a)
        {
            foreach (SceneObject sceneObject in a._value)
            {
                foreach (AbstractParameter param in sceneObject.parameterList)
                {
                    Type type = param.GetType();
                    Helpers.Log(sceneObject.name + ": " + param.name + " type:" + type);
                    if (type == typeof(Parameter<Vector3>))
                    {
                        //GameObject button = Resources.Load("Prefabs/Button") as GameObject;
                        //GameObject buttonInstance = SceneObject.Instantiate(button, Vector3.zero, Quaternion.identity);
                        
                        //var rectTransform = button.GetComponent<RectTransform>();
                        //rectTransform.SetParent(GameObject.Find("Canvas").transform);
                        //rectTransform.offsetMin = Vector2.zero;
                        //rectTransform.offsetMax = Vector2.zero;
                        //button.onClick.AddListener(SpawnPlayer);
                    }
                }
            }
        }

    }
}