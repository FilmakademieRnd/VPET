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

namespace backup
{
    //public class ColorSelect : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    public class ColorSelectBackupForHueSatOnly : UIBehaviour, IDragHandler
    {
        private Vector2 pickerSize;

        private Color outputColor;

        private Material mat;

        // development phase
        private Image testImage;
        private float hue;
        private float sat;

        //!
        //! Unity function called when the object becomes enabled and active.
        //!
        protected override void OnEnable()
        {
            // Grab picker dimensions
            RectTransform rect = GetComponent<RectTransform>();
            pickerSize = rect.sizeDelta;

            // Grab material
            mat = GetComponent<Image>().material;

            // Grab selected object - development version
            var obj = GameObject.Find("TestImage");
            testImage = obj.GetComponent<Image>();

            // Initial color from selected object
            Color startCol = testImage.color;

            // Decompose into HSV components
            Color.RGBToHSV(startCol, out hue, out sat, out _);
            Color shaderColor = Color.HSVToRGB(hue, 1f, 1f);

            mat.SetColor("_InputColor", shaderColor);

            outputColor = Color.HSVToRGB(hue, sat, 1f);
        }


        //!
        //! Unity function called by IBeginDragHandler when a drag starts
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        //public void OnBeginDrag(PointerEventData data)
        //{
        //    //Vector3 clickPos = data.position;
        //    //clickPos -= transform.position;
        //    //Debug.Log("Begin: " + clickPos.ToString());
        //}

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
            Debug.Log("Drag: " + clickPos.ToString());
            if (clickPos.x < 0)
            {
                //myCol = Gradient(clickPos.y);
                //mat.SetColor("_InputColor", myCol);

                hue = clickPos.y;
                mat.SetColor("_InputColor", Color.HSVToRGB(hue, 1f, 1f));
            }
            else
            {
                //finalCol = Color.Lerp(Color.white, myCol, clickPos.y);

                sat = clickPos.y;
            }
            outputColor = Color.HSVToRGB(hue, sat, 1f);

            testImage.color = outputColor;
        }

        //!
        //! Unity function called by IEndDragHandler when a drag ends
        //! @param data Data of the drag event e.g. postion, delta, ...
        //!
        //public void OnEndDrag(PointerEventData data)
        //{
        //    //Debug.Log("End: " + data.position.ToString());
        //}


        // Mathematical helpers
        Vector3 AddVec(Vector3 inVec, float inFloat)
        {
            return new(inVec.x + inFloat, inVec.y + inFloat, inVec.z + inFloat);
        }
        float ModFloat(float a, float b)
        {
            return a % b;
            //return (a % b + b) % b; // more like actual modulo but not needed
        }

        Vector3 ModVec(Vector3 inVec, float inFloat)
        {
            return new(ModFloat(inVec.x, inFloat), ModFloat(inVec.y, inFloat), ModFloat(inVec.z, inFloat));
        }

        Vector3 AbsVec(Vector3 inVec)
        {
            return new(Mathf.Abs(inVec.x), Mathf.Abs(inVec.y), Mathf.Abs(inVec.z));
        }

        Vector3 ClampVec(Vector3 inVec, float a, float b)
        {
            return new(Mathf.Clamp(inVec.x, a, b), Mathf.Clamp(inVec.y, a, b), Mathf.Clamp(inVec.z, a, b));
        }

        Color Gradient(float c)
        {
            Vector3 rgb = ClampVec(AddVec(AbsVec(AddVec(ModVec(AddVec(new(0f, 4f, 2f), c * 6f), 6f), -3f)), -1f), 0f, 1f);
            rgb = Vector3.Scale(Vector3.Scale(rgb, rgb), AddVec(-2f * rgb, 3f));
            return new(rgb.x, rgb.y, rgb.z, 1f);
        }
    }
}


