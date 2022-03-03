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
        //! elements selectable in the SnapSelect
        //!
        private List<float> _elements;

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
        //! The amount of elements that can be selected without sliding (similar to a usual menu)
        //!
        public bool _selectByClick = false;

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
        //! Reference to arrows visualizing drag direction
        //!
        private RectTransform _arrows;

        //!
        //! amount of selectable elements in this SnapSelect
        //!
        private int _elementCount;

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
        //! Did the drag start
        //!
        private Vector2 _dragStart;

        //!
        //! which axis / element is currently active (edited)
        //!
        private int _currentAxis;

        //!
        //! sensitivity multiplicator
        //!
        private float _sensitivity;

        //!
        //! Init function of the SnapSelect that needs to be called manually before any elements apear
        //! Note that this initialization will automatically disable allowValueSetting
        //! @param elementNames This is the name displayed in the UI
        //!
        public void Init(List<string> elementNames)
        {
            _allowValueSetting = false;
            List<Tuple<float, string>> list = new List<Tuple<float, string>>();
            foreach (string s in elementNames)
                list.Add(new Tuple<float, string>(0f, s));
            Init(list, 0f);
        }

        //!
        //! Init function of the SnapSelect that needs to be called manually before any elements apear
        //! @param elementTupels Tupel value of elements to add to the SnapSelect (this are usually the axis), first value is the inital value and second the name displayed in the UI
        //! @param sensitivityIn sensitivity multiplicator for the elements
        //!
        public void Init(List<Tuple<float, string>> elementTupels, float sensitivityIn)
        {
            _elements = new List<float>();
            _elementSize = this.GetComponent<RectTransform>().sizeDelta;

            RectTransform background = null;
            if (_backgroundPrefab)
                background = SceneObject.Instantiate(_backgroundPrefab, this.transform).GetComponent<RectTransform>();


            GameObject contentFramePrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_ContentFrame");
            _contentMask = SceneObject.Instantiate(contentFramePrefab, this.transform).GetComponent<RectTransform>();
            _contentPanel = _contentMask.GetChild(0).GetComponent<RectTransform>();

            _sensitivity = sensitivityIn;

            _elementCount = elementTupels.Count;
            _currentAxis = 0;

            _dragable = _elementCount > _selectableItems;

            if (_valueText)
                _valueText.gameObject.SetActive(_allowValueSetting);


            if (_isVertical)
            {
                _contentMask.anchorMin = new Vector2(0.5f, 1f);
                _contentMask.anchorMax = new Vector2(0.5f, 1f);
                _contentMask.pivot = new Vector2(0.5f, 1f);
                _contentPanel.anchorMin = new Vector2(0.5f, 1f);
                _contentPanel.anchorMax = new Vector2(0.5f, 1f);
                _contentPanel.pivot = new Vector2(0.5f, 1f);

                if (background)
                {
                    background.anchorMin = new Vector2(0.5f, 1f);
                    background.anchorMax = new Vector2(0.5f, 1f);
                    background.pivot = new Vector2(0.5f, 1f);
                    background.sizeDelta = new Vector2(_elementSize.x, _elementSize.y * _selectableItems);
                }
                _contentMask.sizeDelta = new Vector2(_elementSize.x, _elementSize.y * (_previewExtend * 2 + _selectableItems));
                _contentMask.anchoredPosition = new Vector2(0, -_elementSize.y * _previewExtend);


                /*if (_allowValueSetting)
                {
                    GameObject arrowsPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Arrows");
                    _arrows = SceneObject.Instantiate(arrowsPrefab, this.transform).GetComponent<RectTransform>();
                    _arrows.anchorMin = new Vector2(0.5f, 1f);
                    _arrows.anchorMax = new Vector2(0.5f, 1f);
                    _arrows.pivot = new Vector2(0.5f, 1f);
                    _arrows.localRotation = Quaternion.Euler(0, 0, 90);
                    _arrows.anchoredPosition = new Vector2(0f, -(_elementSize.y * (_selectableItems-1)) / 2f);
                }*/
                if (_loop)
                {
                    _contentPanel.sizeDelta = new Vector2(_elementSize.x, _elementSize.y * _elementCount * 3);
                    _contentPanel.anchoredPosition = new Vector2(0, _elementSize.y * (_previewExtend - _elementCount));
                }
                else
                {
                    _contentPanel.sizeDelta = new Vector2(_elementSize.x, _elementSize.y * _elementCount);
                    _contentPanel.anchoredPosition = new Vector2(0, _elementSize.y * _previewExtend);
                }
            }
            else
            {
                if (background)
                    background.sizeDelta = new Vector2(_elementSize.x * _selectableItems, _elementSize.y);
                _contentMask.sizeDelta = new Vector2(_elementSize.x * (_previewExtend * 2 + _selectableItems), _elementSize.y);
                _contentMask.anchoredPosition = new Vector2(-_elementSize.y * _previewExtend, 0);
                if (_allowValueSetting)
                {
                    GameObject arrowsPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Arrows");
                    _arrows = SceneObject.Instantiate(arrowsPrefab, this.transform).GetComponent<RectTransform>();
                    _arrows.localRotation = Quaternion.Euler(0, 0, 0);
                    _arrows.anchoredPosition = new Vector2((_elementSize.x*(_selectableItems-1)) / 2f, 0f);
                }
                if (_loop)
                {
                    _contentPanel.sizeDelta = new Vector2(_elementSize.x * _elementCount * 3, _elementSize.y);
                    _contentPanel.anchoredPosition = new Vector2(_elementSize.x * (_previewExtend - _elementCount), 0);
                }
                else
                {
                    _contentPanel.sizeDelta = new Vector2(_elementSize.x * _elementCount, _elementSize.y);
                    _contentPanel.anchoredPosition = new Vector2(_elementSize.x * _previewExtend, 0);
                }

            }

            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/SnapSelectParts/PRE_Element");

            int repetitions = 1;
            if (_loop)
            {
                repetitions = 3;
            }

            int elementPos = 0;
            for (int i = 0; i < repetitions; i++)
            {
                foreach (Tuple<float, string> elementTupel in elementTupels)
                {
                    Transform elementTrans = SceneObject.Instantiate(elementPrefab).transform;
                    if (_isVertical)
                    {
                        elementTrans.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                        elementTrans.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                        elementTrans.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                    }
                    elementTrans.SetParent(_contentPanel, false);
                    elementTrans.GetComponent<SnapSelectElement>().index = elementPos % elementTupels.Count;
                    elementTrans.GetComponent<SnapSelectElement>().clicked += handleClick;
                    elementTrans.GetComponent<RectTransform>().sizeDelta = _elementSize * 0.8f;

                    elementTrans.name = elementTupel.Item2;
                    elementTrans.GetComponent<TextMeshProUGUI>().text = elementTupel.Item2;
                    if (_isVertical)
                    {
                        elementTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -((_elementSize.y * 0.1f) + elementPos * _elementSize.y));
                    }
                    else
                    {
                        elementTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2((_elementSize.x * 0.1f) + elementPos * _elementSize.x, 0);
                    }
                    if(i == 0)
                        _elements.Add(elementTupel.Item1);
                    elementPos++;
                }
            }

            setText(elementTupels[0].Item1);
            _initialized = true;
        }

        //!
        //! Function called when an element was clicked
        //! @param sender Sender of the event
        //! @param e SnapSelectElement that has been clicked
        //!
        private void handleClick(object sender, SnapSelectElement e)
        {
            if (_selectByClick && !_axisDecided)
            {
                if (elementClicked != null)
                {
                    _currentAxis = e.index;
                    setText(_elements[_currentAxis]);
                    parameterChanged.Invoke(this, _currentAxis);
                    elementClicked.Invoke(this, e.index);
                }
            }

        }

        //!
        //! Function receiving an updated float when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param f float to be set
        //!
        public void setParam(object sender, float f)
        {
            _elements[0] = f;
            setText(_elements[_currentAxis]);
        }

        //!
        //! Function receiving an updated Vector2 when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param v2 Vector2 to be set
        //!
        public void setParam(object sender, Vector2 v2 )
        {
            _elements[0] = v2.x;
            _elements[1] = v2.y;
            setText(_elements[_currentAxis]);
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
            _elements[0] = v3.x;
            _elements[1] = v3.y;
            _elements[2] = v3.z;
            _elements[3] = (v3.x + v3.y + v3.z) / 3f;
            setText(_elements[_currentAxis]);

        }

        //!
        //! Function receiving an updated quaternion when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param q quaternion to be set
        //!
        public void setParam(object sender, Quaternion q)
        {
            Vector3 rot = q.eulerAngles;
            _elements[0] = rot.x;
            _elements[1] = rot.y;
            _elements[2] = rot.z;
            setText(_elements[_currentAxis]);
        }

        //!
        //! Unity function called by IBeginDragHandler when a drag starts
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnBeginDrag(PointerEventData data)
        {
            _dragStart = data.position;
            _axisDecided = false;
        }

        //!
        //! Unity function called by IDragHandler when a drag is currently performed
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnDrag(PointerEventData data)
        {
            Vector2 contentPos = _contentPanel.anchoredPosition;

            if (!_axisDecided && Vector2.Distance(_dragStart, data.position) > _contentMask.sizeDelta.x / 8f)
            {
                _majorAxisX = Mathf.Abs(_dragStart.x - data.position.x) > Mathf.Abs(_dragStart.y - data.position.y);
                _axisDecided = true;
            }
            if(_axisDecided)
            {
                if (_isVertical)
                    if (_majorAxisX)
                    {
                        //adjust Parameter
                        if (_allowValueSetting)
                            valueChanged.Invoke(this, (data.delta.x / Screen.width) * _sensitivity);
                    }
                    else
                    { 
                        if (!_loop && (_contentPanel.anchoredPosition.y > (_previewExtend + 0.4f) * _elementSize.y))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, (_previewExtend + 0.4f) * _elementSize.y);
                        }
                        else if (!_loop && (_contentPanel.anchoredPosition.y < -_contentPanel.sizeDelta.y + (_previewExtend + _selectableItems - 0.4f) * _elementSize.y))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, -_contentPanel.sizeDelta.y + (_previewExtend + _selectableItems - 0.4f) * _elementSize.y);
                        }
                        else if (_loop || ( _dragable
                                && ((_contentPanel.anchoredPosition.y < (_previewExtend + 0.4f) * _elementSize.y)
                                    && (_contentPanel.anchoredPosition.y > -_contentPanel.sizeDelta.y + (_previewExtend + _selectableItems - 0.4f) * _elementSize.y))))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + data.delta.y);
                        }
                        draggingAxis.Invoke(this, true);

                    }
                else
                {
                    if (_majorAxisX)
                    {
                        if (!_loop && (_contentPanel.anchoredPosition.x > (_previewExtend + 0.4f) * _elementSize.x))
                        {
                            _contentPanel.anchoredPosition = new Vector2((_previewExtend + 0.4f) * _elementSize.x, contentPos.y);
                        }
                        else if (!_loop && (_contentPanel.anchoredPosition.x < -_contentPanel.sizeDelta.x + (_previewExtend + _selectableItems - 0.4f) * _elementSize.x))
                        {
                            _contentPanel.anchoredPosition = new Vector2(-_contentPanel.sizeDelta.x + (_previewExtend + _selectableItems - 0.4f) * _elementSize.x, contentPos.y);
                        }
                        else if (_loop || (_dragable
                                && ((_contentPanel.anchoredPosition.x < (_previewExtend + 0.4f) * _elementSize.x)
                                    && (_contentPanel.anchoredPosition.x > -_contentPanel.sizeDelta.x + (_previewExtend + _selectableItems - 0.4f) * _elementSize.x))))
                        {
                            _contentPanel.anchoredPosition = new Vector2(contentPos.x + data.delta.x, contentPos.y);
                        }
                        draggingAxis.Invoke(this, true);

                    }
                    else
                    {
                        //adjust Parameter
                        if(_allowValueSetting)
                            valueChanged.Invoke(this, (data.delta.y / Screen.height) * _sensitivity);
                    }
                }
            }

            //realize infinit looping
            if (_initialized && _loop)
            {
                if (_isVertical)
                {
                    if (contentPos.y > _elementSize.y * (_previewExtend - _elementCount))
                    {
                        _contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y - (_elementSize.y * _elementCount));
                    }
                    if (contentPos.y < _elementSize.y * (_previewExtend - (2 * _elementCount)))
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

        //!
        //! Unity function called by IEndDragHandler when a drag ends
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnEndDrag(PointerEventData data)
        {
            if (_axisDecided && ((_isVertical && !_majorAxisX) || (!_isVertical && _majorAxisX)))
            {
                Vector2 contentPos = _contentPanel.GetComponent<RectTransform>().anchoredPosition;
                if (_isVertical)
                {
                    _contentPanel.anchoredPosition = new Vector2(contentPos.x, Mathf.Round(contentPos.y / _elementSize.y)* _elementSize.y);
                    _currentAxis = Mathf.FloorToInt((-(_contentPanel.anchoredPosition.y / _elementSize.y)+1) % (_elementCount));
                }
                else
                {
                    _contentPanel.anchoredPosition = new Vector2(Mathf.Round(contentPos.x / _elementSize.x) * _elementSize.x, contentPos.y);
                    _currentAxis = Mathf.FloorToInt((-(_contentPanel.anchoredPosition.x / _elementSize.x)+1) % (_elementCount));
                }
                if (_currentAxis < 0)
                    _currentAxis = _elementCount + _currentAxis;
                parameterChanged.Invoke(this, _currentAxis);
                setText(_elements[_currentAxis]);
            }
            _axisDecided = false;
            draggingAxis.Invoke(this, true);
        }
    }
}