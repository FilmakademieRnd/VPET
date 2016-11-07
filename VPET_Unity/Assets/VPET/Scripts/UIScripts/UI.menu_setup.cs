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
using UnityEngine.UI;
using System.Collections;

namespace vpet
{
	public partial class UI : MonoBehaviour 
	{
        //!
        //! Sets up all buttons for the main menu.
        //!
        private void setupMainMenu()
        {
            // views
            IMenuButton buttonPers = Elements.MenuButtonToggle();
            buttonPers.AddAction(GeneralMenu_Perspective_sel, GeneralMenu_Perspective_nrm, () => togglePerspectives());
            UI.OnUIChanged.AddListener(() => { buttonPers.Toggled = (secondaryMenu.currentLayout == layouts.PERSPECTIVES); });  // register for ui changes 
            mainMenu.addButton(buttonPers);
            // info button
            //
            // modes
            IMenuButton buttonModes = Elements.MenuButtonList();
            buttonModes.AddAction(ModeMenu_EditMode_sel, ModeMenu_EditMode_nrm, () => changeMode(layouts.EDIT));
            buttonModes.AddAction(ModeMenu_AnimationMode_sel, ModeMenu_AnimationMode_nrm, () => changeMode(layouts.ANIMATION)); // anim
            buttonModes.AddAction(ModeMenu_ScoutMode_sel, ModeMenu_ScoutMode_nrm, () => changeMode(layouts.SCOUT)); //scout
            mainMenu.addButton(buttonModes);
            // gyro toggle
            IMenuButton buttonGyro = Elements.MenuButtonToggle();
            buttonGyro.AddAction(GeneralMenu_Gyro_sel, GeneralMenu_Gyro_nrm, () => mainController.toggleCameraRotation()); // for toggle
            buttonGyro.AddHoldAction(() => mainController.resetCameraOffset());
            mainMenu.addButton(buttonGyro);
            // config
            IMenuButton buttonConfig = Elements.MenuButtonToggle();
            buttonConfig.AddAction(GeneralMenu_Settings_sel, GeneralMenu_Settings_nrm, () => drawConfigWidget());
            buttonConfig.AddAction(GeneralMenu_Settings_sel, GeneralMenu_Settings_nrm, () => hideConfigWidget()); // untoggle action
            UI.OnUIChanged.AddListener(() => { buttonConfig.Toggled = (configWidget.gameObject.activeSelf == true); } );  // register for ui changes 
            mainMenu.addButton( buttonConfig);
			mainMenu.addButtonToLayout( buttonConfig, layouts.SPLASH);
			// quit
			IMenuButton buttonQuit = Elements.MenuButton();
			buttonQuit.AddAction(GeneralMenu_OnOff_sel, GeneralMenu_OnOff_nrm, call: () => quitApplication() ); // switch off
			mainMenu.addButton( buttonQuit);
			mainMenu.addButtonToLayout( buttonQuit, layouts.SPLASH);

            // set splash layout
            mainMenu.switchLayout(layouts.SPLASH);
	    }
		
