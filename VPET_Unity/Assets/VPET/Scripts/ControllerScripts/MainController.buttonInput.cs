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
﻿using UnityEngine;
using System.Collections;

//!
//! MainController part handling all inputs by GUI buttons
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! click on the translation button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonTranslationClicked(bool active){
	        if (active){
	            activeMode = Mode.translationMode;
	        }
	        else{
	            activeMode = Mode.idle;
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the rotation button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonRotationClicked(bool active){
	        if (active){
	            activeMode = Mode.rotationMode;
	        }
	        else{
	            activeMode = Mode.idle;
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the scale button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonScaleClicked(bool active){
	        if (active){
	            activeMode = Mode.scaleMode;
	        }
	        else{
	            activeMode = Mode.idle;
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the light intensity button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightIntensityClicked(bool active)
	    {
	        if (active)
	        {
	            ui.lightSettingsWidget.setSliderType(LightSettingsWidget.SliderType.INTENSITY);
	            activeMode = Mode.lightSettingsMode;
	            serverAdapter.sendLock(currentSelection, true);
	        }
	        else
	        {
	            activeMode = Mode.idle;
	            serverAdapter.sendLock(currentSelection, false);
	            openMenu();
	        }
	
	    }
	
	    //!
	    //! click on the light angle button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightAngleClicked(bool active)
	    {
	        if (active)
	        {
	            ui.lightSettingsWidget.setSliderType(LightSettingsWidget.SliderType.ANGLE);
	            activeMode = Mode.lightSettingsMode;
	            serverAdapter.sendLock(currentSelection, true);
	        }
	        else
	        {
	            activeMode = Mode.idle;
	            serverAdapter.sendLock(currentSelection, false);
	            openMenu();
	        }
	    }
	
	    //!
	    //! click on the light range button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightRangeClicked(bool active)
	    {
	        if (active)
	        {
	            ui.lightSettingsWidget.setSliderType(LightSettingsWidget.SliderType.RANGE);
	            activeMode = Mode.lightSettingsMode;
	            serverAdapter.sendLock(currentSelection, true);
	        }
	        else
	        {
	            activeMode = Mode.idle;
	            serverAdapter.sendLock(currentSelection, false);
	            openMenu();
	        }
	    }
	
	
	    //!
	    //! click on the light intensity button
	    //! @param      active      state that the button dows no have (on or off)
	    //!
	    public void buttonLightColorClicked(bool active)
	    {
	        if (active)
	        {
	            ui.lightSettingsWidget.setSliderType(LightSettingsWidget.SliderType.COLOR);
	            activeMode = Mode.lightSettingsMode;
	            serverAdapter.sendLock(currentSelection, true);
	        }
	        else
	        {
	            activeMode = Mode.idle;
	            serverAdapter.sendLock(currentSelection, false);
	            openMenu();
	        }
	
	    }

        //!
        //! click on animation edit
        //! @param      active      state that the button dows no have (on or off)
        //!
        public void buttonAnimationEditClicked(bool active)
        {
            if (active)
            {
                activeMode = Mode.animationEditing;
            }
            else
            {
                activeMode = Mode.idle;
                openMenu();
            }

        }


    }
}