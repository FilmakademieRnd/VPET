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

//! @file "ColorSelect.cs"
//! @brief implementation of a color picker.
//! @author Paulo Scatena
//! @author Simon Spielmann
//! @version 0
//! @date 02.04.2022

using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    public class ColorSelect : UIBehaviour, IBeginDragHandler, IDragHandler
    {
        private Vector2 pickerSize;

        private Color outputColor;

        private Material mat;

        // development phase
        //private Image testImage;
        private float hue;
        private float sat;
        private float val;

        private bool hueDrag = false;

        private Parameter<Color> col = null;

        private Canvas _canvas;

        //!
        //! Init function of the ColorSelect that needs to be called manually 
        //! @param color This is the color parameter to be displayed and edited
        //!
        public void Init(AbstractParameter param)
        {
            col = (Parameter<Color>)param;

            Color inColor = col.value;

            // Decompose into HSV components
            Color.RGBToHSV(inColor, out hue, out sat, out val);

            // Use pure hue for the material input
            Color shaderColor = Color.HSVToRGB(hue, 1f, 1f);
            mat.SetColor("_InputColor", shaderColor);

            // Set indicator coordinates
            mat.SetVector("_InputPos", new(sat * .8f, val, .9f, hue));

            // Output starts as the input
            outputColor = inColor;
        }


        //!
        //! Unity function called when the object becomes enabled and active.
        //!
        protected override void OnEnable()
        {
            _canvas = GetComponentInParent<Canvas>();
            // Grab picker dimensions
            RectTransform rect = GetComponent<RectTransform>();
            pickerSize = rect.rect.size * _canvas.scaleFactor;

            // Grab material
            mat = GetComponent<Image>().material;
        }


        //!
        //! Unity function called by IBeginDragHandler when a drag starts
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnBeginDrag(PointerEventData data)
        {
            Vector3 clickPos = data.position;
            clickPos -= transform.position;
            clickPos /= pickerSize;
            // Identify area of operation - TODO: get rid of magic number?
            hueDrag = clickPos.x > .3f;
            //Debug.Log("Begin: " + clickPos.ToString());
        }

        //!
        //! Unity function called by IDragHandler when a drag is currently performed
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        public void OnDrag(PointerEventData data)
        {
            Vector3 clickPos = data.position;
            clickPos -= transform.position;
            clickPos /= pickerSize;
            clickPos.y = Mathf.Clamp(clickPos.y + .5f, 0f, 1f);
            //Debug.Log("Drag: " + clickPos.ToString());
            if (hueDrag)
            {
                hue = clickPos.y;
                mat.SetColor("_InputColor", Color.HSVToRGB(hue, 1f, 1f));
            }
            else
            {
                val = clickPos.y;
                sat = Mathf.Clamp((clickPos.x + .5f) * 1.25f, 0f, 1f);
            }
            outputColor = Color.HSVToRGB(hue, sat, val);

            mat.SetVector("_InputPos", new(sat * .8f, val, .9f, hue));

            if (col != null)
                col.setValue(outputColor);
        }

    }
}


