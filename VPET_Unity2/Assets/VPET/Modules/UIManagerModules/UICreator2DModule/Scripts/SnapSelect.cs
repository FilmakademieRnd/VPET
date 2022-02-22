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
        List<float> elements;

        //!
        //! is the SnapSelect scrollable vertically or horizontally?
        //!
        public bool isVertical;

        //!
        //! should elements be looped infinitely
        //!
        public bool loop;

        //!
        //! Should the SnapSelect throw events when the value of an axis is changed?
        //!
        public bool allowValueSetting;

        //!
        //! amount of elements viewed as preview in each direction
        //!
        public int previewExtend;

        //!
        //! amount of fading being applied to preview Elements (multiplied with distance)
        //!
        public float fadeFactor;

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
        //! Reference to main SnapSelect UI Panel
        //!
        private RectTransform mainPanel;

        //!
        //! Reference to content UI Panel, containing all elements as childs
        //!
        private RectTransform contentPanel;

        //!
        //! Reference to arrows visualizing drag direction
        //!
        private RectTransform arrows;

        //!
        //! Reference to text for current value of
        //!
        private TextMeshProUGUI valueText;

        //!
        //! amount of selectable elements in this SnapSelect
        //!
        private int elementCount;

        //!
        //! is the SnapSelect already initialized?
        //!
        private bool initialized = false;

        //!
        //! Size of each Element in the 2D UI
        //!
        private Vector2 elementSize;

        //!
        //! is the axis currently being dragged Y or Y
        //!
        private bool majorAxisX;

        //!
        //! Did the drag already decided which direction it is going to
        //!
        private bool axisDecided;

        //!
        //! Did the drag start
        //!
        private Vector2 dragStart;

        //!
        //! which axis / element is currently active (edited)
        //!
        private int currentAxis;

        //!
        //! sensitivity multiplicator
        //!
        private float sensitivity;

        //!
        //! Init function of the SnapSelect that needs to be called manually before any elements apear
        //! @param elementTupels Tupel value of elements to add to the SnapSelect (this are usually the axis), first value is the inital value and second the name displayed in the UI
        //! @param sensitivityIn sensitivity multiplicator for the elements
        //!
        public void Init(List<Tuple<float, string>> elementTupels, float sensitivityIn)
        {
            elements = new List<float>();
            elementSize = this.GetComponent<RectTransform>().sizeDelta;
            mainPanel = this.transform.GetChild(1).GetComponent<RectTransform>();
            contentPanel = mainPanel.GetChild(0).GetComponent<RectTransform>();
            arrows = this.transform.GetChild(2).GetComponent<RectTransform>();
            valueText = this.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            sensitivity = sensitivityIn;

            elementCount = elementTupels.Count;
            currentAxis = 0;

            arrows.gameObject.SetActive(allowValueSetting);
            valueText.gameObject.SetActive(allowValueSetting);


            if (isVertical)
            {
                mainPanel.sizeDelta = new Vector2(elementSize.x, elementSize.y * (previewExtend * 2 + 1));
                arrows.localRotation = Quaternion.Euler(0, 0, 90);
                if(loop)
                    contentPanel.sizeDelta = new Vector2(elementSize.x, elementSize.y * elementCount * 3);
                else
                    contentPanel.sizeDelta = new Vector2(elementSize.x, elementSize.y * elementCount);
                contentPanel.localPosition = new Vector2(0, elementSize.y);
            }
            else
            {
                mainPanel.sizeDelta = new Vector2(elementSize.x * (previewExtend * 2 + 1), elementSize.y);
                arrows.localRotation = Quaternion.Euler(0, 0, 0);
                if(loop)
                    contentPanel.sizeDelta = new Vector2(elementSize.x * elementCount * 3, elementSize.y);
                else
                    contentPanel.sizeDelta = new Vector2(elementSize.x * elementCount, elementSize.y);
                contentPanel.localPosition = new Vector2(elementSize.x, 0);

            }

            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/PRE_Element");

            int startIdx = 0;
            int endidX = 0;
            if (loop)
            {
                startIdx = -1;
                endidX = 1;
            }

            for (int i = startIdx; i < endidX+1; i++)
            {
                int elementPos = 0;
                foreach (Tuple<float, string> elementTupel in elementTupels)
                {
                    Transform elementTrans = SceneObject.Instantiate(elementPrefab).transform;
                    elementTrans.SetParent(contentPanel, false);
                    elementTrans.GetComponent<RectTransform>().sizeDelta = elementSize * 0.8f ;

                    elementTrans.name = elementTupel.Item2;
                    elementTrans.GetComponent<TextMeshProUGUI>().text = elementTupel.Item2;
                    if (isVertical)
                    {
                        elementTrans.GetComponent<RectTransform>().localPosition = new Vector2(0, (elementPos + i * elementCount - 1) * elementSize.y);
                    }
                    else
                    {
                        elementTrans.GetComponent<RectTransform>().localPosition = new Vector2((elementPos + i * elementCount - 1) * elementSize.x, 0);
                    }
                    if(i == 0)
                        elements.Add(elementTupel.Item1);
                    elementPos++;
                }
            }

            valueText.text = elementTupels[0].Item1.ToString("N2");
            initialized = true;
        }

        //!
        //! Function receiving an updated float when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param f float to be set
        //!
        public void setParam(object sender, float f)
        {
            elements[0] = f;
            valueText.text = elements[currentAxis].ToString("N2");
        }

        //!
        //! Function receiving an updated Vector2 when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param v2 Vector2 to be set
        //!
        public void setParam(object sender, Vector2 v2 )
        {
            elements[0] = v2.x;
            elements[1] = v2.y;
            valueText.text = elements[currentAxis].ToString("N2");
        }

        //!
        //! Function receiving an updated Vector3 when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param v3 Vector3 to be set
        //!
        public void setParam(object sender, Vector3 v3)
        {
            elements[0] = v3.x;
            elements[1] = v3.y;
            elements[2] = v3.z;
            elements[3] = (v3.x + v3.y + v3.z) / 3f;
            valueText.text = elements[currentAxis].ToString("N2");

        }

        //!
        //! Function receiving an updated quaternion when it changed (usually called e.g. by Spinner caused by OnValueChange of Parameter)
        //! @param sender Sender of the event
        //! @param q quaternion to be set
        //!
        public void setParam(object sender, Quaternion q)
        {
            Vector3 rot = q.eulerAngles;
            elements[0] = rot.x;
            elements[1] = rot.y;
            elements[2] = rot.z;
            valueText.text = elements[currentAxis].ToString("N2");
        }

        //!
        //! Unity function called by IBeginDragHandler when a drag starts
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnBeginDrag(PointerEventData data)
        {
            dragStart = data.position;
            axisDecided = false;
        }

        //!
        //! Unity function called by IDragHandler when a drag is currently performed
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnDrag(PointerEventData data)
        {
            Vector2 contentPos = contentPanel.anchoredPosition;

            if (!axisDecided && Vector2.Distance(dragStart, data.position) > mainPanel.sizeDelta.x / 8f)
            {
                majorAxisX = Mathf.Abs(dragStart.x - data.position.x) > Mathf.Abs(dragStart.y - data.position.y);
                axisDecided = true;
            }
            if(axisDecided)
            {
                if (isVertical)
                    if (majorAxisX)
                    {
                        //adjust Parameter
                        if (allowValueSetting)
                            valueChanged.Invoke(this, (data.delta.x / Screen.width) * sensitivity);
                    }
                    else
                    { 
                        if (loop
                            || ((contentPanel.anchoredPosition.y < (contentPanel.sizeDelta.y/2 - elementSize.y / 1.5f))
                                && (contentPanel.anchoredPosition.y > -(contentPanel.sizeDelta.y / 2 + elementSize.y / 2.5f)))) // <150 >-250
                        {
                            contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + data.delta.y);
                            draggingAxis.Invoke(this, true);
                        }
                    }
                else
                {
                    if (majorAxisX)
                    {
                        if (loop
                            || ((contentPanel.anchoredPosition.x < (contentPanel.sizeDelta.x / 2 - elementSize.x / 1.5f))
                                && (contentPanel.anchoredPosition.x > -(contentPanel.sizeDelta.x / 2 + elementSize.x / 2.5f))))
                        {
                            contentPanel.anchoredPosition = new Vector2(contentPos.x + data.delta.x, contentPos.y);
                            draggingAxis.Invoke(this, true);
                        }
                    }
                    else
                    {
                        //adjust Parameter
                        if(allowValueSetting)
                            valueChanged.Invoke(this, (data.delta.y / Screen.height) * sensitivity);
                    }
                }
            }

            //realize infinit looping
            if (initialized && loop)
            {
                if (isVertical)
                {
                    if (contentPos.y > elementSize.y * elementCount)
                    {
                        contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y - (elementSize.y * elementCount));
                    }
                    if (contentPos.y < elementSize.y * -elementCount)
                    {
                        contentPanel.anchoredPosition = new Vector2(contentPos.x, contentPos.y + (elementSize.y * elementCount));
                    }
                }
                else
                {
                    if (contentPos.x > elementSize.x * elementCount)
                    {
                        contentPanel.anchoredPosition = new Vector2(contentPos.x - (elementSize.x * elementCount), contentPos.y);
                    }
                    if (contentPos.x < elementSize.x * -elementCount)
                    {
                        contentPanel.anchoredPosition = new Vector2(contentPos.x + (elementSize.x * elementCount), contentPos.y);
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
            if (axisDecided && ((isVertical && !majorAxisX) || (!isVertical && majorAxisX)))
            {
                Vector2 contentPos = contentPanel.GetComponent<RectTransform>().anchoredPosition;
                if (isVertical)
                {
                    contentPanel.anchoredPosition = new Vector2(contentPos.x, Mathf.Round(contentPos.y / elementSize.y)* elementSize.y);
                    currentAxis = Mathf.FloorToInt((-(contentPanel.anchoredPosition.y / elementSize.y)+1) % (elementCount));
                }
                else
                {
                    contentPanel.anchoredPosition = new Vector2(Mathf.Round(contentPos.x / elementSize.x) * elementSize.x, contentPos.y);
                    currentAxis = Mathf.FloorToInt((-(contentPanel.anchoredPosition.x / elementSize.x)+1) % (elementCount));
                }
                if (currentAxis < 0)
                    currentAxis = elementCount + currentAxis;
                parameterChanged.Invoke(this, currentAxis);
                valueText.text = elements[currentAxis].ToString("N2");
            }
            draggingAxis.Invoke(this, true);
        }
    }
}