	    //!
	    //! Sets up all buttons for the secondary menu.
	    //!
	    private void setupSecondaryMenu()
	    {
			// add empty layout for edit mode
			secondaryMenu.addLayout(layouts.EDIT);

			// cam from snapshot
			IMenuButton buttonSnap = Elements.MenuButton();
			buttonSnap.AddAction(PerspectiveMenu_Snapshot_sel, PerspectiveMenu_Snapshot_nrm);
			secondaryMenu.addButton( buttonSnap, layouts.PERSPECTIVES);
			// top
			IMenuButton buttonTop = Elements.MenuButtonToggle();
			buttonTop.AddAction(PerspectiveMenu_OrthographicTop_sel, PerspectiveMenu_OrthographicTop_nrm, call: () => orthographicCamera(MainController.View.TOP)); 
			buttonTop.AddAction(PerspectiveMenu_OrthographicTop_sel, PerspectiveMenu_OrthographicTop_nrm, call: () => perspectiveCamera()); 
			secondaryMenu.addButton( buttonTop, layouts.PERSPECTIVES);
			// front
			IMenuButton buttonFront = Elements.MenuButtonToggle();
			buttonFront.AddAction(PerspectiveMenu_OrthographicFront_sel, PerspectiveMenu_OrthographicFront_nrm, call: () => orthographicCamera(MainController.View.FRONT)); 
			buttonFront.AddAction(PerspectiveMenu_OrthographicFront_sel, PerspectiveMenu_OrthographicFront_nrm, call: () => perspectiveCamera() ); 
			secondaryMenu.addButton( buttonFront, layouts.PERSPECTIVES);
			// right
			IMenuButton buttonRight = Elements.MenuButtonToggle();
			buttonRight.AddAction(PerspectiveMenu_OrthographicRight_sel, PerspectiveMenu_OrthographicRight_nrm, call: () => orthographicCamera(MainController.View.RIGHT)); 
			buttonRight.AddAction(PerspectiveMenu_OrthographicRight_sel, PerspectiveMenu_OrthographicRight_nrm, call: () => perspectiveCamera() ); 
			secondaryMenu.addButton( buttonRight, layouts.PERSPECTIVES);
			// left
			IMenuButton buttonLeft = Elements.MenuButtonToggle();
			buttonLeft.AddAction(PerspectiveMenu_OrthographicLeft_sel, PerspectiveMenu_OrthographicLeft_nrm, call: () => orthographicCamera(MainController.View.LEFT)); 
			buttonLeft.AddAction(PerspectiveMenu_OrthographicLeft_sel, PerspectiveMenu_OrthographicLeft_nrm, call: () => perspectiveCamera() ); 
			secondaryMenu.addButton( buttonLeft, layouts.PERSPECTIVES);
			// bottom
			IMenuButton buttonBottom = Elements.MenuButtonToggle();
			buttonBottom.AddAction(PerspectiveMenu_OrthographicBottom_sel, PerspectiveMenu_OrthographicBottom_nrm, call: () => orthographicCamera(MainController.View.BOTTOM));
			buttonBottom.AddAction(PerspectiveMenu_OrthographicBottom_sel, PerspectiveMenu_OrthographicBottom_nrm, call: () => perspectiveCamera() );
			secondaryMenu.addButton( buttonBottom, layouts.PERSPECTIVES);
			// ncam
			IMenuButton buttonNcam = Elements.MenuButtonToggle();
			buttonNcam.AddAction(PerspectiveMenu_PrincipalCam_sel, PerspectiveMenu_PrincipalCam_nrm, call: () => ncamCamera() );  
			secondaryMenu.addButton( buttonNcam, layouts.PERSPECTIVES);
			// predefined
			IMenuButton buttonPre = Elements.MenuButton();
			buttonPre.AddAction(PerspectiveMenu_External_sel, PerspectiveMenu_External_nrm, call: () => predefinedCamera() ); 
			secondaryMenu.addButton( buttonPre, layouts.PERSPECTIVES);
	        // aperture
			IMenuButton buttonApert = Elements.MenuButton();
			buttonApert.AddAction(ScoutMode_Aperture_sel, ScoutMode_Aperture_nrm); //
			secondaryMenu.addButton( buttonApert, layouts.SCOUT);
			// cam settings
			IMenuButton buttonCam = Elements.MenuButton();
			buttonCam.AddAction(ScoutMode_CamSettings_sel, ScoutMode_CamSettings_nrm); //
			secondaryMenu.addButton( buttonCam, layouts.SCOUT);
			// fov
			IMenuButton buttonFov = Elements.MenuButton();
			buttonFov.AddAction(ScoutMode_FocalLength_sel, ScoutMode_FocalLength_nrm); //
			secondaryMenu.addButton( buttonFov, layouts.SCOUT);
			// path record
			IMenuButton buttonRec = Elements.MenuButton();
			buttonRec.AddAction(ScoutMode_PathRecord_sel, ScoutMode_PathRecord_nrm); //
			secondaryMenu.addButton( buttonRec, layouts.SCOUT);
			// snapshot
			IMenuButton buttonScout = Elements.MenuButton();
			buttonScout.AddAction(ScoutMode_Snapshot_sel, ScoutMode_Snapshot_nrm); // 15
			secondaryMenu.addButton( buttonScout, layouts.SCOUT);
			// click to move
			IMenuButton buttonClickMoveCam = Elements.MenuButtonToggle();
			buttonClickMoveCam.AddAction(EditMode_TranslateClickToMove_sel, EditMode_TranslateClickToMove_nrm, call: () => pointToMoveCamera(buttonClickMoveCam) ); // 
			secondaryMenu.addButton( buttonClickMoveCam, layouts.SCOUT);
	        // animation mode buttons
			// previous key
			IMenuButton buttonKeyPrev = Elements.MenuButton();
			buttonKeyPrev.AddAction(AnimationMode_JumpToPreviousKeyframe_sel, AnimationMode_JumpToPreviousKeyframe_nrm, call: () => mainController.AnimationController.previousKeyframe() ); // 
			secondaryMenu.addButton( buttonKeyPrev, layouts.ANIMATION);
			// add timeline // 
			GameObject timeLine = GameObject.Find("GUI/Canvas/UI/TimeLine");
			if (timeLine == null)
			{
				Debug.LogError(string.Format("{0}: Cant Find TimeLine (GUI/Canvas/UI/TimeLine).", this.GetType()));
			}
			else
			{
				secondaryMenu.addObject(timeLine, layouts.ANIMATION);
			}
			// next key
			IMenuButton buttonKeyNext = Elements.MenuButton();
			buttonKeyNext.AddAction(AnimationMode_JumpToNextKeyframe_sel, AnimationMode_JumpToNextKeyframe_nrm, call: () => mainController.AnimationController.nextKeyframe() ); // 
			secondaryMenu.addButton( buttonKeyNext, layouts.ANIMATION);
			// play pause
			IMenuButton buttonPlay = Elements.MenuButtonToggle();
			buttonPlay.AddAction(AnimationMode_Pause_sel, AnimationMode_Play_nrm, () => mainController.AnimationController.togglePlayPause() ); 
			secondaryMenu.addButton( buttonPlay, layouts.ANIMATION);
			
            // fire cue
            IMenuButton buttonPlayCue = Elements.MenuButtonToggle();
            //buttonPlayCue.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => mainController.AnimationController.playAnimationLayer()); // 20
            buttonPlayCue.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => animationFireCueMenu(buttonPlayCue) ); // 20
            secondaryMenu.addButton(buttonPlayCue, layouts.ANIMATION);
							
