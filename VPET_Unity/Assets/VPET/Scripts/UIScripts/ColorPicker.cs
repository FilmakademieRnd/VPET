/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

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
using System.IO;

namespace vpet
{
    // public delegate void CallbackColor(Color c);

    public class ColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

        public bool UseMaterial = false;

        public bool IsYCBCR = false;

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

        public Material m_material;

        // conversion matrix
        private Matrix4x4 ycbcrToRGBTransform = new Matrix4x4(
            new Vector4(1.0f, +0.0000f, +1.4020f, -0.7010f),
            new Vector4(1.0f, -0.3441f, -0.7141f, +0.5291f),
            new Vector4(1.0f, +1.7720f, +0.0000f, -0.8860f),
            new Vector4(0.0f, +0.0000f, +0.0000f, +1.0000f)
        ).transpose;

        private Texture2D m_readableTexture = null;
        private Texture2D m_readableTextureB = null;

        void Awake()
        {
            image = this.GetComponent<Image>();
            if (image == null) Debug.LogError(string.Format("{0}: No Image Component attached.", this.GetType()));

        }

        // DRAG
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.position.y > 200)
            {
                // Debug.Log("BEGIN DRAG");
                isActive = true;
            }
        }

        public void OnDrag(PointerEventData data)
        {
            // Debug.Log("ON DRAG delta: " + data.position + " rcet position " + image.rectTransform.position + " anchor min " + image.rectTransform.anchorMin + " size delt " + image.rectTransform.sizeDelta);
            if (data.position.y > 200)
            {
                if (UseMaterial)
                {
                    float x = ((data.position.x / Screen.width - 0.5f) * 3f / 4f) + 0.5f;
                    float y = 1f - data.position.y / Screen.height;
                    // Debug.Log("x: " + x + " y: " + y);

                    if (IsYCBCR)
                    {
                        //m_readableTexture = (Texture2D)m_material.GetTexture("_textureY"); //getRenderTexture((Texture2D)image.material.GetTexture("_textureY"), RenderTextureFormat.R8, TextureFormat.R8, "Y");
                        //m_readableTextureB = (Texture2D)m_material.GetTexture("_textureCbCr"); //getRenderTexture((Texture2D)image.material.GetTexture("_textureCbCr"), RenderTextureFormat.RG16, TextureFormat.RG16, "CBCR");
                        m_readableTexture = getRenderTexture(m_material.GetTexture("_textureY"), RenderTextureFormat.Default, TextureFormat.R8);
                        m_readableTextureB = getRenderTexture(m_material.GetTexture("_textureCbCr"), RenderTextureFormat.ARGB32, TextureFormat.RG16);
                        float _y = m_readableTexture.GetPixelBilinear(x, y)[0];
                        Color cbcr = m_readableTextureB.GetPixelBilinear(x, y);
                        // Debug.Log("_y: " + _y +  " cb: " + cbcr.r + " CR: " + cbcr.g);
                        Color rgba = ycbcrToRGBTransform * new Vector4(_y, cbcr.r, cbcr.g, 1.0f);
                        // Debug.Log("Color: " + rgba);
                        if (callback != null) callback(rgba);
                        Destroy(m_readableTexture);
                        Destroy(m_readableTextureB);

                    }
                    else
                    {
                        m_readableTexture = getRenderTexture(m_material.mainTexture, RenderTextureFormat.Default, TextureFormat.R8);
                        callback(m_readableTexture.GetPixelBilinear(x, y));
                        Destroy(m_readableTexture);
                    }
                }
                else
                {
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

                    if (callback != null) callback(image.sprite.texture.GetPixelBilinear(x * 0.5f + 0.5f, y * 0.5f + 0.5f));
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Debug.Log("END DRAG");
            isActive = false;
        }

        private Texture2D getRenderTexture(Texture texture, RenderTextureFormat formatR, TextureFormat formatT)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                formatR,
                RenderTextureReadWrite.Default);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);
            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false, false);

            // Copy the pixels from the RenderTexture to the new Texture
            readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTexture.Apply();


            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            return readableTexture;

        }
    }
}
