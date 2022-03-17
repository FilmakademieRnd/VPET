/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright(c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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

//! @file "MenuCreatorModule.cs"
//! @brief Implementation of the MenuCreatorModule, creating UI menus based on a menuTree object.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 02.03.2022

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace vpet
{
    public class MenuCreatorModule : UIManagerModule
    {
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public MenuCreatorModule(string name, Core core) : base(name, core)
        {
        }

        //!
        //! A reference to the previous created menu, null at the beginning.
        //!
        private MenuTree m_oldMenu;
        //!
        //! Prefab for the Unity canvas object.
        //!
        private GameObject m_canvas;
        //!
        //! Prefab for the Unity panel object, used as pacer and background.
        //!
        private GameObject m_panel;
        //!
        //! Prefab for the Unity button object, used as UI element for an action parameter.
        //!
        private GameObject m_button;
        //!
        //! Prefab for the Unity toggle object, used as UI element for an bool parameter.
        //!
        private GameObject m_toggle;
        //!
        //! Prefab for the Unity text object, used as UI element for an string (read only) parameter.
        //!
        private GameObject m_text;
        //!
        //! Prefab for the Unity input field object, used as UI element for an string parameter.
        //!
        private GameObject m_inputField;
        //!
        //! Prefab for the Unity dropdown object, used as UI element for a list parameter.
        //!
        private GameObject m_dropdown;
        //!
        //! The list containing all UI elemets of the current menu.
        //!
        private List<GameObject> m_uiElements;

        //!
        //! Init Function
        //!
        protected override void Init(object sender, EventArgs e)
        {
            m_uiElements = new List<GameObject>();

            m_canvas = Resources.Load("Prefabs/MenuCanvas") as GameObject;
            m_panel = Resources.Load("Prefabs/MenuPanel") as GameObject;
            m_button = Resources.Load("Prefabs/MenuButton") as GameObject;
            m_toggle = Resources.Load("Prefabs/MenuToggle") as GameObject;
            m_text = Resources.Load("Prefabs/MenuText") as GameObject;
            m_inputField = Resources.Load("Prefabs/MenuInputField") as GameObject;
            m_dropdown = Resources.Load("Prefabs/MenuDropdown") as GameObject;

            // [REVIEW]
            // Just for testing, please remove!
            //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            List<AbstractParameter> parameterList = new List<AbstractParameter>();
            parameterList.Add(new Parameter<float>(1, "t1"));
            parameterList.Add(new Parameter<float>(12, "t2"));
            parameterList.Add(new Parameter<float>(100, "t4"));
            parameterList.Add(new Parameter<float>(7, "t12"));

            MenuTree menu = new MenuTree()
               .Begin(MenuItem.IType.HSPLIT)
                   .Begin(MenuItem.IType.VSPLIT)
                       .Add(new Parameter<string>("This is a test string", "StringParameter"))
                       .Add(MenuItem.IType.SPACE)
                       .Add(new Parameter<bool>(true, "BoolParameter"))
                       .Add(MenuItem.IType.SPACE)
                       .Add("That's an info text string.")
                   .End()
                   .Begin(MenuItem.IType.HSPLIT)
                       .Add(new Parameter<object>(null, "OK"))
                       .Add(new Parameter<object>(null, "Abort"))
                       .Add(new ListParameter(parameterList, "TestList"))
                   .End()
              .End();
            menu.caption = "TestMenu";
            menu.setIcon("Images/button_translate");
            manager.addMenu(menu);

            MenuTree menu2 = new MenuTree()
               .Begin(MenuItem.IType.VSPLIT)

               .Begin(MenuItem.IType.VSPLIT)
                   .Add(new Parameter<string>("This is a test string", "StringParameter"))
                   .Add(MenuItem.IType.SPACE)
                   .Add(new Parameter<bool>(true, "BoolParameter"))
                   .Add(MenuItem.IType.SPACE)
                   .Add("That's an info text string.")
               .End()
               .Begin(MenuItem.IType.HSPLIT)
                   .Add(new Parameter<object>(null, "OK"))
                   .Add(new Parameter<object>(null, "Abort"))
               .End()
            .End();

            menu2.caption = "Bla!";
            manager.addMenu(menu2);
            // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            manager.menuSelected += createMenu;
        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        ~MenuCreatorModule()
        {
            manager.menuSelected -= createMenu;
        }

        //!
        //! Function creating a menu UI element based on a MenuTree object.
        //!
        //! @param sender A reference to the UI manager.
        //! @param menu A reference to the MenuTree used to create the UI elements of a menu.
        //!
        void createMenu(object sender, MenuTree menu)
        {
            destroyMenu();

            if (menu == null)
                return;

            if (menu.visible && m_oldMenu == menu)
                menu.visible = false;
            else
            {
                GameObject menuCanvas = GameObject.Instantiate(m_canvas);
                m_uiElements.Add(menuCanvas);
                GameObject rootPanel = menuCanvas.transform.Find("Panel").gameObject;
                m_uiElements.Add(rootPanel);
                menu.Items.ForEach(p => createMenufromTree(p, rootPanel));
                menu.visible = true;
            }

            m_oldMenu = menu;
        }

        //!
        //! Function that builds UI menu objecrts by recursively traversing a menuTree, starting at the given menuItem.
        //!
        //! @param item The start item for the tree traversal.
        //! @param parentObject The items parent Unity GameObject.
        //!
        private void createMenufromTree(MenuItem item, GameObject parentObject)
        {
            GameObject newObject = null;

            switch (item.Type)
            {
                case MenuItem.IType.HSPLIT:
                    newObject = GameObject.Instantiate(m_panel, parentObject.transform);
                    HorizontalLayoutGroup horizontalLayout = newObject.AddComponent<HorizontalLayoutGroup>();
                    horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
                    if ((parentObject.GetComponent<HorizontalLayoutGroup>() != null) ||
                        (parentObject.GetComponent<VerticalLayoutGroup>() != null))
                    {
                        horizontalLayout.childForceExpandHeight = false;
                        horizontalLayout.childForceExpandWidth = false;
                        horizontalLayout.childControlHeight = false;
                        horizontalLayout.childControlWidth = false;
                    }
                    break;
                case MenuItem.IType.VSPLIT:
                    newObject = GameObject.Instantiate(m_panel, parentObject.transform);
                    VerticalLayoutGroup verticalLayout = newObject.AddComponent<VerticalLayoutGroup>();
                    verticalLayout.childAlignment = TextAnchor.MiddleCenter;
                    if ((parentObject.GetComponent<HorizontalLayoutGroup>() != null) ||
                        (parentObject.GetComponent<VerticalLayoutGroup>() != null))
                    {
                        verticalLayout.childForceExpandHeight = false;
                        verticalLayout.childForceExpandWidth = false;
                        verticalLayout.childControlHeight = true;
                        verticalLayout.childControlWidth = true;
                    }
                    break;
                case MenuItem.IType.SPACE:
                    {
                        newObject = GameObject.Instantiate(m_panel, parentObject.transform);
                    }
                    break;
                case MenuItem.IType.TEXT:
                    {
                        newObject = GameObject.Instantiate(m_text, parentObject.transform);
                        TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
                        textComponent.text = ((Parameter<string>)item.Parameter).value;                        
                        textComponent.color = manager.uiSettings.colors.FontColor;
                        textComponent.font = manager.uiSettings.defaultFont;
                        textComponent.fontSize = manager.uiSettings.defaultFontSize;
                        textComponent.fontStyle = FontStyles.Bold;
                     }
                    break;
                case MenuItem.IType.PARAMETER:
                    switch (item.Parameter.vpetType) 
                    {
                        case AbstractParameter.ParameterType.ACTION:
                            {
                                newObject = GameObject.Instantiate(m_button, parentObject.transform);
                                Button button = newObject.GetComponent<Button>();
                                Action parameterAction = ((Parameter<Action>)item.Parameter).value;
                                button.onClick.AddListener(() => parameterAction());
                                TextMeshProUGUI textComponent = newObject.GetComponentInChildren<TextMeshProUGUI>();
                                textComponent.text = item.Parameter.name;
                                textComponent.color = manager.uiSettings.colors.FontColor;
                                textComponent.font = manager.uiSettings.defaultFont;
                                textComponent.fontSize = manager.uiSettings.defaultFontSize;
                                Image imgButton = button.GetComponent<Image>();
                                imgButton.color = manager.uiSettings.colors.ButtonBG;
                                
                            }
                        break;
                        case AbstractParameter.ParameterType.BOOL:
                            {
                                newObject = GameObject.Instantiate(m_toggle, parentObject.transform);
                                Toggle toggle = newObject.GetComponent<Toggle>();
                                toggle.onValueChanged.AddListener(delegate { ((Parameter<bool>)item.Parameter).setValue(toggle.isOn); });
                                toggle.isOn = ((Parameter<bool>) item.Parameter).value;
                                Text textComponent = newObject.GetComponentInChildren<Text>();
                                textComponent.text = item.Parameter.name;
                                textComponent.color = manager.uiSettings.colors.FontColor;
                                textComponent.fontSize = manager.uiSettings.defaultFontSize;
                            }
                        break;
                        case AbstractParameter.ParameterType.STRING:
                            {
                                newObject = GameObject.Instantiate(m_inputField, parentObject.transform);
                                TMP_InputField inputField = newObject.GetComponent<TMP_InputField>();
                                inputField.onEndEdit.AddListener(delegate { ((Parameter<string>)item.Parameter).setValue(inputField.text); });
                                inputField.text = ((Parameter<string>)item.Parameter).value;
                                Image imgButton = inputField.GetComponent<Image>();
                                imgButton.color = manager.uiSettings.colors.DropDown_TextfieldBG;
                                inputField.textComponent.color = manager.uiSettings.colors.FontColor;
                                inputField.textComponent.font = manager.uiSettings.defaultFont;
                                inputField.textComponent.fontSize = manager.uiSettings.defaultFontSize;
                            }
                            break;
                        case AbstractParameter.ParameterType.LIST:
                            {
                                newObject = GameObject.Instantiate(m_dropdown, parentObject.transform);
                                TMP_Dropdown dropDown = newObject.GetComponent<TMP_Dropdown>();
                                List<string> names = new List<string>();

                                foreach (AbstractParameter parameter in ((ListParameter)item.Parameter).parameterList)
                                    names.Add(parameter.name);

                                dropDown.AddOptions(names);
                                dropDown.onValueChanged.AddListener(delegate { ((ListParameter)item.Parameter).select(dropDown.value); });

                                dropDown.image.color = manager.uiSettings.colors.DropDown_TextfieldBG;                                
                                dropDown.captionText.color = manager.uiSettings.colors.FontColor;
                                dropDown.captionText.font = manager.uiSettings.defaultFont;
                                dropDown.captionText.fontSize = manager.uiSettings.defaultFontSize;
                                dropDown.itemText.color = manager.uiSettings.colors.FontColor;
                                dropDown.itemText.font = manager.uiSettings.defaultFont;
                                dropDown.itemText.fontSize = manager.uiSettings.defaultFontSize;

                            }
                                break;
                    }
                break;
            }

            m_uiElements.Add(newObject);
            item.Children.ForEach(p => createMenufromTree(p, newObject));
        }

        //!
        //! Function to destroy all created UI elements of a menu.
        //!
        private void destroyMenu()
        {
            foreach (GameObject uiElement in m_uiElements)
            {
                UnityEngine.Object.Destroy(uiElement);
            }
            m_uiElements.Clear();
        }
    }
}