	        // translation mode buttons
			// 3dwidget
			//IMenuButton buttonTrans = Elements.MenuButtonToggle();
			//buttonTrans.AddAction(EditMode_Translate3DWidget_sel, EditMode_Translate3DWidget_nrm, () => editWidget3D(buttonTrans) ); // 
			//secondaryMenu.addButton( buttonTrans, layouts.TRANSLATION);
			// attach to cam
			IMenuButton buttonAttachCam = Elements.MenuButtonToggle();
			buttonAttachCam.AddAction(EditMode_TranslateAttachToCam_sel, EditMode_TranslateAttachToCam_nrm,() => editLinkToCamera(buttonAttachCam)); // 
            UI.OnUIChanged.AddListener(() => { buttonAttachCam.Toggled = (mainController.ActiveMode == MainController.Mode.objectLinkCamera); }); // register ui changes
			secondaryMenu.addButton( buttonAttachCam, layouts.TRANSLATION);
			// click to move
			IMenuButton buttonClickMove = Elements.MenuButtonToggle();
			buttonClickMove.AddAction(EditMode_TranslateClickToMove_sel, EditMode_TranslateClickToMove_nrm, () => editPointToMove(buttonClickMove) ); // 
            UI.OnUIChanged.AddListener(() => { buttonClickMove.Toggled = (mainController.ActiveMode == MainController.Mode.pointToMoveMode); }); // register ui changes
            secondaryMenu.addButton( buttonClickMove, layouts.TRANSLATION);
	    }

        private void animationFireCueMenu(IMenuButton button)
        {
            GameObject obj = ((Button)button).gameObject;

            SubMenu subMenu = ((Button)button).gameObject.GetComponent<SubMenu>();

            if (subMenu == null)
            {
                subMenu = ((Button)button).gameObject.AddComponent<SubMenu>();
                subMenu.DirToExpand = SubMenu.direction.TOP;

                GameObject buttonTextPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonText");
                // add animation layer buttons
                for (int i = 0; i < 2; ++i) {
                    int layerIndex = i;
                    vpet.IMenuButton animLayerButton = vpet.Elements.MenuButton();
                    animLayerButton.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => mainController.AnimationController.playAnimationLayer(layerIndex));
                    subMenu.addButton(animLayerButton);
                    GameObject buttonTextObj = GameObject.Instantiate(buttonTextPrefab);
                    GameObject buttonObj = ((Button)animLayerButton).gameObject;
                    buttonTextObj.transform.parent = buttonObj.transform;
                    buttonTextObj.transform.localScale = new Vector3(1, 1, 1);
                    buttonTextObj.transform.localPosition = new Vector3(0,0,0);
                    Text text = buttonTextObj.GetComponent<Text>();
                    if (text != null)
                        text.text = i.ToString();
                }
            }

