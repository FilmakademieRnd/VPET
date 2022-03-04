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

//! @file "TextHighlighter.cs"
//! @brief Helper class to handle fading of SnapSelect elements
//! @author Jonas Trottnow
//! @version 0
//! @date 02.03.2022

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

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
        //! Reference to RectTransform representing the center of the SnapSelect
        //!
        private RectTransform refRect;

        //!
        //! Reference to Text field of the Element
        //!
        private TextMeshProUGUI txt;

        //!
        //! Reference to the associated snapSelect Instance
        //!
        private SnapSelect snapSelect;

        //!
        //! index of the element in the menu
        //! reported back to SnapSelect when element is clicked
        //!
        public int index;

        //!
        //! Time of last pointer down
        //!
        float pointerDownTime;

        //!
        //! Event emitted when value has changed
        //!
        public event EventHandler<SnapSelectElement> clicked;

        //!
        //! Unity Start() function used to initialize all references
        //!
        void Start()
        {
            rect = this.GetComponent<RectTransform>();
            snapSelect = this.transform.parent.parent.parent.GetComponent<SnapSelect>();
            refRect = snapSelect.GetComponent<RectTransform>();
            txt = this.GetComponent<TextMeshProUGUI>();
            size = Mathf.Max(rect.sizeDelta.x, rect.sizeDelta.y);

            //attach updating function to drag event
            snapSelect.draggingAxis += updateTransparency;

            //attach reset function to click event
            snapSelect.elementClicked += setHighlight;

            //run initial transparency update
            updateTransparency(this, true);
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
                float d = Vector3.Distance(rect.position, refRect.position - new Vector3(refRect.sizeDelta.x / 2f, refRect.sizeDelta.y / 2f, 0)) / size;
                float f = snapSelect._fadeFactor;
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f / (d * (1f / f)));//((1f - (d * (1f - f))) / d) *255f);
            }
        }

        //!
        //! Unity function called by IPointerUpHandler when a touch ends on the menu
        //! needed to make OnPointerUp possible
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.time;
        }
        //!
        //! Unity function called by IPointerUpHandler when a touch ends on the menu
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnPointerUp(PointerEventData data)
        {
            if ((Time.time - pointerDownTime) < 0.2f)
            {
                clicked.Invoke(this, this);
            }
        }

        //!
        //! Sets and resets the text color
        //! @param sender Sender of the event
        //! @param idx Index of last clicked element
        //!
        public void setHighlight(object sender, int idx)
        {
            if (idx != index)
            {
                txt.color = Color.white;
            }
            else
            {
                txt.color = Color.red;
            }
        }
    }
}
