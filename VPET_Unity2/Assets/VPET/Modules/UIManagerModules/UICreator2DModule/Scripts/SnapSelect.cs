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

//! @file "ScrollSnap.cs"
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
        List<float> elements;

        public bool isVertical;

        public bool loop;

        public bool allowValueSetting;

        public int previewExtend;

        public float fadeFactor;

        //!
        //! Event emitted when parameter has changed
        //!
        public event EventHandler<int> parameterChanged;

        //!
        //! Event emitted when value has changed
        //!
        public event EventHandler<float> valueChanged;

        private RectTransform mainPanel;

        private RectTransform contentPanel;

        private RectTransform arrows;

        private TextMeshProUGUI valueText;

        private int elementCount;

        private bool initialized = false;

        private Vector2 elementSize;

        private bool majorAxisX;
        private bool axisDecided;

        private Vector2 dragStart;

        private int currentAxis;

        private bool draggingActive;

        private float deltaValue;

        private float sensitivity;

        /*public void test()
        {
            List<string> elementNames = new List<string>() { "X", "Y", "Z"};
            Init(elementNames);
        }*/


        // Start is called before the first frame update
        public void Init(List<Tuple<float, string>> elementTupels, float sensitivityIn)
        {
            elements = new List<float>();
            loop = true;
            elementSize = this.GetComponent<RectTransform>().sizeDelta;
            mainPanel = this.transform.GetChild(1).GetComponent<RectTransform>();
            contentPanel = mainPanel.GetChild(0).GetComponent<RectTransform>();
            arrows = this.transform.GetChild(2).GetComponent<RectTransform>();
            valueText = this.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            draggingActive = false;
            deltaValue = 0;
            sensitivity = sensitivityIn;

            elementCount = elementTupels.Count;
            currentAxis = 0;

            if (isVertical)
            {
                mainPanel.sizeDelta = new Vector2(elementSize.x, elementSize.y * (previewExtend * 2 + 1));
                arrows.localRotation = Quaternion.Euler(0, 0, 90);
                contentPanel.sizeDelta = new Vector2(elementSize.x, elementSize.y * elementCount * 3);
                contentPanel.localPosition = new Vector2(0, elementSize.y);
            }
            else
            {
                mainPanel.sizeDelta = new Vector2(elementSize.x * (previewExtend * 2 + 1), elementSize.y);

                contentPanel.sizeDelta = new Vector2(elementSize.x * elementCount * 3, elementSize.y);
                contentPanel.localPosition = new Vector2(elementSize.x, 0);

            }


            GameObject elementPrefab = Resources.Load<GameObject>("Prefabs/PRE_Element");
            
            for (int i = -1; i < 2; i++)
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

            valueText.text = elementTupels[0].Item1.ToString();
            initialized = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (draggingActive && axisDecided && ((isVertical && majorAxisX) || (!isVertical && !majorAxisX)))
            {
                elements[currentAxis] += (((deltaValue) * sensitivity) / Time.deltaTime);
                valueChanged.Invoke(this, elements[currentAxis]);
                valueText.text = elements[currentAxis].ToString();
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            dragStart = data.position;
            axisDecided = false;
            draggingActive = true;
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 contentPos = contentPanel.GetComponent<RectTransform>().anchoredPosition;

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
                        deltaValue = (data.position.x - dragStart.x)/Screen.width;
                    }

                    else
                        contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x, contentPos.y + data.delta.y);
                else
                    if (majorAxisX)
                    contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x + data.delta.x, contentPos.y);
                else
                {
                    //adjust Parameter
                    deltaValue = (data.position.y - dragStart.y)/Screen.height;
                }
            }

            //realize infinit looping
            if (initialized && loop)
            {
                if (isVertical)
                {
                    if (contentPos.y > elementSize.y * elementCount)
                    {
                        contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x, contentPos.y - (elementSize.y * elementCount));
                    }
                    if (contentPos.y < elementSize.y * -elementCount)
                    {
                        contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x, contentPos.y + (elementSize.y * elementCount));
                    }
                }
                else
                {
                    if (contentPos.x > elementSize.x * elementCount)
                    {
                        contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x - (elementSize.x * elementCount), contentPos.y);
                    }
                    if (contentPos.x < elementSize.x * -elementCount)
                    {
                        contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x + (elementSize.x * elementCount), contentPos.y);
                    }
                }
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            draggingActive = false;
            deltaValue = 0;
            if (axisDecided && ((isVertical && !majorAxisX) || (!isVertical && majorAxisX)))
            {
                Vector2 contentPos = contentPanel.GetComponent<RectTransform>().anchoredPosition;
                if (isVertical)
                {
                    contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.x,
                                                                                              Mathf.Round(contentPos.y / elementSize.y)* elementSize.y);
                    currentAxis = Mathf.FloorToInt((-(contentPanel.GetComponent<RectTransform>().anchoredPosition.y / elementSize.y)+1) % (elementCount));
                    if (currentAxis < 0)
                        currentAxis = elementCount + currentAxis;
                    Debug.Log(currentAxis);
                    parameterChanged.Invoke(this, currentAxis);
                    valueText.text = elements[currentAxis].ToString();
                }
                else
                {
                    contentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Round(contentPos.x / elementSize.x) * elementSize.x,
                                                                                              contentPos.y);
                    currentAxis = Mathf.FloorToInt((-(contentPanel.GetComponent<RectTransform>().anchoredPosition.x / elementSize.x)+1) % (elementCount));
                    if (currentAxis < 0)
                        currentAxis = elementCount + currentAxis;
                    Debug.Log(currentAxis);
                    parameterChanged.Invoke(this, currentAxis);
                    valueText.text = elements[currentAxis].ToString();
                }
            }

        }
    }
}