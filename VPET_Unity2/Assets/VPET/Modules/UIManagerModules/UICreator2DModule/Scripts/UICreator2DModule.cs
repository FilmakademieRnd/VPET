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

//! @file "UICreator2DModule.cs"
//! @brief implementation of 2D manipulator UI module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @author Paulo Scatena
//! @version 0
//! @date 14.02.2022

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
        // icon layout configuration
        private int selectorsPerRow = 3;
        private int selectorSize = 100;
        private int selectorSpacing = 10;

        //settings resource
        public UICreator2DModuleSettings settings;

        //List of Manipolators
        private List<GameObject> instancedManipulators = new List<GameObject>();

        //List of selection Buttons for Manipulators
        private List<GameObject> instancedManipulatorSelectors = new List<GameObject>();

        private Transform UI2D;
        private Transform manipulatorPanel;
        private Transform manipulatorSelectionPanel;

        public bool turnOnAndOffCanvasObject = false;
        public bool blocksRaycasts = true;
        public bool isActive = true;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator2DModule(string name, Core core) : base(name, core)
        {
            GameObject canvas = Resources.Load<GameObject>("Prefabs/PRE_Canvas_2DUI");
            Transform canvasTrans = SceneObject.Instantiate(canvas).transform;
            canvasTrans.name = "Canvas_2DUI";
            UI2D = canvasTrans.GetChild(0).transform;
            manipulatorSelectionPanel = UI2D.GetChild(0);
            manipulatorPanel = UI2D.GetChild(1);
            HideMenu();
        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor.
        //! @param sender callback sender
        //! @param e event reference
        //!
        protected override void Init(object sender, EventArgs e)
        {
            manager.selectionChanged += createUI;
            settings = Resources.Load("DATA_VPET_2D_UI_Settings") as UICreator2DModuleSettings;

            //m_sceneObjectViewMenu = GameObject.FindObjectOfType<SceneObjectViewMenu>();
        }

        //!
        //! Function that recreates the UI Layout.
        //! Being called when selection has changed.
        //! @param sender callback sender
        //! @param sceneObjects event payload containg all sceneObjects the UI shall be created for
        //!
        private void createUI(object sender, List<SceneObject> sceneObjects)
        {

            clearUI();

            if (sceneObjects.Count < 1)
            {
                return;
            }

            ShowMenu();

            //TODO Account for more than the first sceneObject being selected
            SceneObject mainSelection = sceneObjects[0];

            int manipIndex = 0;
            int paramIndex = 0;
            foreach (var paramater in mainSelection.parameterList)
            {
                //Check which type of Manipulator edits this type of Parameter
                //Get UI Prefab Reference via this Manipulator Type
                try
                {
                    ManipulatorReference manipRef = settings.manipulators[paramIndex];

                    if (manipRef.manipulatorPrefab != null)
                    {
                        Manipulator createdManip = SceneObject.Instantiate(manipRef.manipulatorPrefab, manipulatorPanel);
                        Debug.Log(createdManip.name);
                        createdManip.gameObject.SetActive(false);
                        createdManip.LinkToParameter(paramater);
                        instancedManipulators.Add(createdManip.gameObject);

                        ManipulatorSelector createdManipSelector = SceneObject.Instantiate(settings.manipulatorSelector, manipulatorSelectionPanel);
                        createdManipSelector.GetComponent<RectTransform>().sizeDelta = new Vector2(selectorSize, selectorSize);
                        createdManipSelector.GetComponent<RectTransform>().localPosition = new Vector2((selectorSize+selectorSpacing) * (paramIndex % selectorsPerRow - 1),
                                                                                                       -(selectorSize + selectorSpacing) * ((float)Math.Floor(paramIndex/ (float)selectorsPerRow)-1f));
                        instancedManipulatorSelectors.Add(createdManipSelector.gameObject);

                        //Initialization of the different Manipulators
                        createdManipSelector.Init(this, manipRef.selectorIcon, manipIndex);
                        switch (manipRef.parameterType)
                        {
                            case ParameterType.Position:
                                Spinner spinnerPos = (Spinner)createdManip;
                                spinnerPos.Init(mainSelection.gameObject.transform.position);
                                break;
                            case ParameterType.Rotation:
                                Spinner spinnerRot = (Spinner)createdManip;
                                spinnerRot.Init(mainSelection.gameObject.transform.rotation.eulerAngles);
                                break;
                            case ParameterType.Scale:
                                Spinner spinnerScale = (Spinner)createdManip;
                                spinnerScale.Init(mainSelection.gameObject.transform.localScale);
                                break;
                        }

                        manipIndex++;
                    }
                }
                catch (System.IndexOutOfRangeException e)
                {

                }

                paramIndex++;
            }

            if (instancedManipulators.Count > 0)
            {
                SelectManipulator(0);
            }
        }

        //!
        //! function to delete all 2D UI elements
        //!
        private void clearUI()
        {
            foreach (var manip in instancedManipulators)
            {
                GameObject.Destroy(manip);
            }
            instancedManipulators.Clear();

            foreach (var manipSelec in instancedManipulatorSelectors)
            {
                GameObject.Destroy(manipSelec);
            }
            instancedManipulatorSelectors.Clear();
        }

        //!
        //! function called when manipulator shall be changed
        //! @param index index of the Manipulator to be chosen
        //!
        public void SelectManipulator(int index)
        {
            instancedManipulators.ForEach(manip => manip.gameObject.SetActive(false));
            instancedManipulators[index].gameObject.SetActive(true);
            foreach (GameObject g in instancedManipulatorSelectors)
                g.GetComponent<ManipulatorSelector>().visualizeIdle();
            instancedManipulatorSelectors[index].GetComponent<ManipulatorSelector>().visualizeActive();
            // actual send command
            manager.setManipulatorMode(index);
        }

        //!
        //! function to enable or disapble 2D UI interactablitity
        //! @param value interactable true/false

        //!
        private void SetInteractable(bool value)
        {
            UI2D.GetComponent<CanvasGroup>().interactable = value;
            UI2D.GetComponent<CanvasGroup>().blocksRaycasts = value ? blocksRaycasts : false;
        }

        //!
        //! function to set 2D UI alpha
        //! @param value alpha value
        //!
        private void SetAlpha(float value)
        {
            UI2D.GetComponent<CanvasGroup>().alpha = value;
        }

        //!
        //! function to activate or deactivate 2D UI
        //! @param value avtivate/deactivate
        //!
        private void SetActive(bool value)
        {
            UI2D.gameObject.SetActive(value);
        }

        //!
        //! Show 2D manipulator menu
        //! @param setInteractable sets if menu shall be initially interactable
        //! @param onComplete action being called when menu is fully visible
        //!
        public virtual void ShowMenu(bool setInteractable = true, System.Action onComplete = null)
        {
            //Before Fade
            if (turnOnAndOffCanvasObject)
            {
                SetActive(true);
            }

            SetAlpha(1f);
            isActive = true;

            if (setInteractable)
            {
                SetInteractable(true);
            }

            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }

        //!
        //! Hide 2D manipulator menu
        //! @param setInteractable sets if menu shall be initially interactable
        //! @param onComplete action being called when menu is fully visible
        //!
        public void HideMenu(bool setInteractable = true, System.Action onComplete = null)
        {
            //Before Fade
            isActive = false;

            if (setInteractable)
            {
                SetInteractable(false);
            }

            SetAlpha(0f);
            if (turnOnAndOffCanvasObject)
            {
                SetActive(false);
            }

            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }
    }
}