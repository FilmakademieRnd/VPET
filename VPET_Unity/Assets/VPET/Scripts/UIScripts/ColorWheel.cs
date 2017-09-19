/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace vpet
{
    public delegate void CallbackColor(Color c);

    public class ColorWheel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Color currentValue = Color.white;

        public Color Value
        {
            set
            {
                currentValue = value;
            }
        }


        private float sensitivity = 1f;
        public float Sensitivity
        {
            set { sensitivity = value; }
        }

        private Image image = null;


        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
        }

        private CallbackColor callback;
        public CallbackColor Callback
        {
            set { callback = value; }
        }

        void Awake()
        {
            image = this.GetComponent<Image>();
            if (image == null) Debug.LogError(string.Format("{0}: No Image Component attached.", this.GetType()));
        }

        // DRAG
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Debug.Log("BEGIN DRAG");
            isActive = true;
        }

        public void OnDrag(PointerEventData data)
        {
            //Debug.Log("ON DRAG delta: " + data.position + " rcet position " + image.rectTransform.position + " anchor min " + image.rectTransform.anchorMin + " size delt " + image.rectTransform.sizeDelta);

            RectTransform imageTransform = image.rectTransform;
            float x = (data.position.x - imageTransform.position.x) / Screen.width * 10f;
            float y = (data.position.y - imageTransform.position.y) / Screen.width * 10f;
            float radius = Mathf.Sqrt(x * x + y * y);

            if (radius > 1f)
            {
                // + offset to exclude aliased boarder values
                x /= radius + .05f;
                y /= radius + .05f;
            }
            //Debug.Log("x:" + x + "y:" + y);
            callback(image.sprite.texture.GetPixelBilinear(x * 0.5f + 0.5f, y * 0.5f + 0.5f));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Debug.Log("END DRAG");
            isActive = false;
        }

    }
}