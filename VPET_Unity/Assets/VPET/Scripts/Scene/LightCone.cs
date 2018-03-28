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

//!
//! This class colorizes the object according to the light color and scales the object based on the distance to the camera, the range of the light and the coneAngle.
//! It is beeing applied / should be attached on gameObjects visualizing a spot light.
//!      
//!
namespace vpet
{
    public class LightCone : MonoBehaviour
    {
        //!
        //! height of the light cone
        //!
        static float height = 2.0f;
        //!
        //! radius of the light cone
        //!
        static float radius = 1.0f;
        //!
        //! initial cone angle used to speed up future calculations and make debugging easier
        //!
        // float initialConeAngleDegree = (Mathf.Atan(radius/height) * Mathf.Rad2Deg)*2.0f;

        private Material material;
        private Light sourceLight;

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            material = this.GetComponent<Renderer>().material;
            sourceLight = this.transform.parent.GetComponent<Light>();

            // this.transform.Rotate(-90,0,0);
            updateCone();
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            if (this.GetComponent<Renderer>().enabled)
            {
                updateCone();
            }
        }


        //! 
        //! Update the object shape based on new light parameters.
        //!
        private void updateCone()
        {
            material.color = new Color(sourceLight.color.r,
                              sourceLight.color.g,
                              sourceLight.color.b,
                              material.color.a);

            float radius_spot = Mathf.Tan(this.transform.parent.GetComponent<Light>().spotAngle / 2f * Mathf.Deg2Rad) * this.transform.parent.GetComponent<Light>().range;

            this.transform.localScale = new Vector3(radius_spot,
                                                    this.transform.parent.GetComponent<Light>().range / height,
                                                    radius_spot);
        }
    }
}