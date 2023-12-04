/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "SnapSelect.cs"
//! @brief implementation of scroll and snap functionality for Manipulators such as Spinner
//! @author Jonas Trottnow
//! @version 0
//! @date 02.02.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using TMPro;
using System;

namespace vpet
{
    public class SnapSelect : UIBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        //!
        //! unique element values selectable in the SnapSelect
        //!
        private List<float> _elementValues;

        //!
        //! non-unique list of elements
        //!
        private List<SnapSelectElement> _elements;

        //!
        //! is the SnapSelect scrollable vertically or horizontally?
        //!
        public bool _isVertical;

        //!
        //! should elements be looped infinitely
        //!
        public bool _loop = true;

        //!
        //! Should the SnapSelect throw events when the value of an axis is changed?
        //!
        public bool _allowValueSetting = false;

        //!
        //! amount of elements viewed as preview in each direction
        //!
        public int _previewExtend = 1;

        //!
        //! amount of fading being applied to preview Elements (multiplied with distance)
        //!
        public float _fadeFactor = 0.5f;

        //!
        //! The amount of elements that can be selected without sliding (similar to a usual menu)
        //!
        public float _selectableItems = 1;

        //!
        //! Can elments be clicked
        //!
        public bool _selectByClick = false;

        //!
        //! Can multiple elements be selected at once
        //!
        public bool _multiSelect = false;

        //!
        //! Is the menu collapsable
        //!
        public bool _collapsable = false;

        //!
        //! Is the menu collapsable
        //!
        public Sprite _collapsableIcon;

        //!
        //! Event emitted when parameter has changed
        //!
        public event EventHandler<int> parameterChanged;

        //!
        //! Event emitted when value has changed
        //!
        public event EventHandler<float> valueChanged;

        //!
        //! Event emitted when Axis dragging is in progress.
        //!
        public event EventHandler<bool> draggingAxis;

        //!
        //! Event emitted when elements shall update the higlighted status (All elements listen to this event)
        //!
        public event EventHandler<int> highlightElement;

        public event EventHandler<MenuButton.HighlightEventArgs> updateHighlightElement;

        //!
        //! Event emitted when editing of the current parameter ended (finger is lifted)
        //!
        public event EventHandler<bool> editingEnded;

        //!
        //! Event emitted when Axis dragging is in progress.
        //!
        public event EventHandler<int> elementClicked;

        //!
        //! Reference to text field for current value
        //!
        public TextMeshProUGUI _valueText;

        //!
        //! Reference to Prefab for background
        //!
        public GameObject _backgroundPrefab;

        //!
        //! Reference to VPET ui Settings
        //!
        public VPETUISettings _uiSettings;
        public VPETUISettings uiSettings
        {
            get => _uiSettings;
            set => _uiSettings = value;
        }

        //!
        //! Reference to VPET uiManager
        //!
        private UIManager _manager;
        public UIManager manager
        {
            set => _manager = value;
        }

