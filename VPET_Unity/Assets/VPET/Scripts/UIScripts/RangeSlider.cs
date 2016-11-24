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
﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace vpet
{

    public class RangeSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public class OnValueChangedEvent : UnityEvent<float> { };

        public enum SliderDirection { VERTICAL, HORIZONTAL };

        public OnValueChangedEvent OnValueChanged = new OnValueChangedEvent();


        public Text ValueField = null;

        public SliderDirection sliderDirection = SliderDirection.VERTICAL;

        private float currentValue = 0f;
        public float Value
        {
            get { return currentValue;  }
            set
            {
                currentValue = value;
                ValueField.text = String.Format("{0:##0.#}", currentValue); 
                // onValueChange();
            }
        }
        public float velocity = 0f;

        public float Sensitivity = 1f;


        private float minValue = float.MinValue;
        public float MinValue
        {
            set { minValue = value; }
        }

        private float maxValue = float.MaxValue;
        public float MaxValue
        {
            set { maxValue = value; }
        }

        public Sprite CenterSprite
        {
            get { return transform.GetComponent<Image>().sprite; }
            set { transform.GetComponent<Image>().sprite = value; }
        }


        private UnityAction<float> callback = null;
        public UnityAction<float> Callback
        {
            set { callback = value; }
        }

        void Awake()
        {

        }

        void Start()
        {
            transform.parent.localPosition = new Vector3(VPETSettings.Instance.canvasHalfWidth - UI.ButtonOffset, 0, 0);
        }

        // Update is called once per frame
        void Update()
        {
            if (velocity != 0)
            {
                currentValue += velocity * Time.deltaTime * Sensitivity;
                currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

                onValueChange();

                if (Mathf.Abs(velocity) < 10f) velocity = 0f;
            }
        }

        public void IncreaseValue( float v )
        {
            Value = currentValue + Sensitivity * v;
            onValueChange();
        }

        private void onValueChange()
        {
            ValueField.text = String.Format("{0:##0.#}", currentValue);
            // invoke
            if ( callback != null )callback(currentValue);
        }

        public void Show()
        {
            transform.parent.gameObject.SetActive(true);
        }

        public void Hide()
        {
            transform.parent.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            velocity = 0;
            //ValueField.gameObject.SetActive(true);
        }


        // DRAG
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Debug.Log("BEGIN DRAG");
            //ValueField.transform.parent.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData data)
        {
            // Debug.Log("ON DRAG delta: " + data.delta.ToString());
            if (sliderDirection == SliderDirection.HORIZONTAL)
            {
                velocity = data.position.x - data.pressPosition.x;
            }
            else
            {
                velocity = data.position.y - data.pressPosition.y;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Debug.Log("END DRAG");
            //ValueField.transform.parent.gameObject.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            velocity = 0;
           // ValueField.gameObject.SetActive(false);
        }
    }
}