/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneObjectPointLight.cs"
//! @brief implementation SceneObjectLight as a specialisation of the SceneObject.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.06.2021

using System.Collections;
using System.Collections.Generic;
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
        //! the range of the light
        //!
        private Parameter<float> range;

        //!
        //! the reference to the light component
        //!
        protected Light _light;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            _light = this.GetComponent<Light>();
            if (_light)
            {
                color = new Parameter<Color>(_light.color, "color", (short) parameterList.Count);
                color.hasChanged += updateColor;
                _parameterList.Add(color);
                intensity = new Parameter<float>(_light.intensity, "intensity", (short)parameterList.Count);
                intensity.hasChanged += updateIntensity;
                _parameterList.Add(intensity);
                range = new Parameter<float>(_light.range, "range", (short)parameterList.Count);
                range.hasChanged += updateRange;
                _parameterList.Add(range);
            }
            else
                Helpers.Log("no light component found!");

        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            //updateSceneObjectLightParameters();
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
        //! Update the light range of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new range value
        //!
        private void updateRange(object sender, float a)
        {
            _light.range = a;
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
            if (_light.range != intensity.value)
                range.value = _light.range;
        }
    }
}