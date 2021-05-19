using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET light object as a specialisation of the SceneObject
    //!
    public class LightObject : SceneObject
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
                color = new Parameter<Color>(_light.color);
                intensity = new Parameter<float>(_light.intensity);
                range = new Parameter<float>(_light.range);
            }
            else
                Helpers.Log("no light component found!");

        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            updateLightParameters();
        }

        //!
        //! updates the Unity light component specific parameters and informs all connected VPET parameters about the change
        //!
        private void updateLightParameters()
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