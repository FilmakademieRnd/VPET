/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/v-p-e-t

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;

namespace vpet
{
    public class LightSettingsWidget : MonoBehaviour
    {

        private ColorWheel colorWheel;

        private RangeSlider slider;

        private RangeSlider intensitySlider;

        private RangeSlider rangeSlider;

        private RangeSlider angleSlider;

        private SceneObject currentLight;

        private SliderType sliderType;




        public enum SliderType { INTENSITY, RANGE, ANGLE, COLOR };

        void Awake()
        {
            // create range sliders
            // slider
            GameObject rangeSliderPrefab = Resources.Load<GameObject>("VPET/Prefabs/RangeTemplate");
            if (rangeSliderPrefab == null) Debug.LogError(string.Format("{0}: Cannot load: RangeTemplate.", this.GetType()));
            else
            {
                // intensity
                GameObject sliderInstance = Instantiate(rangeSliderPrefab);
                sliderInstance.name = rangeSliderPrefab.name;
                sliderInstance.transform.SetParent(this.transform, false);
                sliderInstance.transform.localPosition = new Vector3(0, (-VPETSettings.Instance.canvasHalfHeight + 2 * UI.ButtonOffset) * VPETSettings.Instance.canvasAspectScaleFactor, 0);
                sliderInstance.transform.localScale = Vector3.one;
                slider = sliderInstance.GetComponent<RangeSlider>();

                /*
	            // range
	            sliderInstance = Instantiate(rangeSliderPrefab);
	            sliderInstance.name = rangeSliderPrefab.name + "_range";
	            sliderInstance.transform.parent = this.transform;
	            sliderInstance.transform.localPosition = new Vector3(100, 0, 0);
	            sliderInstance.transform.localScale = Vector3.one;
	            rangeSlider = sliderInstance.GetComponent<RangeSlider>();
	            rangeSlider.Callback = this.updateRange;
	
	            // angle
	            sliderInstance = Instantiate(rangeSliderPrefab);
	            sliderInstance.name = rangeSliderPrefab.name + "_angle";
	            sliderInstance.transform.parent = this.transform;
	            sliderInstance.transform.localPosition = new Vector3(400, 0, 0);
	            sliderInstance.transform.localScale = Vector3.one;
	            angleSlider = sliderInstance.GetComponent<RangeSlider>();
	            angleSlider.MaxValue = 179f;
	            angleSlider.Callback = this.updateAngle;
	            */
            }

            // color wheel
            GameObject colorWheelPrefab = Resources.Load<GameObject>("VPET/ColorWheelTemplate");
            if (colorWheelPrefab == null) Debug.LogError(string.Format("{0}: Cannot load: ColorWheelTemplate.", this.GetType()));
            else
            {
                GameObject colorWheelInstance = Instantiate(colorWheelPrefab);
                colorWheelInstance.transform.SetParent(this.transform, false);
                colorWheelInstance.transform.localPosition = new Vector3(-VPETSettings.Instance.canvasHalfWidth + 250, (-VPETSettings.Instance.canvasHalfHeight + 250) * VPETSettings.Instance.canvasAspectScaleFactor, 0);
                colorWheelInstance.transform.localScale = 4 * Vector3.one;
                colorWheel = colorWheelInstance.GetComponent<ColorWheel>();
                colorWheel.Callback = this.updateColor;
            }

        }



        void Start()
        {
            hide();
        }

        public void setSliderType(SliderType type)
        {
            sliderType = type;
        }


        public void hide()
        {
            colorWheel.gameObject.SetActive(false);
            slider.gameObject.SetActive(false);
            /*
	        intensitySlider.gameObject.SetActive(false);
	        rangeSlider.gameObject.SetActive(false);
	        angleSlider.gameObject.SetActive(false);
	        */
        }

        public void show(SceneObject light)
        {
            currentLight = light;

            switch (sliderType)
            {
                case SliderType.INTENSITY:
                    slider.MaxValue = float.MaxValue;
                    slider.Sensitivity = VPETSettings.Instance.lightIntensityFactor / 20f;
                    slider.Value = currentLight.getLightIntensity();
                    slider.Callback = this.updateIntensity;
                    slider.gameObject.SetActive(true);
                    break;
                case SliderType.RANGE:
                    slider.MaxValue = float.MaxValue;
                    slider.Sensitivity = 0.5f;
                    slider.Value = currentLight.getLightRange();
                    slider.Callback = this.updateRange;
                    slider.gameObject.SetActive(true);
                    break;
                case SliderType.ANGLE:
                    slider.MaxValue = 179f;
                    slider.Sensitivity = 0.5f;
                    slider.Value = currentLight.getLightAngle();
                    slider.Callback = this.updateAngle;
                    slider.gameObject.SetActive(true);
                    break;
                case SliderType.COLOR:
                    colorWheel.gameObject.SetActive(true);
                    colorWheel.Value = currentLight.getLightColor();
                    break;
            }


            /*
	        intensitySlider.gameObject.SetActive(true);
	        intensitySlider.Value = currentLight.getLightIntensity();
	        if ( currentLight.isSpotLight  || currentLight.isPointLight )
	        {
	            rangeSlider.gameObject.SetActive(true);
	            rangeSlider.Value = currentLight.getLightRange();
	        }
	        if ( currentLight.isSpotLight )
	        {
	            angleSlider.gameObject.SetActive(true);
	            angleSlider.Value = currentLight.getLightAngle();
	        }
	        */
        }

        private void updateColor(Color color)
        {
            currentLight.setLightColor(color);
        }


        private void updateIntensity(float intensity)
        {
            currentLight.setLightIntensity(intensity);
        }

        private void updateRange(float range)
        {
            currentLight.setLightRange(range);
        }

        private void updateAngle(float angle)
        {
            currentLight.setLightAngle(angle);
        }

    }
}