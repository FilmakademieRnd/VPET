﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SceneObjectLight : SceneObject
    {
        //!
        //! initial light color at startup, used to reset object (only used when object has a light component)
        //!
        public Color initialLightColor;
        //!
        //! initial light intensity at startup, used to reset object (only used when object has a light component)
        //!
        public float initialLightIntensity;
        //!
        //! initial light range at startup, used to reset object (only used when object has a light component)
        //!
        public float initialLightRange;
        //!
        //! initial light spot angle at startup, used to reset object (only used when object has a light component)
        //!
        public float initialSpotAngle;
        //!
        //! enumeration of available light parameters
        //!
        private enum LightParameter { Intensity, Color, Range, Angle };
        //!
        //! last modified light parameter
        //!
        LightParameter lastModifiedLightParameter;
        //!
        //! is this GameObject a directional light
        //!
        public bool isDirectionalLight = false;
        //!
        //! is this GameObject a spot light
        //!
        public bool isSpotLight = false;
        //!
        //! is this GameObject a point light
        //!
        public bool isPointLight = false;
        //!
        //! is this GameObject a area light
        //!
        public bool isAreaLight = false;

        private Transform lightTarget = null;

        private Transform lightGeo = null;

        private Light sourceLight = null;

        public Light SourceLight
        {
            get { return sourceLight; }
        }

        public float exposure = 3f;

        private Renderer renderer;


        // Start is called before the first frame update
        void Start()
        {
            base.Start();

            // light specific init
            if (transform.childCount > 0)
            {
                lightTarget = transform.GetChild(0);
                if (lightTarget != null)
                {
                    sourceLight = lightTarget.GetComponent<Light>();
                }
            }

            initialLightColor = sourceLight.color;
            initialLightColor.a = 0.25f;
            initialLightIntensity = sourceLight.intensity;

            if (sourceLight.type == LightType.Directional)
            {
                isDirectionalLight = true;
                lightGeo = lightTarget.Find("Arrow");
            }
            else if (sourceLight.type == LightType.Spot)
            {
                isSpotLight = true;
                initialLightRange = sourceLight.range;
                initialSpotAngle = sourceLight.spotAngle;
                lightGeo = lightTarget.Find("Cone");
            }
            else if (sourceLight.type == LightType.Point)
            {
                isPointLight = true;
                initialLightRange = sourceLight.range;
                lightGeo = lightTarget.Find("Sphere");
            }
            else if (sourceLight.type == LightType.Area)
            {
                isAreaLight = true;
                sourceLight.type = LightType.Spot;
                initialLightRange = sourceLight.range;
                initialSpotAngle = sourceLight.spotAngle;
                lightGeo = lightTarget.Find("Cone");
            }

            boxCollider.isTrigger = true; // not interacting
            LightIcon iconScript = lightTarget.Find("LightQuad").GetComponent<LightIcon>();
            iconScript.TargetCollider = boxCollider;
            iconScript.TargetScale = target.lossyScale; // target.localScale;

            renderer = lightGeo.GetComponent<Renderer>();
        }

        // Update is called once per frame
        void Update()
        {
            base.Update();
#if !SCENE_HOST
            //turn on highlight modes
            if (selected && drawGlowAgain)
            {
                if (lightGeo)
                {
                    renderer.enabled = true;
                    this.showHighlighted(lightGeo.gameObject);
                }
                drawGlowAgain = false;
            }
            //turn off highlight mode
            else if (!selected && !drawGlowAgain)
            {
                if (lightGeo)
                {
                    renderer.enabled = false;
                }
                this.showNormal(this.gameObject);
                drawGlowAgain = true;
            }
#endif
        }

        //!
        //! set the light color of this object, if it is a light
        //!
        public Color getLightColor()
        {
            if (isDirectionalLight || isPointLight || isSpotLight || isAreaLight)
            {
                return sourceLight.color;
            }
            return Color.black;
        }

        //!
        //! get the light intensity
        //!
        public float getLightIntensity()
        {
            if (sourceLight != null) return sourceLight.intensity / VPETSettings.Instance.lightIntensityFactor; // / VPETSettings.Instance.sceneScale;
            return 0f;
        }


        //!
        //! get the light range
        //!
        public float getLightRange()
        {
            if (isPointLight || isSpotLight || isAreaLight)
            {
                return sourceLight.range;// / VPETSettings.Instance.sceneScale;
            }
            return float.MaxValue;
        }

        //!
        //! get the light range
        //!
        public float getLightAngle()
        {
            if (isSpotLight || isAreaLight)
            {
                return sourceLight.spotAngle;
            }
            return 150f;
        }

        public void resetAll()
        {
            base.resetAll();

            if (isSpotLight || isPointLight || isDirectionalLight || isAreaLight)
            {
                sourceLight.color = initialLightColor;
                sourceLight.intensity = initialLightIntensity;
                renderer.material.color = initialLightColor;
                serverAdapter.SendObjectUpdate(this, ParameterType.COLOR);
                serverAdapter.SendObjectUpdate(this, ParameterType.INTENSITY);
                serverAdapter.SendObjectUpdate(this, ParameterType.EXPOSURE);
            }
            if (isSpotLight || isPointLight || isAreaLight)
            {
                sourceLight.range = initialLightRange;
                serverAdapter.SendObjectUpdate(this, ParameterType.RANGE);
            }
            if (isSpotLight || isAreaLight)
            {
                sourceLight.spotAngle = initialSpotAngle;
                serverAdapter.SendObjectUpdate(this, ParameterType.ANGLE);
            }
        }

        //!
        //! set the light color of this object, if it is a light
        //! @param      color     new color of the light  
        //!
        public void setLightColor(Color color)
        {
            if (isDirectionalLight || isPointLight || isSpotLight || isAreaLight)
            {
                color.a = 0.25f;
                sourceLight.color = color;
                renderer.material.color = color;
                lastModifiedLightParameter = LightParameter.Color;

                serverAdapter.SendObjectUpdate(this, ParameterType.COLOR);
            }
        }


        //!
        //! set the light intensity of this object, if it is a light
        //! @param      intensity     new intensity of the light  
        //!
        public void setLightIntensity(float intensity)
        {
            if (isDirectionalLight || isPointLight || isSpotLight || isAreaLight)
            {
                sourceLight.intensity = intensity * VPETSettings.Instance.lightIntensityFactor; // * VPETSettings.Instance.sceneScale;
                lastModifiedLightParameter = LightParameter.Intensity;
                serverAdapter.SendObjectUpdate(this, ParameterType.INTENSITY);
            }
        }


        //!
        //! set the light range of this object, if it is a light
        //! @param      range     new range of the light  
        //!
        public void setLightRange(float range)
        {
            if (isPointLight || isSpotLight || isAreaLight)
            {
                sourceLight.range = range;
                lastModifiedLightParameter = LightParameter.Range;
                serverAdapter.SendObjectUpdate(this, ParameterType.RANGE);
            }
        }

        //!
        //! set the light cone angle of this object, if it is a light
        //! @param      angle     new cone angle of the light  
        //!
        public void setLightAngle(float angle)
        {
            if (isSpotLight || isAreaLight)
            {
                sourceLight.spotAngle = angle;
                lastModifiedLightParameter = LightParameter.Angle;
                serverAdapter.SendObjectUpdate(this, ParameterType.ANGLE);
            }
        }

        //!
        //! hide or show the visualization (cone, sphere, arrow) of the light
        //! @param      set     hide-> true, show->false   
        //!
        public override void hideVisualization(bool set)
        {
            Debug.Log("LightGEO turned " + set);
            if (lightGeo)
            {
                renderer.enabled = !set;
            }
        }

        //!
        //! send updates of the last modifications to the network server
        //!
        protected void sendUpdate()
        {
#if !SCENE_HOST
            base.sendUpdate();

            if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
            {
                switch (lastModifiedLightParameter)
                {
                    case (LightParameter.Intensity):
                        serverAdapter.SendObjectUpdate(this, ParameterType.INTENSITY);
                        break;
                    case (LightParameter.Color):
                        serverAdapter.SendObjectUpdate(this, ParameterType.COLOR);
                        break;
                    case (LightParameter.Angle):
                        serverAdapter.SendObjectUpdate(this, ParameterType.ANGLE);
                        break;
                    case (LightParameter.Range):
                        serverAdapter.SendObjectUpdate(this, ParameterType.RANGE);
                        break;
                    default:
                        break;
                }
            }
#endif
        }
    }
}