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

        private SceneObject currentLight;

        private SliderType sliderType;


        public enum SliderType { INTENSITY, RANGE, ANGLE, COLOR };

        void Awake()
        {
            // get color wheel
            colorWheel = transform.GetComponentInChildren<ColorWheel>();
            if (colorWheel == null) Debug.LogWarning(string.Format("{0}: Cant Find Component in Canvas: ColorWheel.", this.GetType()));
            else
            {
                colorWheel.Callback = this.updateColor;
            }
        }

        public void SetSliderType(SliderType type)
        {
            sliderType = type;
        }

        public SliderType GetSliderType()
        {
            return sliderType;
        }

        public void hide()
        {
            colorWheel.gameObject.SetActive(false);
        }

        public void show(SceneObject light)
        {
            currentLight = light;

            if (sliderType == SliderType.COLOR)
            {
                colorWheel.gameObject.SetActive(true);
                colorWheel.Value = currentLight.getLightColor();
            }
            else
            {
                colorWheel.gameObject.SetActive(false);
            }
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