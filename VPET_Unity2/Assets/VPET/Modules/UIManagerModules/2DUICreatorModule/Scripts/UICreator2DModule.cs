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
//! @date 18.08.2021

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! implementation of VPET 2D UI scene creator module
    //!
    public class UICreator2DModule : UIManagerModule
    {

        private List<GameObject> UIParameterList = new List<GameObject>();
        private Canvas m_canvas;
        public UICreator2DModuleSettings settings;
        private SceneObjectViewMenu m_sceneObjectViewMenu;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator2DModule(string name, Core core) : base(name, core)
        {

        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            manager.selectionChanged += createUI;
            settings = Resources.Load("DATA_VPET_2D_UI_Settings") as UICreator2DModuleSettings;

            m_canvas = m_core.m_vpetCanvas;
            m_sceneObjectViewMenu = GameObject.FindObjectOfType<SceneObjectViewMenu>();
        }

        //!
        //! Function that recreates the UI Layout.
        //! Being called when selection has changed.
        //!
        private void createUI(object sender, List<SceneObject> sceneObjects)
        {
            clearUI();
            
            // [REVIEW]
            // UI Debug
            Debug.Log("--- Creating 2D UI ---");
            Debug.Log(sceneObjects);

            m_sceneObjectViewMenu.Init(this, sceneObjects);

            /* int buttonOffset = 50;

            foreach (SceneObject sceneObject in sceneObjects)
            {
                //foreach (AbstractParameter param in sceneObject.parameterList)
                for (int i = 0; i < sceneObject.parameterList.Count ; i++)
                {
                    AbstractParameter param = sceneObject.parameterList[i];
                    Vector3 sliderPosition = new Vector3(800, 400 - (buttonOffset * i), 0);
                    
                    Helpers.Log(sceneObject.name + ": " + param.name + " type:" + param.cType);

                    GameObject gameObjectInstance = null;

                    switch (param.vpetType)
                    {
                        case AbstractParameter.ParameterType.FLOAT:
                            {
                                gameObjectInstance = SceneObject.Instantiate(m_slider, Vector3.zero, Quaternion.identity);
                                Parameter<float> p = (Parameter<float>)param;
                                gameObjectInstance.GetComponent<Slider>().onValueChanged.AddListener(p.setValue);
                                buttonOffset = 50;
                                break;
                            }
                        case AbstractParameter.ParameterType.VECTOR3:
                            {
                                gameObjectInstance = SceneObject.Instantiate(m_spinner, Vector3.zero, Quaternion.identity);
                                Parameter<Vector3> p = (Parameter<Vector3>)param;
                                Spinner spinner = gameObjectInstance.GetComponent<Spinner>();
                                spinner.Init(p.value);
                                spinner.hasChanged += p.setValue;
                                buttonOffset = 100;
                                break;
                            }
                    }

                    //If we added a UI Elemnt, position it here
                    if (gameObjectInstance != null)
                    {
                        UIParameterList.Add(gameObjectInstance);

                        RectTransform rectTransform = gameObjectInstance.GetComponent<RectTransform>();
                        rectTransform.SetPositionAndRotation(sliderPosition, Quaternion.identity);
                        gameObjectInstance.transform.SetParent(GameObject.Find("Canvas").transform);

                        gameObjectInstance.GetComponentInChildren<Text>().text = param.name;
                    }
                }
            } */
        }

        private void clearUI()
        {
            /* foreach (GameObject oldUIElement in UIParameterList)
            {
                UnityEngine.Object.Destroy(oldUIElement);
            }

            UIParameterList.Clear(); */

            m_sceneObjectViewMenu.Clear();
        }

    }
}