            if (!subMenu.isOpen)
            {
                subMenu.show();
            }
            else
            {
                subMenu.hide();
            }

        }
	
	    private void setupCenterMenu()
	    {
            // translate
            IMenuButton buttonTrans = Elements.MenuButtonToggle();
            buttonTrans.AddAction(EditMode_Translate_sel, EditMode_Translate_nrm, () => editTranslation(buttonTrans));
            centerMenu.addButton(buttonTrans, layouts.OBJECT);
            centerMenu.addButtonToLayout(buttonTrans, layouts.LIGHTDIR);
            centerMenu.addButtonToLayout(buttonTrans, layouts.LIGHTSPOT);
            centerMenu.addButtonToLayout(buttonTrans, layouts.LIGHTPOINT);
            // rotate
            IMenuButton buttonRot = Elements.MenuButtonToggle();
            buttonRot.AddAction(EditMode_Rotate_sel, EditMode_Rotate_nrm, () => editRotation(buttonRot));
            UI.OnUIChanged.AddListener(() => { buttonRot.Toggled = mainController.ActiveMode == MainController.Mode.rotationMode; });
            centerMenu.addButton(buttonRot, layouts.OBJECT);
            centerMenu.addButtonToLayout(buttonRot, layouts.LIGHTDIR);
            centerMenu.addButtonToLayout(buttonRot, layouts.LIGHTSPOT);
            // scale
            IMenuButton buttonScl = Elements.MenuButtonToggle();
            buttonScl.AddAction(EditMode_Scale_sel, EditMode_Scale_nrm, () => editScale(buttonScl));
            centerMenu.addButton(buttonScl, layouts.OBJECT);
            // reset
            IMenuButton buttonReset = Elements.MenuButton();
			buttonReset.AddAction(EditMode_Reset_sel, EditMode_Reset_nrm, call: () => objectReset() );
			centerMenu.addButton( buttonReset, layouts.OBJECT );
			centerMenu.addButtonToLayout( buttonReset, layouts.LIGHTDIR );
			centerMenu.addButtonToLayout( buttonReset, layouts.LIGHTSPOT );
			centerMenu.addButtonToLayout( buttonReset, layouts.LIGHTPOINT );
	        // graviy/kinematic on off
			IMenuButton buttonKin = Elements.MenuButtonToggle();
			buttonKin.AddAction(EditMode_GravityOn_sel, EditMode_GravityOn_nrm,  () => mainController.toggleLockSelectionKinematic() );
            UI.OnUIChanged.AddListener( () => { buttonKin.Toggled = mainController.HasGravityOn(); } );  // register for ui changes 
            centerMenu.addButton( buttonKin, layouts.OBJECT );
            // light color
            IMenuButton buttonLightCol = Elements.MenuButtonToggle();
            buttonLightCol.AddAction(EditMode_LightColour_sel, EditMode_LightColour_nrm, () => editLightColor(buttonLightCol)); // 
            centerMenu.addButton(buttonLightCol, layouts.LIGHTDIR);
            centerMenu.addButtonToLayout(buttonLightCol, layouts.LIGHTSPOT);
            centerMenu.addButtonToLayout(buttonLightCol, layouts.LIGHTPOINT);
            // angle
            IMenuButton buttonLightAng = Elements.MenuButtonToggle();
			buttonLightAng.AddAction(EditMode_LightConeAngle_sel, EditMode_LightConeAngle_nrm, () => editLightAngle(buttonLightAng));
			centerMenu.addButton( buttonLightAng, layouts.LIGHTSPOT );
			// intensity
			IMenuButton buttonLightInt = Elements.MenuButtonToggle();
			buttonLightInt.AddAction(EditMode_LightIntensity_sel, EditMode_LightIntensity_nrm, () => editLightIntensity(buttonLightInt));
			centerMenu.addButton( buttonLightInt, layouts.LIGHTDIR );
			centerMenu.addButtonToLayout( buttonLightInt, layouts.LIGHTSPOT );
			centerMenu.addButtonToLayout( buttonLightInt, layouts.LIGHTPOINT );
			// cue add
			IMenuButton buttonAniCueAdd = Elements.MenuButton();
			buttonAniCueAdd.AddAction(AnimationMode_CueAdd_sel, AnimationMode_CueAdd_nrm, () => animationAddCueMenu( buttonAniCueAdd ) );
			centerMenu.addButton( buttonAniCueAdd, layouts.ANIMATION );
			// cue remove
			IMenuButton buttonAniCueRem = Elements.MenuButton();
			buttonAniCueRem.AddAction(AnimationMode_CueDelete_sel, AnimationMode_CueDelete_nrm); // 10
			centerMenu.addButton( buttonAniCueRem, layouts.ANIMATION );
            // edit animation
            IMenuButton buttonAniEdi = Elements.MenuButtonToggle();
            buttonAniEdi.AddAction(AnimationMode_KeyframeTranslate_sel, AnimationMode_KeyframeTranslate_nrm, () => editAnimation(buttonAniEdi));
            centerMenu.addButton(buttonAniEdi, layouts.ANIMATION);
            // delete animation
            IMenuButton buttonAniRem = Elements.MenuButton();
            buttonAniRem.AddAction(AnimationMode_DeleteKeyframe_sel, AnimationMode_DeleteKeyframe_nrm, call: () => animationDelete());
            centerMenu.addButton(buttonAniRem, layouts.ANIMATION);
        }

}
}