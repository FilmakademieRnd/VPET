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
        private int selectorSize = 15;
        private int selectorSpacing = 2;

        //Currently displayed manipulator (can be null if none is displayed)
        GameObject currentManipulator;

        //Currently displayed AddSelector (can be null if none is displayed)
        GameObject currentAddSelector;

        //Button for additional parameters, hidden if currentAddSelector is active
        GameObject currentAddButton;

        //Currently displayed manipulator (can be null if none is displayed)
        GameObject selectorPrefab;

        //List of selection Buttons for Manipulators
        private List<GameObject> instancedManipulatorSelectors = new List<GameObject>();

        //!
        //! Event emitted when parameter has changed
        //!
        public event EventHandler<int> parameterChanged;

        private Transform UI2D;
        private Transform manipulatorPanel;
        private Transform manipulatorSelectionPanel;
        private Button undoButton;
        private Button redoButton;
        private Button resetButton;

        public bool blocksRaycasts = true;

        //!
        //! currently selected SceneObject
        //!
        private SceneObject mainSelection;

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
            undoButton = UI2D.GetChild(2).GetChild(0).GetComponent<Button>();
            redoButton = UI2D.GetChild(2).GetChild(1).GetComponent<Button>();
            resetButton = UI2D.GetChild(2).GetChild(2).GetComponent<Button>();
            undoButton.onClick.AddListener(() => manager.core.getManager<SceneManager>().getModule<UndoRedoModule>().undoStep());
            redoButton.onClick.AddListener(() => manager.core.getManager<SceneManager>().getModule<UndoRedoModule>().redoStep());


            selectorPrefab = Resources.Load<GameObject>("Prefabs/PRE_UI_Manipulator_Selector");

            HideMenu();
        }

        //!
        //! Destructor
        //!
        ~UICreator2DModule()
        {
            undoButton.onClick.RemoveAllListeners();
            redoButton.onClick.RemoveAllListeners();
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
            mainSelection = sceneObjects[0];

            int paramIndex = 0;
            if (mainSelection.parameterList.Count > 2)
            {
                //inititalize selectors for translation, rotation, scale
                for (int i = 0; i < ((mainSelection.parameterList.Count > 3)? 4 : 3); i++)
                {
                    GameObject createdManipSelector = SceneObject.Instantiate(selectorPrefab, manipulatorSelectionPanel);
                    createdManipSelector.GetComponent<RectTransform>().sizeDelta = new Vector2(selectorSize, selectorSize);
                    createdManipSelector.GetComponent<RectTransform>().localPosition = new Vector2((selectorSize + selectorSpacing) * paramIndex, -selectorSpacing);
                    instancedManipulatorSelectors.Add(createdManipSelector.gameObject);

                    Sprite icon = Resources.Load<Sprite>("Images/button_translate");
                    switch (i)
                    {
                        //translation
                        case 0:
                            icon = Resources.Load<Sprite>("Images/button_translate");
                            break;
                        //rotation
                        case 1:
                            icon = Resources.Load<Sprite>("Images/button_rotate");
                            break;
                        //scale
                        case 2:
                            icon = Resources.Load<Sprite>("Images/button_scale");
                            break;
                        case 3:
                            icon = Resources.Load<Sprite>("Images/button_more");
                            createdManipSelector.GetComponent<RectTransform>().localPosition = new Vector2((selectorSize + selectorSpacing), -selectorSize - 2* selectorSpacing);
                            currentAddButton = createdManipSelector;
                            break;
                    }
                    if (icon)
                        createdManipSelector.GetComponent<ManipulatorSelector>().Init(this, manager.uiSettings, icon, (i==3)? -1 : i);


                    paramIndex++;
                }

                //handle additional parameters
                if (mainSelection.parameterList.Count > 3)
                {
                    GameObject spinnerPrefab = Resources.Load<GameObject>("Prefabs/PRE_UI_AddSelector");
                    currentAddSelector = SceneObject.Instantiate(spinnerPrefab, UI2D);
                    SnapSelect snapSelect = currentAddSelector.GetComponent<SnapSelect>();
                    snapSelect.uiSettings = manager.uiSettings;

                    for (int i = 3; i < mainSelection.parameterList.Count; i++)
                    {
                        snapSelect.addElement(mainSelection.parameterList[i].name);
                    }
                    snapSelect.parameterChanged += createAdditionalManipulator;
                    currentAddSelector.SetActive(false);
                }

                createManipulator(0);
            }
        }

        //!
        //! function to delete all 2D UI elements
        //!
        private void clearUI()
        {
            GameObject.DestroyImmediate(currentManipulator);
            GameObject.DestroyImmediate(currentAddSelector);
            if(manipulatorPanel)
                manipulatorPanel.gameObject.SetActive(true);
            if (undoButton)
            {
                undoButton.gameObject.SetActive(false);
                redoButton.gameObject.SetActive(false);
                resetButton.gameObject.SetActive(false);
            }

            foreach (var manipSelec in instancedManipulatorSelectors)
            {
                GameObject.DestroyImmediate(manipSelec);
            }
            instancedManipulatorSelectors.Clear();
        }

        private void createAdditionalManipulator(object sender, int i)
        {
            createManipulator(i + 3);
        }

        //!
        //! function called when manipulator shall be changed
        //! @param index index of the Parameter a Manipulator shall be drawn for
        //!
        public void createManipulator(int index)
        {
            if (parameterChanged != null)
                parameterChanged.Invoke(this, index);
            if (currentManipulator)
                GameObject.Destroy(currentManipulator);

            if (index < 0 && currentAddSelector)
            {
                currentAddButton.SetActive(false);
                currentAddSelector.SetActive(true);
                createManipulator(currentAddSelector.GetComponent<SnapSelect>().currentAxis+3);
                return;
            }
            else if (index < 3 && currentAddSelector)
            {
                currentAddButton.SetActive(true);
                currentAddSelector.SetActive(false);
            }

            AbstractParameter abstractParam = mainSelection.parameterList[index];
            AbstractParameter.ParameterType type = abstractParam.vpetType;

            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => abstractParam.reset());

            switch (type)
            {
                case AbstractParameter.ParameterType.FLOAT:
                case AbstractParameter.ParameterType.VECTOR2:
                case AbstractParameter.ParameterType.VECTOR3:
                case AbstractParameter.ParameterType.QUATERNION:
                    GameObject spinnerPrefab = Resources.Load<GameObject>("Prefabs/PRE_UI_Spinner");
                    currentManipulator = SceneObject.Instantiate(spinnerPrefab, manipulatorPanel);
                    currentManipulator.GetComponent<Spinner>().uiSettings = manager.uiSettings;
                    currentManipulator.GetComponent<Spinner>().Init(abstractParam);
                    currentManipulator.GetComponent<Spinner>().doneEditing += manager.core.getManager<SceneManager>().getModule<UndoRedoModule>().addHistoryStep;
                    break;
                case AbstractParameter.ParameterType.COLOR:
                    GameObject resourcePrefab = Resources.Load<GameObject>("Prefabs/PRE_UI_ColorPicker");
                    currentManipulator = SceneObject.Instantiate(resourcePrefab, manipulatorPanel);
                    currentManipulator.GetComponent<ColorSelect>().Init(abstractParam);
                    // Set TRS manipulator to null
                    parameterChanged.Invoke(this, -1);
                    break;
                case AbstractParameter.ParameterType.ACTION:
                case AbstractParameter.ParameterType.BOOL:
                case AbstractParameter.ParameterType.INT:
                case AbstractParameter.ParameterType.STRING:
                case AbstractParameter.ParameterType.VECTOR4:
                default:
                    Helpers.Log("No UI for parameter type implemented...");
                    break;

            }

            foreach (GameObject g in instancedManipulatorSelectors)
                g.GetComponent<ManipulatorSelector>().visualizeIdle();

            if(index < 3)
                instancedManipulatorSelectors[index].GetComponent<ManipulatorSelector>().visualizeActive();
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
        //! Show 2D manipulator menu
        //! @param setInteractable sets if menu shall be initially interactable
        //!
        public virtual void ShowMenu(bool setInteractable = true)
        {

            SetAlpha(1f);
            undoButton.gameObject.SetActive(true);
            redoButton.gameObject.SetActive(true);
            resetButton.gameObject.SetActive(true);
            if (setInteractable)
            {
                SetInteractable(true);
            }
        }

        //!
        //! Hide 2D manipulator menu
        //! @param setInteractable sets if menu shall be initially interactable
        //!
        public void HideMenu(bool setInteractable = true)
        {
            SetAlpha(0f);
            undoButton.gameObject.SetActive(false);
            redoButton.gameObject.SetActive(false);
            resetButton.gameObject.SetActive(false);

            if (setInteractable)
                SetInteractable(false);
        }
    }
}