        //!
        //! Is the menu fixed or dragable
        //!
        private bool _dragable = true;
        public bool dragable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dragable;
        }

        //!
        //! Reference to main SnapSelect UI Panel
        //!
        private RectTransform _contentMask;

        //!
        //! Reference to content UI Panel, containing all elements as childs
        //!
        private RectTransform _contentPanel;

        //!
        //! Reference to SnapSelect background
        //!
        private RectTransform background;

        //!
        //! Reference to arrows visualizing drag direction
        //!
        private RectTransform _arrows;

        //!
        //! amount of selectable elements in this SnapSelect
        //!
        private int _elementCount;

        //!
        //! The amount of elements that in the Menu
        //!
        private float _menuElementCount;

        //!
        //! is the SnapSelect already initialized?
        //!
        private bool _initialized = false;

        //!
        //! Size of each Element in the 2D UI
        //!
        private Vector2 _elementSize;

        //!
        //! is the axis currently being dragged Y or Y
        //!
        private bool _majorAxisX;

        //!
        //! Did the drag already decided which direction it is going to
        //!
        private bool _axisDecided;

        //!
        //! Canvas of SnapSelect in parent object
        //!
        private Canvas _canvas;

        //!
        //! Did the drag start
        //!
        private Vector2 _dragStart;

        //!
        //! which axis / element is currently active (edited)
        //!
        private int _currentAxis;
        public int currentAxis { get => _currentAxis; }

        //!
        //! sensitivity multiplicator
        //!
        private float _sensitivity;

        public int addElement(string caption, int buttonID)
        {
            return addElement(caption, 0f, null, false, buttonID);
        }

        public int addElement(Sprite sprite, int buttonID)
        {
            return addElement(sprite, 0f, null, false, buttonID);
        }

        public int addElement(string caption, Sprite sprite, int buttonID)
        {
            return addElement(caption,sprite, 0f, null, false, buttonID);
        }


        //!
        //!
        //!
        public int addElement(string caption, float initValue = 0f, Action action = null, bool isToogle = false, int buttonID = -1)
        {
            //add element to element list
            _elementValues.Add(initValue);
            if (_currentAxis != -1) 
                setText(_elementValues[_currentAxis]);

            //initialize elements
            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Element");

            int repetitions = _loop ? 3 : 1;

            for (int i = 0; i < repetitions; i++)
            {
                Transform elementTrans = SceneObject.Instantiate(elementPrefab, this.transform).transform;
                elementTrans.GetComponent<SnapSelectElement>().buttonID = (buttonID == -1) ? _elementCount : buttonID;
                elementTrans.GetComponent<SnapSelectElement>().index = _elementCount;
                elementTrans.GetComponent<SnapSelectElement>().clicked += handleClick;
                elementTrans.GetComponent<SnapSelectElement>().clickAction = action;
                elementTrans.GetComponent<SnapSelectElement>().isToggle = isToogle;

                elementTrans.name = caption;
                elementTrans.GetChild(0).GetComponent<TextMeshProUGUI>().text = caption;
                elementTrans.GetComponent<Image>().color = new Color(255, 255, 255, 0);

                _elements.Add(elementTrans.GetComponent<SnapSelectElement>());
            }

            _elementCount++;

            Create();

            return _elementCount - 1;
        }

        //!
        //!
        //!
        public int addElement(Sprite sprite, float initValue = 0f, Action action = null, bool isToogle = false, int buttonID = -1)
        {
            //add element to element list
            _elementValues.Add(initValue);
            if (_currentAxis != -1)
                setText(_elementValues[_currentAxis]);


            //initialize elements
            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Element");

            int repetitions = _loop ? 3 : 1;

            for (int i = 0; i < repetitions; i++)
            {
                Transform elementTrans = SceneObject.Instantiate(elementPrefab, this.transform).transform;
                elementTrans.GetComponent<SnapSelectElement>().index = _elementCount;
                elementTrans.GetComponent<SnapSelectElement>().buttonID = (buttonID == -1)? _elementCount : buttonID;
                elementTrans.GetComponent<SnapSelectElement>().clicked += handleClick;
                elementTrans.GetComponent<SnapSelectElement>().clickAction = action;
                elementTrans.GetComponent<SnapSelectElement>().isToggle = isToogle;

                elementTrans.name = sprite.name;
                elementTrans.GetComponent<Image>().sprite = sprite;

                _elements.Add(elementTrans.GetComponent<SnapSelectElement>());
            }

            _elementCount++;

            Create();

            return _elementCount - 1;
        }

        //!
        //!
        //!
        public int addElement(string caption, Sprite sprite, float initValue = 0f, Action action = null, bool isToogle = false, int buttonID = -1)
        {
            //add element to element list
            _elementValues.Add(initValue);
            if (_currentAxis != -1)
                setText(_elementValues[_currentAxis]);


            //initialize elements
            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Element");

            int repetitions = _loop ? 3 : 1;

            for (int i = 0; i < repetitions; i++)
            {
                Transform elementTrans = SceneObject.Instantiate(elementPrefab, this.transform).transform;
                elementTrans.GetComponent<SnapSelectElement>().index = _elementCount;
                elementTrans.GetComponent<SnapSelectElement>().buttonID = (buttonID == -1) ? _elementCount : buttonID;
                elementTrans.GetComponent<SnapSelectElement>().clicked += handleClick;
                elementTrans.GetComponent<SnapSelectElement>().clickAction = action;
                elementTrans.GetComponent<SnapSelectElement>().isToggle = isToogle;

                elementTrans.name = caption;

                elementTrans.GetChild(0).GetComponent<TextMeshProUGUI>().text = caption;
                elementTrans.GetComponent<Image>().sprite = sprite;
                _elements.Add(elementTrans.GetComponent<SnapSelectElement>());
            }

            _elementCount++;

            Create();

            return _elementCount - 1;
        }

        //!
        //! adjust the sensitvity for parameter editing
        //! @param sensitivityIn sensitivity multiplicator for the elements
        //!
        public void setSensitivity(float sensitivityIn)
        {
            _sensitivity = sensitivityIn;
        }

        //!
        //! Unity Awake function
        //!
        protected override void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_collapsable)
            {
                Image img = this.gameObject.AddComponent<Image>();
                if (_collapsableIcon)
                {
                    img.sprite = _collapsableIcon;
                }
                Button button = this.gameObject.AddComponent<Button>();
                button.onClick.AddListener(showHide) ;
            }
            _elementValues = new List<float>();
            _elements = new List<SnapSelectElement>();
            background = null;
            _contentMask = null;
            _arrows = null;
            if (_selectByClick)
                _currentAxis = -1;
            else
            {
                _currentAxis = 0;
                parameterChanged?.Invoke(this, 0);
            }
            _elementCount = 0;
            _menuElementCount = 0;
        }

        //!
        //! Init function of the SnapSelect that needs to be called manually before any elements apear
        //!
        private void Create()
        {
            //setup overall layout
            _elementSize = this.GetComponent<RectTransform>().sizeDelta;
            if (_selectableItems < 0)
            {
                _menuElementCount = _elementCount;
                _dragable = false;
            }
            else
            {
                _menuElementCount = (_elementCount < _selectableItems) ? _elementCount : _selectableItems;
                _dragable = _elementCount > _selectableItems;
            }


            if (_backgroundPrefab & background == null)
            {
                background = SceneObject.Instantiate(_backgroundPrefab, this.transform).GetComponent<RectTransform>();
            }


            if (_contentMask == null)
            {
                GameObject contentFramePrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_ContentFrame");
                _contentMask = SceneObject.Instantiate(contentFramePrefab, this.transform).GetComponent<RectTransform>();
                _contentMask.GetComponent<RectMask2D>().softness = Vector2Int.RoundToInt(_elementSize * (_previewExtend * 2.0f));
                if (_isVertical)
                    _contentMask.GetComponent<RectMask2D>().softness = new Vector2Int(0, _contentMask.GetComponent<RectMask2D>().softness.y);
                else
                    _contentMask.GetComponent<RectMask2D>().softness = new Vector2Int(_contentMask.GetComponent<RectMask2D>().softness.y, 0);

                _contentPanel = _contentMask.GetChild(0).GetComponent<RectTransform>();
            }

            if (_valueText)
                _valueText.gameObject.SetActive(_allowValueSetting);


            if (_isVertical)
            {
                if (!_contentPanel.gameObject.GetComponent<VerticalLayoutGroup>())
                {
                    VerticalLayoutGroup vG = _contentPanel.gameObject.AddComponent<VerticalLayoutGroup>();
                    vG.childControlHeight = false;
                    vG.childControlWidth = false;
                    vG.childForceExpandWidth = false;
                    vG.childForceExpandHeight = false;
                }
                switchToVerticalAlign(_contentMask);
                switchToVerticalAlign(_contentPanel);

                if (background)
                {
                    switchToVerticalAlign(background);
                }
            }
            else
            {
                if (!_contentPanel.gameObject.GetComponent<HorizontalLayoutGroup>())
                {
                    HorizontalLayoutGroup hG = _contentPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hG.childControlHeight = false;
                    hG.childControlWidth = false;
                    hG.childForceExpandWidth = false;
                    hG.childForceExpandHeight = false;
                }
            }

            _contentMask.sizeDelta = multiplyAlignedVector(_elementSize, true, (_previewExtend * 2 + _menuElementCount));
            _contentMask.anchoredPosition = multiplyAlignedVector(_elementSize, false, _previewExtend, true);
            _contentPanel.sizeDelta = multiplyAlignedVector(_elementSize, true, _elementCount * (_loop ? 3 : 1));
            _contentPanel.anchoredPosition = multiplyAlignedVector(_elementSize, false, -_previewExtend + (_elementCount * (_loop ? 1 : 0)), true);
            if (background)
                background.sizeDelta = multiplyAlignedVector(_elementSize, true, _menuElementCount);

            if (_collapsable)
            {
                if (_isVertical)
                {
                    _contentMask.anchoredPosition -= new Vector2(0f, _elementSize.y * (1 + _previewExtend));
                    //_contentPanel.anchoredPosition -= new Vector2(0f, _elementSize.y * (1 + _previewExtend));
                    background.anchoredPosition = new Vector2(0f, -_elementSize.y * (1 + _previewExtend));
                }
                else
                {
                    _contentMask.anchoredPosition += new Vector2(_elementSize.x * (1 + _previewExtend), 0f);
                    //_contentPanel.anchoredPosition += new Vector2(_elementSize.x * (1 + _previewExtend), 0f);
                    background.anchoredPosition = new Vector2(_elementSize.x * (1 + _previewExtend), 0f);
                }
                background.gameObject.SetActive(false);
                _contentMask.gameObject.SetActive(false);
            }

            if (_allowValueSetting && _arrows == null)
            {
                GameObject arrowsPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Arrows");
                _arrows = SceneObject.Instantiate(arrowsPrefab, _contentMask).GetComponent<RectTransform>();
                _arrows.localRotation = Quaternion.Euler(0, 0, _isVertical? 90 : 0);
                _arrows.anchoredPosition = Vector2.zero;
                _arrows.SetParent(this.transform,true);
            }

            int repetitions = _loop ? 3 : 1;
            int counter = 0;
            for (int r = 0; r < repetitions; r++)
                for (int e = 0; e < _elementValues.Count; e++)
                {
                    SnapSelectElement element = _elements[e * repetitions + r];
                    if (_isVertical)
                        switchToVerticalAlign(element.GetComponent<RectTransform>());
                    element.transform.SetParent(_contentPanel);
                    //fix for sorted ordering, defines expicit order of ContentPanel childs
                    element.transform.SetSiblingIndex(counter++);
                    element.GetComponent<RectTransform>().sizeDelta = _elementSize;
                    //element.GetComponent<RectTransform>().anchoredPosition = multiplyAlignedVector(_elementSize, false, -(0.1f + r * _elementValues.Count + e), true);
                }

            if(_elementCount == 1 && !_selectByClick)
                showHighlighted(0, true);

            _initialized = true;
        }

        //!
        //! Switch a RectTransform to vertical alignment (origin moves to top center)
        //!
        private void switchToVerticalAlign(RectTransform t)
        {
            t.anchorMin = new Vector2(0.5f, 1f);
            t.anchorMax = new Vector2(0.5f, 1f);
            t.pivot = new Vector2(0.5f, 1f);
        }

        //!
        //!
        //!
        private Vector2 multiplyAlignedVector(Vector2 vecIn, bool keepValueA, float newValueB, bool negateValueHorizontal = false)
        {
            Vector2 vecOut;
            int keepA = keepValueA ? 1 : 0;
            int negateHorizontal = negateValueHorizontal ? -1 : 1;
            if (_isVertical)
            {
                vecOut = new Vector2(vecIn.x * keepA, vecIn.y * newValueB);
            }
            else
            {
                vecOut = new Vector2(vecIn.x * negateHorizontal * newValueB, vecIn.y * keepA);
            }
            return vecOut;
        }

        //!
        //!
        //!
        public void showHide()
        {
            if (_collapsable)
            {
                if (_contentMask.gameObject.activeSelf)
                {
                    _contentMask.gameObject.SetActive(false);
                    background.gameObject.SetActive(false);
                }
                else
                {
                    _contentMask.gameObject.SetActive(true);
                    background.gameObject.SetActive(true);
                }
            }
        }

        //!
        //! Function called when an element was clicked
        //! @param sender Sender of the event
        //! @param e SnapSelectElement that has been clicked
        //!
        private void handleClick(object sender, SnapSelectElement e)
        {
            if (_manager.ui2Dinteractable)
            {
                if (_selectByClick && !_axisDecided)
                {
                    _currentAxis = e.index;
                    setText(_elementValues[_currentAxis]);
                    parameterChanged?.Invoke(this, _currentAxis);
                    elementClicked?.Invoke(this, e.buttonID);
                    highlightElement?.Invoke(this, e.index);
                }
                else if (!_axisDecided)
                {
                    if (e.index != _currentAxis)
                    {
                        int direction = 0;
                        if (e.index == (_currentAxis + 1) % _elementCount)
                            direction = -1;
                        else if (e.index == (_elementCount + _currentAxis - 1) % _elementCount)
                            direction = 1;
                        Vector2 contentPos = _contentPanel.GetComponent<RectTransform>().anchoredPosition;
                        if (_isVertical)
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, (Mathf.Round(contentPos.y / _elementSize.y) + direction) * _elementSize.y);
                        }
                        else
                        {
                            _contentPanel.anchoredPosition = new Vector2((Mathf.Round(contentPos.x / _elementSize.x) + direction) * _elementSize.x, contentPos.y);
                        }
                        contentPos = _contentPanel.anchoredPosition;
                        showHighlighted(e.index);
                        _currentAxis = e.index;
                        parameterChanged?.Invoke(this, _currentAxis);
                        setText(_elementValues[_currentAxis]);
                        _axisDecided = false;

                        //realize infinit looping
                        if (_initialized && _loop)
                        {
                            if (_isVertical)
                            {
                                if (contentPos.y > _elementSize.y * (-_previewExtend + (2 * _elementCount)))
                                {
                                    _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y - (_elementSize.y * _elementCount));
                                }
                                if (contentPos.y < _elementSize.y * (-_previewExtend + _elementCount))
                                {
                                    _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + (_elementSize.y * _elementCount));
                                }
                            }
                            else
                            {
                                if (contentPos.x > _elementSize.x * (_previewExtend - _elementCount))
                                {
                                    _contentPanel.anchoredPosition = new Vector2(contentPos.x - (_elementSize.x * _elementCount), contentPos.y);
                                }
                                if (contentPos.x < _elementSize.x * (_previewExtend - (2 * _elementCount)))
                                {
                                    _contentPanel.anchoredPosition = new Vector2(contentPos.x + (_elementSize.x * _elementCount), contentPos.y);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void updateHighlight(object sender, MenuButton.HighlightEventArgs e)
        {
            updateHighlightElement?.Invoke(this, e);
        }

        public void showHighlighted(int id, bool force = false)
        {
            if(force || _currentAxis != id)
                highlightElement?.Invoke(this, id);
        }

        public void resetHighlighting(object sender, EventArgs e)
        {
            highlightElement?.Invoke(this, -1);
        }

        //!
        //! Function receiving an updated float when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param f float to be set
        //!
        public void setParam(object sender, float f)
        {
            _elementValues[0] = f;
            setText(_elementValues[_currentAxis]);
        }

        //!
        //! Function receiving an updated Vector2 when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param v2 Vector2 to be set
        //!
        public void setParam(object sender, Vector2 v2 )
        {
            _elementValues[0] = v2.x;
            _elementValues[1] = v2.y;
            setText(_elementValues[_currentAxis]);
        }

        //!
        //! Set text of parameter textbox if present
        //! @param t new text value
        //!
        private void setText(float t)
        {
            if (_valueText)
                _valueText.text = t.ToString("N2");

        }

        //!
        //! Function receiving an updated Vector3 when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param v3 Vector3 to be set
        //!
        public void setParam(object sender, Vector3 v3)
        {
            _elementValues[0] = v3.x;
            _elementValues[1] = v3.y;
            _elementValues[2] = v3.z;
            _elementValues[3] = (v3.x + v3.y + v3.z) / 3f;
            setText(_elementValues[_currentAxis]);
        }

        //!
        //! Function receiving an updated quaternion when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param q quaternion to be set
        //!
        public void setParam(object sender, Quaternion q)
        {
            Vector3 rot = q.eulerAngles;
            _elementValues[0] = rot.x;
            _elementValues[1] = rot.y;
            _elementValues[2] = rot.z;
            setText(_elementValues[_currentAxis]);
        }

        //!
        //! Unity function called by IBeginDragHandler when a drag starts
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnBeginDrag(PointerEventData data)
        {
            if (_manager.ui2Dinteractable)
            {
                _dragStart = data.position;
                _axisDecided = false;
            }
        }

        //!
        //! Unity function called by IDragHandler when a drag is currently performed
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnDrag(PointerEventData data)
        {
            if (_manager.ui2Dinteractable)
            {
                Vector2 contentPos = _contentPanel.anchoredPosition;
                float scale = 1f / _canvas.scaleFactor;


                if (!_axisDecided && Vector2.Distance(_dragStart, data.position) > (_isVertical ? (_contentMask.sizeDelta.y / 8f) : (_contentMask.sizeDelta.x / 8f)))
                {
                    _majorAxisX = Mathf.Abs(_dragStart.x - data.position.x) > Mathf.Abs(_dragStart.y - data.position.y);
                    _axisDecided = true;
                }

                if (_axisDecided)
                {
                    if (_isVertical)
                    {
                        if (_majorAxisX)
                        {
                            //adjust Parameter
                            if (_allowValueSetting)
                                valueChanged?.Invoke(this, (data.delta.x / Screen.width) * _sensitivity);
                        }
                        else if (_dragable)
                        {
                            if (!_loop && (_contentPanel.anchoredPosition.y > (_previewExtend + 0.4f) * _elementSize.y))
                            {
                                Debug.Log("A");
                                _contentPanel.anchoredPosition = new Vector2(contentPos.x, (_previewExtend + 0.4f) * _elementSize.y * scale);
                            }
                            else if (!_loop && (_contentPanel.anchoredPosition.y < -_contentPanel.sizeDelta.y + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.y))
                            {
                                Debug.Log("B");
                                _contentPanel.anchoredPosition = new Vector2(contentPos.x, -_contentPanel.sizeDelta.y + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.y * scale);
                            }
                            else if (_loop || ((_contentPanel.anchoredPosition.y < (_previewExtend + 0.4f) * _elementSize.y)
                                        && (_contentPanel.anchoredPosition.y > -_contentPanel.sizeDelta.y + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.y)))
                            {
                                Debug.Log("C");
                                _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + data.delta.y * scale);
                            }
                            draggingAxis?.Invoke(this, true);

                        }
                    }
                    else
                    {
                        if (!_majorAxisX)
                        {
                            //adjust Parameter
                            if (_allowValueSetting)
                                valueChanged?.Invoke(this, (_majorAxisX ? data.delta.x / Screen.width : data.delta.y / Screen.height) * _sensitivity);
                        }
                        else if (_dragable)
                        {
                            if (!_loop && (_contentPanel.anchoredPosition.x > (_previewExtend + 0.4f) * _elementSize.x))
                            {
                                _contentPanel.anchoredPosition = new Vector2((_previewExtend + 0.4f) * _elementSize.x * scale, contentPos.y);
                            }
                            else if (!_loop && (_contentPanel.anchoredPosition.x < -_contentPanel.sizeDelta.x + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.x))
                            {
                                _contentPanel.anchoredPosition = new Vector2(-_contentPanel.sizeDelta.x + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.x * scale, contentPos.y);
                            }
                            else if (_loop || (_contentPanel.anchoredPosition.x < (_previewExtend + 0.4f) * _elementSize.x)
                                        && (_contentPanel.anchoredPosition.x > -_contentPanel.sizeDelta.x + (_previewExtend + _menuElementCount - 0.4f) * _elementSize.x))
                            {
                                _contentPanel.anchoredPosition = new Vector2(contentPos.x + data.delta.x * scale, contentPos.y);
                            }
                            draggingAxis?.Invoke(this, true);
                        }
                    }
                }

                //realize infinit looping
                if (_initialized && _loop && _dragable)
                {
                    if (_isVertical)
                    {
                        if (contentPos.y > _elementSize.y * (-_previewExtend + (2 * _elementCount)))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y - (_elementSize.y * _elementCount));
                        }
                        if (contentPos.y < _elementSize.y * (-_previewExtend + _elementCount))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + (_elementSize.y * _elementCount));
                        }
                    }
                    else
                    {
                        if (contentPos.x > _elementSize.x * (_previewExtend - _elementCount))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x - (_elementSize.x * _elementCount), contentPos.y);
                        }
                        if (contentPos.x < _elementSize.x * (_previewExtend - (2 * _elementCount)))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x + (_elementSize.x * _elementCount), contentPos.y);
                        }
                    }
                }
            }
        }

        //!
        //! Unity function called by IEndDragHandler when a drag ends
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnEndDrag(PointerEventData data)
        {
            if (_manager.ui2Dinteractable)
            {
                int newAxis;
                if (_axisDecided /*&& _dragable*/ && ((_isVertical && !_majorAxisX) || (!_isVertical && _majorAxisX)))
                {
                    Vector2 contentPos = _contentPanel.GetComponent<RectTransform>().anchoredPosition;
                    if (_isVertical)
                    {
                        _contentPanel.anchoredPosition = new Vector2(contentPos.x, Mathf.Round(contentPos.y / _elementSize.y) * _elementSize.y);
                        newAxis = Mathf.FloorToInt(((_contentPanel.anchoredPosition.y / _elementSize.y) + 1) % (_elementCount));
                    }
                    else
                    {
                        _contentPanel.anchoredPosition = new Vector2(Mathf.Round(contentPos.x / _elementSize.x) * _elementSize.x, contentPos.y);
                        newAxis = Mathf.FloorToInt((-(_contentPanel.anchoredPosition.x / _elementSize.x) + 1) % (_elementCount));
                    }
                    if (newAxis < 0)
                        newAxis = _elementCount + _currentAxis;
                    if (!_selectByClick)
                        showHighlighted(newAxis);
                    _currentAxis = newAxis;
                    parameterChanged?.Invoke(this, _currentAxis);
                    setText(_elementValues[_currentAxis]);
                }
                else if (_axisDecided)
                {
                    editingEnded?.Invoke(this, true);
                }
                _axisDecided = false;
                draggingAxis?.Invoke(this, true);
            }
        }
    }
}