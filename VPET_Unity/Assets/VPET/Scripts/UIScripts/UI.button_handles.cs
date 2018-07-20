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
﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace vpet
{
	public partial  class UI
	{
	    // Main
		//
		//

	    private void infoRequest()
	    {
	
	    }
	
	    private void togglePerspectives()
	    {
	        if (secondaryMenu.currentLayout == layouts.PERSPECTIVES)
	        {
	            secondaryMenu.switchLayout( secondaryMenu.PreviousLayout);
	        }
	        else
	        {
	            secondaryMenu.switchLayout(layouts.PERSPECTIVES);
	        }
	        secondaryMenu.show();
	    }
	
	
	    private void changeMode(  layouts layout )
	    {
            if (layout == layouts.EDIT || layout == layouts.ANIMATION )
            {
                // switch of point to move camera
                // to generic switch off all tools which might be active through secondary menu
                if (mainController.ActiveMode == MainController.Mode.pointToMoveMode)
                {
                    mainController.togglePointToMoveCamera(false);
                }

                layoutUI = layout;
                secondaryMenu.switchLayout(layout);
                if (mainController.getCurrentSelection() != null)
                {
                    if (mainController.getCurrentSelection().GetComponent<SceneObject>().IsLight)
                    {
                        mainController.ActiveMode = MainController.Mode.lightMenuMode;
                    }
                    //else if (mainController.getCurrentSelection().GetComponent<SceneObject>().isMocapTrigger) // mocap trigger component at object
                    //{
                    //    mainController.ActiveMode = MainController.Mode.objectMenuMode;
                    //    // force re-draw center menu because it won't be re-drawn in state machine if mode doesn't change
                    //    drawCenterMenu(layouts.MOCAP);
                    //}
                    else
                    {
                        mainController.ActiveMode = MainController.Mode.objectMenuMode;
                        // force re-draw center menu because it won't be re-drawn in state machine if mode doesn't change
                        drawCenterMenu(layout);
                    }

                    // activate animcontroller when something is selected
                    if ( layout == layouts.ANIMATION )
                    {
                        animationController.activate();
                    }

                }
            }
            else if (layout == layouts.SCOUT)
            {
                layoutUI = layout;
                secondaryMenu.switchLayout(layout);
                mainController.ActiveMode = MainController.Mode.idle;
            }

            UI.OnUIChanged.Invoke();
            secondaryMenu.show();
            hideRangeSlider();
        }

        private void quitApplication()
	    {
	        // TODO: clean quit through main controller
	        Application.Quit();
	    }

        private void openHelp()
        {
            mainController.helpActive = true;
            helpContext.SetActive(true);
            mainMenu.gameObject.SetActive(false);
            mainMenuButton.SetActive(false);
        }

        public void closeHelp()
        {
            mainController.helpActive = false;
            helpContext.SetActive(false);
            mainMenu.gameObject.SetActive(true);
            mainMenuButton.SetActive(true);
        }
	
	
	    // Secondary
		//
		//
	    private void orthographicCamera(MainController.View view)
	    {
			// reset all button states
			secondaryMenu.reset(); 
	        mainController.setOrthographicCamera(view);
	    }
			
		private void perspectiveCamera()
		{
			secondaryMenu.reset();
			mainController.setPerspectiveCamera();
		}

		private void predefinedCamera()
		{
			secondaryMenu.reset();
			mainController.repositionCamera();
		}

		private void ncamCamera()
		{
			secondaryMenu.reset();
			mainController.toggleNcam();
		}

        private void pubCam()
        {
            secondaryMenu.reset();
            mainController.togglePubCam();
        }

        private void editWidget3D(IMenuButton button)
        {
            mainController.buttonTranslationClicked(button.Toggled);
        }

        private void editLinkToCamera(IMenuButton button)
        {
            parameterMenu.reset();
            mainController.toggleObjectLinkCamera(button.Toggled);
            UI.OnUIChanged.Invoke();
        }

        private void editPointToMove(IMenuButton button)
        {
            parameterMenu.reset();
            mainController.togglePointToMove(button.Toggled);
            UI.OnUIChanged.Invoke();
        }

        private void pointToMoveCamera(IMenuButton button)
        {
            mainController.togglePointToMoveCamera(button.Toggled);
        }

        private void cameraFov(IMenuButton button)
        {
            if (button.Toggled)
                mainController.ConnectRangeSlider(mainController.setCamParamFov, Camera.main.fieldOfView.vFovToLens());
            else
                hideRangeSlider();
        }

        // Center
        //
        //
        private void editTranslation(IMenuButton button)
	    {
            UI.OnUIChanged.Invoke();
            centerMenu.animateActive( ((Button)button).gameObject );
            editWidget3D(button);
	    }
	
		private void editRotation(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonRotationClicked( button.Toggled);
	    }
	
		private void editScale(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonScaleClicked(button.Toggled);
	    }
	
	    private void objectReset()
	    {
	        if (mainController.getCurrentSelection() != null)   
	            mainController.getCurrentSelection().GetComponent<SceneObject>().resetAll();
	    }

        private void triggerMocap(IMenuButton button)
        {
            if (button.Toggled)
                mainController.buttonAnimatorCmdClicked(1); //play
            else
                mainController.buttonAnimatorCmdClicked(2); // stop
        }




        //!
        //! click on light color edit
        //! @param      button      button sent the request
        //!	
        private void editLightColor(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.COLOR);
            mainController.buttonLightColorClicked(button.Toggled);
	    }

        //!
        //! click on light settings edit
        //! @param      button      button sent the request
        //!	
        private void editLightSettings(IMenuButton button)
        {
            centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.INTENSITY); // TODO: just something but not color
            mainController.buttonLightSettingsClicked(button.Toggled);
        }

        //!
        //! click on light angle edit
        //! @param      button      button sent the request
        //!	
        private void editLightAngle(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.ANGLE);
            mainController.buttonLightAngleClicked(button.Toggled);
	    }

        //!
        //! click on light intensity edit
        //! @param      button      button sent the request
        //!	
        private void editLightIntensity(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
	        mainController.buttonLightIntensityClicked(button.Toggled);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.INTENSITY);
        }

        //!
        //! click on light range edit
        //! @param      button      button sent the request
        //!	
        private void editLightRange(IMenuButton button)
        {
            centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.RANGE);
            mainController.buttonLightRangeClicked (button.Toggled);
        }

        //!
        //! click on animation edit
        //! @param      button      button sent the request
        //!	
        private void editAnimation(IMenuButton button)
	    {
            centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonAnimationEditClicked(button.Toggled);
        }

        //!
        //! click on animation delete
        //!	
        private void animationDelete()
	    {
	        animationController.deleteAnimation();
	        //  deleteKeyframe(int.Parse(mainController.getCurrentSelection().name));
	        // animationController.smoothKeyframeTangents(int.Parse(mainController.getCurrentSelection().name));
	    }
	
	
		private void animationAddCueMenu( IMenuButton button )
		{			
			GameObject obj = ((Button)button).gameObject;

			SubMenu subMenu = ((Button)button).gameObject.GetComponent<SubMenu>();

            SceneObject sceneObject = mainController.getCurrentSelection().GetComponent<SceneObject>();

            if (subMenu == null)
			{
                subMenu = ((Button)button).gameObject.AddComponent<SubMenu>();
				subMenu.DirToExpand = SubMenu.direction.RIGHT;

                GameObject buttonTextPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonText");
                // add animation layer buttons
                for (int i = 1; i < 4; ++i)
                {
                    // add animation layer button
                    int layerIndex = i-1;
                    vpet.IMenuButton animLayerButton = vpet.Elements.MenuButtonToggle();
                    // get toggle state
                    animLayerButton.Toggled = sceneObject.animationLayer == layerIndex;
                    animLayerButton.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => animationLayerToggleCurrentObject(animLayerButton, subMenu, layerIndex));
                    // subMenu.OnMenuOpen.AddListener( () => { print("Check for layer " + layerIndex); animLayerButton.Toggled = animationController.IsCurrentSelectionOnLayer(layerIndex); } );
                    UI.OnUIChanged.AddListener(() => { print("Check for layer " + layerIndex); animLayerButton.Toggled = animationController.IsCurrentSelectionOnLayer(layerIndex); });
                    subMenu.addButton(animLayerButton);

                    // add animation layer id on top (text object) 
                    GameObject buttonTextObj = GameObject.Instantiate(buttonTextPrefab);
                    GameObject buttonObj = ((Button)animLayerButton).gameObject;
                    buttonObj.name = "CueAddButton_" + i.ToString();
                    buttonTextObj.transform.parent = buttonObj.transform;
                    buttonTextObj.transform.localScale = new Vector3(1, 1, 1);
                    buttonTextObj.transform.localPosition = new Vector3(0, 0, 0);
                    Text text = buttonTextObj.GetComponent<Text>();
                    if (text != null)
                        text.text = i.ToString();
                }

            }

			if ( !subMenu.isOpen )
			{
				subMenu.show();
			}
			else
			{
                subMenu.hide();
			}
		}

		private void animationLayerAddCurrentObject( SubMenu subMenu, int layerIdx )
		{
			print("Add " + mainController.getCurrentSelection().gameObject +  " to animation layer " + layerIdx );
            animationController.addSelectedObjectToLayer(layerIdx);
            subMenu.hide();
		}

        private void animationLayerToggleCurrentObject( IMenuButton button, SubMenu subMenu, int layerIdx)
        {
            if ( button.Toggled )
            {
                print("Add " + mainController.getCurrentSelection().gameObject + " to animation layer " + layerIdx);
                animationController.addSelectedObjectToLayer(layerIdx);
            }
            else
            {
                print("Remove " + mainController.getCurrentSelection().gameObject + " from animation layer " + layerIdx);
                animationController.removeSelectedObjectFromLayer(layerIdx);
            }
        }




        private void animationFireCueMenu(IMenuButton button)
        {
            GameObject obj = ((Button)button).gameObject;

            SubMenu subMenu = ((Button)button).gameObject.GetComponent<SubMenu>();

            if (subMenu == null)
            {
                subMenu = ((Button)button).gameObject.AddComponent<SubMenu>();
                subMenu.DirToExpand = SubMenu.direction.LEFT;
                subMenu.offset = new Vector2(UI.ButtonOffset, UI.ButtonOffset);
                
                GameObject buttonTextPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonText");
                // add animation layer buttons
                for (int i = 1; i < 4; ++i)
                {
                    int layerIndex = i - 1;
                    vpet.IMenuButton animLayerButton = vpet.Elements.MenuButton();
                    animLayerButton.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => mainController.AnimationController.playAnimationLayer(layerIndex));
                    subMenu.addButton(animLayerButton);
                    GameObject buttonTextObj = GameObject.Instantiate(buttonTextPrefab);
                    GameObject buttonObj = ((Button)animLayerButton).gameObject;
                    buttonTextObj.transform.parent = buttonObj.transform;
                    buttonTextObj.transform.localScale = new Vector3(1, 1, 1);
                    buttonTextObj.transform.localPosition = new Vector3(0, 0, 0);
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



        private void parameterButtonHandle(IMenuButton button, int idx )
        {
            parameterMenu.reset();
            button.Toggled = true;

            // if point to move or link to camera mode switch to translation
            if (mainController.ActiveMode == MainController.Mode.pointToMoveMode || mainController.ActiveMode == MainController.Mode.objectLinkCamera)
                mainController.ActiveMode = MainController.Mode.translationMode;

            rangeSlider.MinValue = float.MinValue;
            rangeSlider.MaxValue = float.MaxValue;

            switch (idx)
            {
                case 0: // X
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateX", 2f*VPETSettings.Instance.controllerSpeed);
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleX", 0.02f);
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateX", 1f);
                    else if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
                    {
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>().setLightIntensity, mainController.getCurrentSelection().GetComponent<SceneObject>().getLightIntensity(), 0.1f/VPETSettings.Instance.lightIntensityFactor);
                        rangeSlider.MinValue = 0f;
                    }
                    break;
                case 1:
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateY", 2f * VPETSettings.Instance.controllerSpeed);
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleY", 0.02f);
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateY", 1f);
                    else if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
                    {
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>().setLightRange, mainController.getCurrentSelection().GetComponent<SceneObject>().getLightRange(), 10f*VPETSettings.Instance.sceneScale);
                        rangeSlider.MinValue = 0.1f;
                    }
                    break;
                case 2:
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateZ", 2f * VPETSettings.Instance.controllerSpeed);
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleZ", 0.02f);
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateZ", 1f);
                    else if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
                    {
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>().setLightAngle, mainController.getCurrentSelection().GetComponent<SceneObject>().getLightAngle(), 1f);
                        rangeSlider.MinValue = 1f;
                        rangeSlider.MaxValue = 179f;
                    }
                    break;
            }
        }
	
}
}