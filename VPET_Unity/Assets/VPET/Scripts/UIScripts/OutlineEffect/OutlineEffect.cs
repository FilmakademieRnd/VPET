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
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class OutlineEffect : MonoBehaviour
{
	List<Outline> outlines = new List<Outline>();

    public Camera sourceCamera;
    public Camera outlineCamera;

    public float lineThickness = 1f;
    public float lineIntensity = 1f;

    public Color lineColor0 = new Color(1f, .8f, .3f);
    private bool flipY = false;
    public bool FlipY
    {
        set { flipY = value;
            UpdateMaterialsPublicProperties(); 
        }
    }

    public bool darkOutlines = false;
    public float alphaCutoff = .5f;

    Material outline1Material;

    Material outlineEraseMaterial;
    Shader outlineShader;
    Shader outlineBufferShader;
    Material outlineShaderMaterial;
    RenderTexture renderTexture;

    Material CreateMaterial(Color emissionColor)
    {
        Material m = new Material(outlineBufferShader);
        m.SetColor("_Color", emissionColor);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        return m;
    }

    void Start()
    {
        CreateMaterialsIfNeeded();
        UpdateMaterialsPublicProperties();

        if (sourceCamera == null)
        {
            sourceCamera = GetComponent<Camera>();

            if (sourceCamera == null)
                sourceCamera = Camera.main;
        }

        if (outlineCamera == null)
        {
            GameObject cameraGameObject = new GameObject("Outline Camera");
            cameraGameObject.transform.parent = sourceCamera.transform;
            outlineCamera = cameraGameObject.AddComponent<Camera>();
        }

        UpdateOutlineCameraFromSource();

        renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
        outlineCamera.targetTexture = renderTexture;
    }

    void OnDestroy()
    {
        renderTexture.Release();
        DestroyMaterials();
    }

    void OnPreCull()
    {
        if (outlines != null)
        {
            for (int i = 0; i < outlines.Count; i++)
            {
                if (outlines[i] != null)
                {
                    Renderer renderer = outlines[i].GetComponent<Renderer>();
                    if (renderer)
                    {
                        outlines[i].originalMaterial = renderer.sharedMaterial;
                        outlines[i].originalLayer = outlines[i].gameObject.layer;

                        if (outlines[i].eraseRenderer)
                            outlines[i].GetComponent<Renderer>().sharedMaterial = outlineEraseMaterial;
                        else
                            outlines[i].GetComponent<Renderer>().sharedMaterial = outline1Material;

                        if (outlines[i].GetComponent<Renderer>() is MeshRenderer)
                        {
                            outlines[i].GetComponent<Renderer>().sharedMaterial.mainTexture = outlines[i].originalMaterial.mainTexture;
                        }
                        outlines[i].gameObject.layer = LayerMask.NameToLayer("Outline");
                    }
                }
            }
        }

        outlineCamera.Render();

        if (outlines != null)
        {
            for (int i = 0; i < outlines.Count; i++)
            {
                if (outlines[i] != null)
                {
                    if (outlines[i].GetComponent<Renderer>() is MeshRenderer)
						outlines[i].GetComponent<Renderer>().sharedMaterial.mainTexture = null;

                    Renderer renderer = outlines[i].GetComponent<Renderer>();
                    if (renderer)
                    {
                        renderer.sharedMaterial = outlines[i].originalMaterial;
                        outlines[i].gameObject.layer = outlines[i].originalLayer;
                    }
                }
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        outlineShaderMaterial.SetTexture("_OutlineSource", renderTexture);
        Graphics.Blit(source, destination, outlineShaderMaterial);
    }

    private void CreateMaterialsIfNeeded()
    {
        if (outlineShader == null)
            outlineShader = Resources.Load<Shader>("OutlineEffect/OutlineShader");
        if (outlineBufferShader == null)
            outlineBufferShader = Resources.Load<Shader>("OutlineEffect/OutlineBufferShader");
        if (outlineShaderMaterial == null)
        {
            outlineShaderMaterial = new Material(outlineShader);
            outlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
            UpdateMaterialsPublicProperties();
        }
        if (outlineEraseMaterial == null)
            outlineEraseMaterial = CreateMaterial(new Color(0, 0, 0, 0));
        if (outline1Material == null)
            outline1Material = CreateMaterial(new Color(1, 0, 0, 0));

        outline1Material.SetFloat("_AlphaCutoff", alphaCutoff);
    }

    private void DestroyMaterials()
    {
        DestroyImmediate(outlineShaderMaterial);
        DestroyImmediate(outlineEraseMaterial);
        DestroyImmediate(outline1Material);
        outlineShader = null;
        outlineBufferShader = null;
        outlineShaderMaterial = null;
        outlineEraseMaterial = null;
        outline1Material = null;
    }

    private void UpdateMaterialsPublicProperties()
    {
        if (outlineShaderMaterial)
        {
            outlineShaderMaterial.SetFloat("_LineThicknessX", lineThickness / 1000);
            outlineShaderMaterial.SetFloat("_LineThicknessY", lineThickness / 1000);
            outlineShaderMaterial.SetFloat("_LineIntensity", lineIntensity);
            outlineShaderMaterial.SetColor("_LineColor1", lineColor0);
            if (flipY)
                outlineShaderMaterial.SetInt("_FlipY", 1);
            else
                outlineShaderMaterial.SetInt("_FlipY", 0);
            if (darkOutlines)
                outlineShaderMaterial.SetInt("_Dark", 1);
            else
                outlineShaderMaterial.SetInt("_Dark", 0);
        }
    }


    void UpdateOutlineCameraFromSource()
    {
        outlineCamera.CopyFrom(sourceCamera);
        outlineCamera.renderingPath = RenderingPath.Forward;
        outlineCamera.enabled = false;
        outlineCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        outlineCamera.clearFlags = CameraClearFlags.SolidColor;
        outlineCamera.cullingMask = LayerMask.GetMask("Outline");
        outlineCamera.rect = new Rect(0, 0, 1, 1);
    }

    public void AddOutline(Outline outline)
    {
        if (!outlines.Contains(outline))
        {
			outlines.Add(outline);
        }
    }
    public void RemoveOutline(Outline outline)
	{
		outlines.Remove(outline);
    }

}
