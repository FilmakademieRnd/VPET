using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET spot light object as a specialisation of the light object
    //!
    public class SpotLightObject : LightObject
    {
        private Parameter<float> spotAngle;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            if (_light)
            {
                spotAngle = new Parameter<float>(_light.spotAngle);
            }
            else
                Helpers.Log("no light component found!");
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            if (_light.spotAngle != spotAngle.value)
                spotAngle.value = _light.spotAngle;
        }
    }
}
