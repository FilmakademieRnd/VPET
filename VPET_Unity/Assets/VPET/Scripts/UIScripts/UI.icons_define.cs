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

namespace vpet
{
    public partial class UI : MonoBehaviour
    {
        private Sprite AnimationMode_CueAdd_nrm;
        private Sprite AnimationMode_CueAdd_sel;
        private Sprite AnimationMode_CueDelete_nrm;
        private Sprite AnimationMode_CueDelete_sel;
        private Sprite AnimationMode_CueFire_nrm;
        private Sprite AnimationMode_CueFire_sel;
        private Sprite AnimationMode_DeleteKeyframe_nrm;
        private Sprite AnimationMode_DeleteKeyframe_sel;
        private Sprite AnimationMode_JumpToNextKeyframe_nrm;
        private Sprite AnimationMode_JumpToNextKeyframe_sel;
        private Sprite AnimationMode_JumpToPreviousKeyframe_nrm;
        private Sprite AnimationMode_JumpToPreviousKeyframe_sel;
        private Sprite AnimationMode_KeyframeBezier_nrm;
        private Sprite AnimationMode_KeyframeBezier_sel;
        private Sprite AnimationMode_KeyframeLinear_nrm;
        private Sprite AnimationMode_KeyframeLinear_sel;
        private Sprite AnimationMode_KeyframeTranslate_nrm;
        private Sprite AnimationMode_KeyframeTranslate_sel;
        private Sprite AnimationMode_Pause_nrm;
        private Sprite AnimationMode_Pause_sel;
        private Sprite AnimationMode_Play_nrm;
        private Sprite AnimationMode_Play_sel;
        private Sprite AnimationMode_Retime_nrm;
        private Sprite AnimationMode_Retime_sel;
        private Sprite AssetLight_nrm;
        private Sprite AssetLight_sel;
        private Sprite AssetObject_nrm;
        private Sprite AssetObject_sel;
        private Sprite EditMode_AnnotationsRead_nrm;
        private Sprite EditMode_AnnotationsRead_sel;
        private Sprite EditMode_AnnotationsWrite_nrm;
        private Sprite EditMode_AnnotationsWrite_sel;
        private Sprite EditMode_AxisX_nrm;
        private Sprite EditMode_AxisX_sel;
        private Sprite EditMode_AxisY_nrm;
        private Sprite EditMode_AxisY_sel;
        private Sprite EditMode_AxisZ_nrm;
        private Sprite EditMode_AxisZ_sel;
        private Sprite EditMode_GravityOn_nrm;
        private Sprite EditMode_GravityOn_sel;
        private Sprite EditMode_LightColour_nrm;
        private Sprite EditMode_LightColour_sel;
        private Sprite EditMode_LightConeAngle_nrm;
        private Sprite EditMode_LightConeAngle_sel;
        private Sprite EditMode_LightIntensity_nrm;
        private Sprite EditMode_LightIntensity_sel;
        private Sprite EditMode_LightSettings_nrm;
        private Sprite EditMode_LightSettings_sel;
        private Sprite EditMode_LightTemperature_nrm;
        private Sprite EditMode_LightTemperature_sel;
        private Sprite EditMode_Reset_nrm;
        private Sprite EditMode_Reset_sel;
        private Sprite EditMode_Rotate_nrm;
        private Sprite EditMode_Rotate_sel;
        private Sprite EditMode_Scale_nrm;
        private Sprite EditMode_Scale_sel;
        private Sprite EditMode_Translate3DWidget_nrm;
        private Sprite EditMode_Translate3DWidget_sel;
        private Sprite EditMode_TranslateAttachToCam_nrm;
        private Sprite EditMode_TranslateAttachToCam_sel;
        private Sprite EditMode_TranslateClickToMove_nrm;
        private Sprite EditMode_TranslateClickToMove_sel;
        private Sprite EditMode_TranslateFingerSwipe_nrm;
        private Sprite EditMode_TranslateFingerSwipe_sel;
        private Sprite EditMode_Translate_nrm;
        private Sprite EditMode_Translate_sel;
        private Sprite GeneralMenu_Gyro_nrm;
        private Sprite GeneralMenu_Gyro_sel;
        private Sprite GeneralMenu_Info_nrm;
        private Sprite GeneralMenu_Info_sel;
        private Sprite GeneralMenu_Main_nrm;
        private Sprite GeneralMenu_Main_sel;
        private Sprite GeneralMenu_Modes_nrm;
        private Sprite GeneralMenu_Modes_sel;
        private Sprite GeneralMenu_OnOff_nrm;
        private Sprite GeneralMenu_OnOff_sel;
        private Sprite GeneralMenu_Perspective_nrm;
        private Sprite GeneralMenu_Perspective_sel;
        private Sprite GeneralMenu_Redo_nrm;
        private Sprite GeneralMenu_Redo_sel;
        private Sprite GeneralMenu_Settings_nrm;
        private Sprite GeneralMenu_Settings_sel;
        private Sprite GeneralMenu_Help_nrm;
        private Sprite GeneralMenu_Help_sel;
        private Sprite GeneralMenu_Undo_nrm;
        private Sprite GeneralMenu_Undo_sel;
        private Sprite ModeMenu_AnimationMode_nrm;
        private Sprite ModeMenu_AnimationMode_sel;
        private Sprite ModeMenu_EditMode_nrm;
        private Sprite ModeMenu_EditMode_sel;
        private Sprite ModeMenu_ScoutMode_nrm;
        private Sprite ModeMenu_ScoutMode_sel;
        private Sprite PerspectiveMenu_External_nrm;
        private Sprite PerspectiveMenu_External_sel;
        private Sprite PerspectiveMenu_Free_nrm;
        private Sprite PerspectiveMenu_Free_sel;
        private Sprite PerspectiveMenu_OrthographicBack_nrm;
        private Sprite PerspectiveMenu_OrthographicBack_sel;
        private Sprite PerspectiveMenu_OrthographicBottom_nrm;
        private Sprite PerspectiveMenu_OrthographicBottom_sel;
        private Sprite PerspectiveMenu_OrthographicFront_nrm;
        private Sprite PerspectiveMenu_OrthographicFront_sel;
        private Sprite PerspectiveMenu_OrthographicLeft_nrm;
        private Sprite PerspectiveMenu_OrthographicLeft_sel;
        private Sprite PerspectiveMenu_Orthographic_nrm;
        private Sprite PerspectiveMenu_OrthographicRight_nrm;
        private Sprite PerspectiveMenu_OrthographicRight_sel;
        private Sprite PerspectiveMenu_Orthographic_sel;
        private Sprite PerspectiveMenu_OrthographicTop_nrm;
        private Sprite PerspectiveMenu_OrthographicTop_sel;
        private Sprite PerspectiveMenu_PrincipalCam_nrm;
        private Sprite PerspectiveMenu_PrincipalCam_sel;
        private Sprite PerspectiveMenu_PubCam_nrm;
        private Sprite PerspectiveMenu_PubCam_sel;
        private Sprite PerspectiveMenu_Snapshot_nrm;
        private Sprite PerspectiveMenu_Snapshot_sel;
        private Sprite PerspectiveMenu_Tango_nrm;
        private Sprite PerspectiveMenu_Tango_sel;
        private Sprite ScoutMode_Aperture_nrm;
        private Sprite ScoutMode_Aperture_sel;
        private Sprite ScoutMode_CamSettings_nrm;
        private Sprite ScoutMode_CamSettings_sel;
        private Sprite ScoutMode_FocalLength_nrm;
        private Sprite ScoutMode_FocalLength_sel;
        private Sprite ScoutMode_PathRecord_nrm;
        private Sprite ScoutMode_PathRecord_sel;
        private Sprite ScoutMode_Snapshot_nrm;
        private Sprite ScoutMode_Snapshot_sel;
        // DOF extension by Tim
        //private Sprite ScoutMode_DOF_nrm;
        //private Sprite ScoutMode_DOF_sel;
        //private Sprite ScoutMode_Focus_nrm;
        //private Sprite ScoutMode_Focus_sel;
        //private Sprite ScoutMode_Visualizer_nrm;
        //private Sprite ScoutMode_Visualizer_sel;
    }
}