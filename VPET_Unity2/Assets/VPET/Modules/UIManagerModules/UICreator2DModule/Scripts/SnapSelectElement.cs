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

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-------------------------------------------------------------------------------
*/

//! @file "TextHighlighter.cs"
//! @brief Helper class to handle fading of SnapSelect elements
//! @author Jonas Trottnow
//! @version 0
//! @date 02.03.2022

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! Implementation of of the helper class attached to each element of a SnapSelect
    //! handles e.g. fading transparency based on distance to the center and clicks
    //!
    public class SnapSelectElement : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        //!
        //! Reference to RectTransform of the Element 
        //!
        private RectTransform rect;

        //!
        //! size of the element
        //!
        float size;

        //!
        //! initial size of element
        //!
        Vector2 initialSize;

        //!
        //! Reference to RectTransform representing the center of the SnapSelect
        //!
        private RectTransform refRect;

        //!
        //! center of the reference rect
        //!
        private Vector3 refRectCenter;

        //!
        //! Reference to Text field of the Element
        //!
        private TextMeshProUGUI txt;

        //!
        //! Reference to Text field of the Element
        //!
        private Image image;

        //!
        //! Reference to the associated snapSelect Instance
        //!
        private SnapSelect snapSelect;

        //!
        //! Reference to VPET UI Settings
        //!
        private VPETUISettings uiSettings;

        //!
        //! index of the element in the menu
        //!
        public int index;

        //!
        //! id of the corresponding menu button
        //! reported back to SnapSelect when element is clicked
        //!
        public int buttonID;


        //!
        //! Event emitted when value has changed
        //!
        public event EventHandler<SnapSelectElement> clicked;

        //!
        //! Action being executed by the button
        //!
        public Action clickAction;

        //!
        //! Is the Button a toogle or switch button
        //!
        public bool isToggle = false;

        //!
        //! Unity OnEnable() function used to initialize all references
        //!
        void Awake()
        {
            rect = this.GetComponent<RectTransform>();
            if (!snapSelect)
            {
                snapSelect = this.transform.parent.GetComponent<SnapSelect>();
                refRect = snapSelect.GetComponent<RectTransform>();
                refRectCenter = refRect.position - new Vector3(refRect.sizeDelta.x, refRect.sizeDelta.y, 0f);
                initialSize = refRect.sizeDelta;
            }
            txt = this.GetComponentInChildren<TextMeshProUGUI>();
            image = this.GetComponent<Image>();
            size = Mathf.Max(rect.sizeDelta.x, rect.sizeDelta.y);

            //attach updating function to drag event
            snapSelect.draggingAxis += updateTransparency;

            //attach reset function to click event
            snapSelect.highlightElement += setHighlight;

            snapSelect.updateHighlightElement += updateHighlight;

            //get ui settings
            uiSettings = snapSelect.uiSettings;

            //run initial transparency update
            updateTransparency(this, true);

            txt.color = image.color = uiSettings.colors.ElementSelection_Default;
            if (image.sprite)
                image.color = uiSettings.colors.ElementSelection_Default;
        }

        //!
        //! Function updating the transparency of the element based on distance to center of SnapSelect
        //! @param sender Sender of the event
        //! @param e Payload of the event for later usage, currently unused
        //!
        public void updateTransparency(object sender, bool e)
        {
            if (!snapSelect._selectByClick)
            {
                float d = Vector3.Distance(rect.position, refRectCenter) / size;
                float f = snapSelect._fadeFactor;
                //rect.sizeDelta = Vector2.Scale(initialSize,new Vector2(1/(1 + d), 1/(1 + d)));
                //if(index == 1) Debug.Log(initialSize + " * "+  d + " = " + rect.sizeDelta);
                //txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f / (d * (1f / f)));//((1f - (d * (1f - f))) / d) *255f);
            }
        }

        //!
        //! Unity function called by IPointerUpHandler when a touch ends on the menu
        //! needed to make OnPointerUp possible
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isToggle)
                if (txt.color != uiSettings.colors.ElementSelection_Highlight)
                {
                    txt.color = uiSettings.colors.ElementSelection_Highlight;
                    if (image)
                        if (image.sprite)
                           image.color = uiSettings.colors.ElementSelection_Highlight;
                }
        }

        //!
        //! Unity function called by IPointerUpHandler when a touch ends on the menu
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnPointerUp(PointerEventData data)
        {
            clicked?.Invoke(this, this);
            clickAction?.Invoke();
            if (isToggle)
            {
                txt.color = uiSettings.colors.ElementSelection_Default;
                if (image)
                    if (image.sprite)
                        image.color = uiSettings.colors.ElementSelection_Default;
            }
        }

        //!
        //! Sets and resets the text color
        //! @param sender Sender of the event
        //! @param idx Index of last clicked element
        //!
        public void setHighlight(object sender, int idx)
        {
            if (idx == index)
            {
                if (txt.color == uiSettings.colors.ElementSelection_Highlight)
                {
                    txt.color = uiSettings.colors.ElementSelection_Default;
                    if(image)
                        if(image.sprite)
                            image.color = uiSettings.colors.ElementSelection_Default;
                }
                else
                {
                    txt.color = uiSettings.colors.ElementSelection_Highlight;
                    if (image)
                        if (image.sprite)
                            image.color = uiSettings.colors.ElementSelection_Highlight;
                }
            }
            else if (!snapSelect._multiSelect)
            {
                txt.color = uiSettings.colors.ElementSelection_Default;
                if (image)
                    if (image.sprite)
                        image.color = uiSettings.colors.ElementSelection_Default;
            }
        }

        public void updateHighlight(object sender, MenuButton.HighlightEventArgs e)
        {
            if (e.id == buttonID)
                if (e.highlight)
                {
                    txt.color = uiSettings.colors.ElementSelection_Highlight;
                    if (image)
                        if (image.sprite)
                            image.color = uiSettings.colors.ElementSelection_Highlight;
                }
                else
                {
                    txt.color = uiSettings.colors.ElementSelection_Default;
                    if (image)
                        if (image.sprite)
                            image.color = uiSettings.colors.ElementSelection_Default;
                }

        }
    }
}
