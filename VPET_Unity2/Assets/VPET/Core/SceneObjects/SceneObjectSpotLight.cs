/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "SceneObjectPointLight.cs"
//! @brief implementation SceneObjectSpotLight as a specialisation of the light object.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET spot light object as a specialisation of the light object
    //!
    public class SceneObjectSpotLight : SceneObjectLight
    {
        //!
        //! the range of the light
        //!
        public Parameter<float> range;
        //!
        //! the angle of the lights spot
        //!
        public Parameter<float> spotAngle;

        //!
        //! Factory to create a new SceneObject and do it's initialisation.
        //! Use this function instead GameObject.AddComponen<>!
        //!
        //! @param gameObject The gameObject the new SceneObject will be attached to.
        //! @sceneID The scene ID for the new SceneObject.
        //!
        public static new SceneObjectSpotLight Attach(GameObject gameObject, byte sceneID = 254)
        {
            SceneObjectSpotLight obj = gameObject.AddComponent<SceneObjectSpotLight>();
            obj.Init(sceneID);

            return obj;
        }

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            if (_light)
            {
                range = new Parameter<float>(_light.range, "range", this);
                range.hasChanged += updateRange;
                spotAngle = new Parameter<float>(_light.spotAngle, "spotAngle", this);
                spotAngle.hasChanged += updateAngle;
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
            range.hasChanged -= updateRange;
            spotAngle.hasChanged -= updateAngle;
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
        //! Update the spot light angle of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new angle value
        //!
        private void updateAngle(object sender, float a)
        {
            _light.spotAngle = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //! 
        //! Function updating the scene objects light parameter spotAngle.
        //! The function is called once per Unity frame call to copy the 
        //! UnityLight value to the VPET parameter.
        //! 
        public override void Update()
        {
            base.Update();
            if (_light.spotAngle != spotAngle.value)
                spotAngle.value = _light.spotAngle;
            if (_light.range != range.value)
                range.value = _light.range;
        }

    }
}
