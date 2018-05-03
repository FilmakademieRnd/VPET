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

[RequireComponent(typeof(Renderer))]
public class Outline : MonoBehaviour
{
    public OutlineEffect outlineEffect;

	public int color;
	public bool eraseRenderer;

	[HideInInspector]
	public int originalLayer;
	[HideInInspector]
	public Material originalMaterial;

    //void Start()
    //{
    //}

    void OnEnable()
    {
        if (Camera.main.transform.childCount > 0)
        {
            if (outlineEffect == null)
                outlineEffect = Camera.main.transform.GetChild(0).GetComponent<Camera>().GetComponent<OutlineEffect>();
            outlineEffect.AddOutline(this);
        }
    }

    void OnDisable()
    {
        if (outlineEffect)
            outlineEffect.RemoveOutline(this);
    }
}
