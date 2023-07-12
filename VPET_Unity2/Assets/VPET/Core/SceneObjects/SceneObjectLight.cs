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

//! @file "SceneObjectPointLight.cs"
//! @brief implementation SceneObjectLight as a specialisation of the SceneObject.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET light object as a specialisation of the SceneObject
    //!
    public class SceneObjectLight : SceneObject
    {
        //!
        //! the color of the light
        //!
        private Parameter<Color> color;

        //!
        //! the intensity of the light
        //!
        private Parameter<float> intensity;

        //!
        //! the reference to the light component
        //!
        protected Light _light;

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            _light = this.GetComponent<Light>();
            if (_light)
            {
                color = new Parameter<Color>(_light.color, "color", this);
                color.hasChanged += updateColor;
                intensity = new Parameter<float>(_light.intensity, "intensity", this);
                intensity.hasChanged += updateIntensity;
            }
            else
                Helpers.Log("no light component found!");

        }

        //!
        //! Function called, when Unity emit it's OnDestroy event.
        //!
        public override void OnDestroy()
        {
            base.OnDestroy();
            color.hasChanged -= updateColor;
            intensity.hasChanged -= updateIntensity;
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
#if UNITY_EDITOR
            updateSceneObjectLightParameters();
#endif
        }

        //!
        //! Update the light color of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new color value
        //!
        private void updateColor(object sender, Color a)
        {
            _light.color = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update the light intensity of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new intensity value
        //!
        private void updateIntensity(object sender, float a)
        {
            _light.intensity = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! updates the Unity light component specific parameters and informs all connected VPET parameters about the change
        //!
        private void updateSceneObjectLightParameters()
        {
            if (_light.color != color.value)
                color.value = _light.color;
            if (_light.intensity != intensity.value)
                intensity.value = _light.intensity;
        }
    }
}