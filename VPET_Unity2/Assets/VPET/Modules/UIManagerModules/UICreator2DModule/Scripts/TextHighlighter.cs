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

//! @file "TextHighlighter.cs"
//! @brief Helper class to handle fading of SnapSelect elements
//! @author Jonas Trottnow
//! @version 0
//! @date 02.02.2022

using UnityEngine;
using TMPro;

namespace vpet
{
    //!
    //! Implementation of of the helper class attached to each Element of a SnapSelect to fade transparency based on distance to the center
    //!
    public class TextHighlighter : MonoBehaviour
    {
        //!
        //! Reference to RectTransform of the Element 
        //!
        private RectTransform rect;

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
        //! Unity Start() function used to initialize all references
        //!
        void Start()
        {
            rect = this.GetComponent<RectTransform>();
            refRect = this.transform.parent.parent.GetComponent<RectTransform>();
            snapSelect = this.transform.parent.parent.parent.GetComponent<SnapSelect>();
            txt = this.GetComponent<TextMeshProUGUI>();

            //attach updating function to darg event
            snapSelect.draggingAxis += updateTransparency;

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
            float dist = Vector2.Distance(rect.position, refRect.position);
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f - ((dist / snapSelect.fadeFactor)));
        }
    }
}
