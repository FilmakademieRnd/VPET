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
//! @date 19.09.2022

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
        public MenuCreatorModule(string name, Manager manager) : base(name, manager)
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
        //! Prefab for the Unity input field object, used as UI element for an string parameter.
        //!
        private GameObject m_numberInputField;
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
            m_numberInputField = Resources.Load("Prefabs/MenuNumberInputField") as GameObject;
            m_dropdown = Resources.Load("Prefabs/MenuDropdown") as GameObject;

            manager.menuSelected += createMenu;
            manager.menuDeselected += hideMenu;
        }

        //!
        //! function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);
            manager.menuSelected -= createMenu;
            manager.menuDeselected -= hideMenu;
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
            {
                m_oldMenu.visible = false;
                return;
            }

            if (menu.visible && m_oldMenu == menu)
                menu.visible = false;
            else
            {    
                GameObject menuCanvas = GameObject.Instantiate(m_canvas);
                menuCanvas.GetComponent<Canvas>().sortingOrder = 15;
                m_uiElements.Add(menuCanvas);
                GameObject rootPanel = menuCanvas.transform.Find("Panel").gameObject;                
                //Image imageComponent = menuCanvas.GetComponentInChildren<Image>();
                Image imageComponent = menuCanvas.transform.Find("Panel_Menu").GetComponent<Image>();
                imageComponent.color = manager.uiAppearanceSettings.colors.MenuTitleBG;
                m_uiElements.Add(rootPanel);
                TextMeshProUGUI menuTitle = menuCanvas.transform.FindDeepChild("Text").GetComponent<TextMeshProUGUI>();
                menuTitle.font = manager.uiAppearanceSettings.defaultFont;
                menuTitle.fontSize = manager.uiAppearanceSettings.defaultFontSize + 1;
                menuTitle.color = manager.uiAppearanceSettings.colors.FontColor;
                menuTitle.text = menu.caption;
                GameObject button = menuCanvas.transform.FindDeepChild("Button").gameObject;
                button.GetComponent<Button>().onClick.AddListener(() => manager.hideMenu());

                ScrollRect rect = rootPanel.GetComponent<ScrollRect>();
                foreach (MenuItem p in menu.Items)
                {
                    GameObject gameObject = createMenufromTree(p, rootPanel);
                    m_uiElements.Add(gameObject);
                    if (menu.scrollable)
                        rect.content = gameObject.GetComponent<RectTransform>();
                    else
                        GameObject.Destroy(rect.verticalScrollbar.transform.gameObject);
                }
                rect.verticalScrollbar.transform.SetAsLastSibling();
                
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
        private GameObject createMenufromTree(MenuItem item, GameObject parentObject)
        {
            GameObject newObject = null;

            switch (item.Type)
            {
                case MenuItem.IType.HSPLIT:
                    newObject = GameObject.Instantiate(m_panel, parentObject.transform);
                    HorizontalLayoutGroup horizontalLayout = newObject.AddComponent<HorizontalLayoutGroup>();
                    horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
                    horizontalLayout.childForceExpandHeight = false;
                    horizontalLayout.childForceExpandWidth = false;
                    horizontalLayout.childControlHeight = false;
                    horizontalLayout.childControlWidth = false;
                    horizontalLayout.spacing = 2;
                    break;
                case MenuItem.IType.VSPLIT:
                    newObject = GameObject.Instantiate(m_panel, parentObject.transform);
                    VerticalLayoutGroup verticalLayout = newObject.AddComponent<VerticalLayoutGroup>();
                    verticalLayout.childAlignment = TextAnchor.MiddleCenter;
                    verticalLayout.childForceExpandHeight = false;
                    verticalLayout.childForceExpandWidth = false;
                    verticalLayout.childControlHeight = true;
                    verticalLayout.childControlWidth = true;
                    verticalLayout.spacing = 2;
                    verticalLayout.padding.top = 3;
                    verticalLayout.padding.bottom = 3;
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
                        textComponent.color = manager.uiAppearanceSettings.colors.FontColor;
                        textComponent.font = manager.uiAppearanceSettings.defaultFont;
                        textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                        textComponent.fontStyle = FontStyles.Normal;
                     }
                    break;
                case MenuItem.IType.TEXTSECTION:
                    {
                        newObject = GameObject.Instantiate(m_text, parentObject.transform);
                        TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
                        textComponent.text = ((Parameter<string>)item.Parameter).value;
                        textComponent.color = manager.uiAppearanceSettings.colors.FontColor;
                        textComponent.font = manager.uiAppearanceSettings.defaultFont;
                        textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                        textComponent.fontStyle = FontStyles.Bold;
                        textComponent.alignment = TextAlignmentOptions.Midline;
                        textComponent.enableWordWrapping = true;
                    }
                    break;
                case MenuItem.IType.PARAMETER:
                    switch (item.Parameter.vpetType) 
                    {
                        case AbstractParameter.ParameterType.ACTION:
                            {
                                newObject = GameObject.Instantiate(m_button, parentObject.transform);
                                Button button = newObject.GetComponent<Button>();                         
                                ColorBlock buttonColors = button.colors;
                                buttonColors.pressedColor = manager.uiAppearanceSettings.colors.ElementSelection_Highlight;
                                button.colors = buttonColors;
                                Action parameterAction = ((Parameter<Action>)item.Parameter).value;
                                button.onClick.AddListener(() => parameterAction());
                                TextMeshProUGUI textComponent = newObject.GetComponentInChildren<TextMeshProUGUI>();
                                textComponent.text = item.Parameter.name;
                                textComponent.color = manager.uiAppearanceSettings.colors.FontColor;                                
                                textComponent.font = manager.uiAppearanceSettings.defaultFont;
                                textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                Image imgButton = button.GetComponent<Image>();
                                imgButton.color = manager.uiAppearanceSettings.colors.ButtonBG;                                
                            }
                        break;
                        case AbstractParameter.ParameterType.BOOL:
                            {
                                newObject = GameObject.Instantiate(m_toggle, parentObject.transform);
                                Toggle toggle = newObject.GetComponent<Toggle>();
                                toggle.isOn = ((Parameter<bool>)item.Parameter).value;
                                toggle.onValueChanged.AddListener(delegate { ((Parameter<bool>)item.Parameter).setValue(toggle.isOn); });
                                Text textComponent = newObject.GetComponentInChildren<Text>();
                                textComponent.text = item.Parameter.name;
                                textComponent.color = manager.uiAppearanceSettings.colors.FontColor;
                                textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                ColorBlock toggleColors = toggle.colors;
                                toggleColors.normalColor = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                toggleColors.highlightedColor = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                toggleColors.pressedColor = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                toggleColors.selectedColor = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                toggleColors.disabledColor = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                toggle.colors = toggleColors;
                            }
                        break;
                        case AbstractParameter.ParameterType.FLOAT:
                            {
                                newObject = GameObject.Instantiate(m_numberInputField, parentObject.transform);
                                TMP_InputField numberInputField = newObject.GetComponent<TMP_InputField>();
                                numberInputField.text = ((Parameter<float>)item.Parameter).value.ToString();
                                numberInputField.onEndEdit.AddListener(delegate { ((Parameter<float>)item.Parameter).setValue(float.Parse(numberInputField.text)); });
                                Image imgButton = numberInputField.GetComponent<Image>();
                                imgButton.color = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                numberInputField.textComponent.color = manager.uiAppearanceSettings.colors.FontColor;
                                numberInputField.textComponent.font = manager.uiAppearanceSettings.defaultFont;
                                numberInputField.textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                numberInputField.caretPosition = numberInputField.text.Length;
                            }
                        break;
                        case AbstractParameter.ParameterType.STRING:
                            {
                                newObject = GameObject.Instantiate(m_inputField, parentObject.transform);
                                TMP_InputField inputField = newObject.GetComponent<TMP_InputField>();
                                inputField.text = ((Parameter<string>)item.Parameter).value;
                                inputField.onEndEdit.AddListener(delegate { ((Parameter<string>)item.Parameter).setValue(inputField.text); });
                                Image imgButton = inputField.GetComponent<Image>();
                                imgButton.color = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                                inputField.textComponent.color = manager.uiAppearanceSettings.colors.FontColor;
                                inputField.textComponent.font = manager.uiAppearanceSettings.defaultFont;
                                inputField.textComponent.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                inputField.selectionColor = manager.uiAppearanceSettings.colors.ElementSelection_Highlight;
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
                                dropDown.value = ((ListParameter)item.Parameter).value;
                                dropDown.onValueChanged.AddListener(delegate { ((ListParameter)item.Parameter).select(dropDown.value); });

                                dropDown.image.color = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;                                
                                dropDown.captionText.color = manager.uiAppearanceSettings.colors.FontColor;
                                dropDown.captionText.font = manager.uiAppearanceSettings.defaultFont;
                                dropDown.captionText.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                dropDown.itemText.color = manager.uiAppearanceSettings.colors.FontColor;
                                dropDown.itemText.font = manager.uiAppearanceSettings.defaultFont;
                                dropDown.itemText.fontSize = manager.uiAppearanceSettings.defaultFontSize;
                                ColorBlock ddColors = dropDown.colors;
                                ddColors.pressedColor = manager.uiAppearanceSettings.colors.ElementSelection_Highlight;
                                dropDown.colors = ddColors;
                                foreach (UnityEngine.UI.Image DD_image in dropDown.GetComponentsInChildren<Image>(true))
                                    DD_image.color = manager.uiAppearanceSettings.colors.DropDown_TextfieldBG;
                            }
                                break;
                    }
                break;
            }

            foreach (MenuItem p in item.Children)
                m_uiElements.Add(createMenufromTree(p, newObject));

            return newObject;
        }

        //!
        //! Function to destroy all created UI elements of a menu.
        //!
        private void hideMenu(object sender, EventArgs e)
        {
            m_oldMenu.visible = false;
            destroyMenu();
        }

        //!
        //! Function to destroy all created UI elements of a menu.
        //!
        private void destroyMenu()
        {
            foreach (GameObject uiElement in m_uiElements)
            {
                UnityEngine.Object.DestroyImmediate(uiElement);
            }
            m_uiElements.Clear();
        }
    }
}
