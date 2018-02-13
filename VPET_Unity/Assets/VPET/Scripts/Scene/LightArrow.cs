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

//! 
//! This class colorizes the object according to the light color and scales the object based on the distance to the camera.
//! It is beeing applied / should be attached on gameObjects visualizing a directional light.
//!
namespace vpet
{
    public class LightArrow : MonoBehaviour
    {

        private Material material;
        private Light sourceLight;

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            this.transform.localRotation = Quaternion.Euler(90, 0, 0);

            material = this.GetComponent<Renderer>().material;
            sourceLight = this.transform.parent.GetComponent<Light>();

            updateArrow();
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            if (this.GetComponent<Renderer>().enabled)
            {
                updateArrow();
            }
        }

        //! 
        //! Update the object shape based on new light parameters.
        //!
        private void updateArrow()
        {
            material.color = new Color(sourceLight.color.r,
                              sourceLight.color.g,
                              sourceLight.color.b,
                              material.color.a);
            this.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, this.transform.parent.position) / 5f) * (Camera.main.fieldOfView / 30f);
        }
    }
}