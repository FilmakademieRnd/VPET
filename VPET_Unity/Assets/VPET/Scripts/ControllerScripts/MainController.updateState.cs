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
//! MainController part handling the update of states (executes state machine)
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
		
	    // cache old state
	    private Mode oldState = Mode.idle;
	    private Transform oldSelection = null;
	    private Transform backupSelection = null;
		public Transform oldParent = null;
	
	    //!
		//! Update is called once per frame
		//!
	    void Update ()
	    {
	        //position modifiers if neccessary
	        if (currentSelection)
	        {
	            if (activeMode == Mode.translationMode || (activeMode == Mode.animationEditing && animationController.editingPosition == true))
	            {
	
	                translateModifier.transform.position = currentSelection.position;
	                //translateModifier.transform.position = currentSelection.GetComponent<Collider>().bounds.center;
	
	                translateModifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 15)*(Camera.main.fieldOfView/30);
	                translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
	                if(activeMode == Mode.animationEditing)
	                {
	                    translateModifier.GetComponent<Modifier>().setVisible(true);
	                }
	            }
	            else if (activeMode == Mode.rotationMode)
	            {
	                rotationModifier.transform.position = currentSelection.position;
	                rotationModifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 15) * (Camera.main.fieldOfView / 30);
	            }
	            else if (activeMode == Mode.scaleMode)
	            {
	                scaleModifier.transform.position = currentSelection.position;
	                scaleModifier.transform.rotation = currentSelection.rotation;
	                scaleModifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 15) * (Camera.main.fieldOfView / 30);
	            }
	        }
	
	        //state machine
	        if (oldState != activeMode || currentSelection != oldSelection)
	        {
	            print(oldState.ToString() + " >>>>> " + activeMode );
	
	            //properly disable old mode
	            switch (oldState) {
	                case (Mode.idle):
	                    break;
	                case (Mode.translationMode):
	                    translateModifier.GetComponent<Modifier>().setVisible(false);
	                    if (activeMode != Mode.pointToMoveMode && activeMode != Mode.objectLinkCamera)
	                    {
	                        ui.drawSecondaryMenu(layouts.EDIT);
	                    }
	                    break;
	                case (Mode.rotationMode):
	                    rotationModifier.GetComponent<Modifier>().setVisible(false);
	                    break;
	                case (Mode.scaleMode):
	                    scaleModifier.GetComponent<Modifier>().setVisible(false);
	                    break;
	                case (Mode.objectLinkCamera):
                        if (currentSelection)
                        {
                            if (currentSelection.GetComponent<SceneObject>().isSpotLight ||
                                currentSelection.GetComponent<SceneObject>().isPointLight ||
                                currentSelection.GetComponent<SceneObject>().isDirectionalLight)
                            {
                                currentSelection.parent = oldParent;
                                //undoRedoController.addAction();
                            }
                            else
                            {
                                currentSelection.GetComponent<SceneObject>().setKinematic(false);
                                currentSelection.parent = oldParent;
                                //undoRedoController.addAction();
                            }
                        }
	                    if (activeMode != Mode.pointToMoveMode && activeMode != Mode.translationMode)
	                    {
	                        ui.drawSecondaryMenu(layouts.EDIT);
	                    }
	                    break;
	                case (Mode.pointToMoveMode):
	                    if (backupSelection)
	                    {
	                        currentSelection = backupSelection;
	                        backupSelection = null;
	                    }
	                    if (activeMode != Mode.translationMode && activeMode != Mode.objectLinkCamera)
	                    {
	                        ui.drawSecondaryMenu(layouts.EDIT);
	                    }
	                    break;
	                case (Mode.oneForAllMode):
	                    if (currentSelection){
	                        currentSelection.GetComponent<SceneObject>().setKinematic(false);
	                    }
	                    break;
	                case (Mode.objectMenuMode):
	                    // ui.hideCenterMenu();
	                    break;
	                case (Mode.lightMenuMode):
	                    //ui.hideCenterMenu();
	                    break;
	                case (Mode.animationEditing):
	                    break;
	                case (Mode.lightSettingsMode):
	                    //if (currentSelection && activeMode != Mode.idle) currentSelection.GetComponent<SceneObject>().hideLightVisualization(false);
	                    ui.hideLightSettingsWidget();
	                    // if (currentSelection) ui.hideCenterMenu();
	                    break;
	                case (Mode.addMode):
	                    break;
	                case (Mode.scoutMode):
	                    break;
	                default:
	                    break;
	            }
	            //enable new mode
	            switch (activeMode)
	            {
	                case (Mode.idle):
	                    ui.hideCenterMenu();
	                    break;
	                case (Mode.translationMode):
	                    translateModifier.GetComponent<Modifier>().setVisible(true);
	                    ui.drawSecondaryMenu(layouts.TRANSLATION);
	                    break;
	                case (Mode.rotationMode):
	                    rotationModifier.GetComponent<Modifier>().setVisible(true);
	                    break;
	                case (Mode.scaleMode):
	                    scaleModifier.GetComponent<Modifier>().setVisible(true);
	                    break;
	                case (Mode.objectLinkCamera):
	                    if (currentSelection.GetComponent<SceneObject>().isSpotLight ||
	                        currentSelection.GetComponent<SceneObject>().isPointLight ||
	                        currentSelection.GetComponent<SceneObject>().isDirectionalLight)
	                    {
	                        backupSelection = currentSelection;
                            oldParent = currentSelection.parent; //.parent;
	                        currentSelection.parent = Camera.main.transform;
                            //currentSelection.parent.parent = Camera.main.transform;
                        }
                        else
	                    {
	                        currentSelection.GetComponent<SceneObject>().setKinematic(true);
							oldParent = currentSelection.parent;
	                        currentSelection.parent = Camera.main.transform;
	                    }
	                    break;
	                case (Mode.pointToMoveMode):
	                    
	                    break;
	                case (Mode.oneForAllMode):
	                    axisLocker = new Vector3(1, 1, 1);
	                    if (currentSelection){
	                        currentSelection.GetComponent<SceneObject>().setKinematic(true);
	                        planeCollider.gameObject.transform.position = currentSelection.position;
	                        planeCollider.gameObject.transform.rotation = Camera.main.transform.rotation;
	                        rotationCollider.gameObject.transform.position = currentSelection.position;
	                    }
	                    break;
	                case (Mode.objectMenuMode):
	                    if (currentSelection && ui.LayoutUI != layouts.SCOUT )
	                    {
	                        if (ui.LayoutUI == layouts.ANIMATION)
	                        {
	                            ui.drawCenterMenu(layouts.ANIMATION);
	                        }
	                        else
	                        {
	                            ui.drawCenterMenu(layouts.OBJECT);
	                        }
	                        // ui.drawObjectModificationMenu(AnimationData.Data.getAnimationClips(currentSelection.gameObject) != null, currentSelectionisKinematic());
	                    }
	                    break;
	                case (Mode.lightMenuMode):
	                    if (currentSelection && ui.LayoutUI != layouts.SCOUT)
	                    {
	                        if (currentSelection.GetComponent<SceneObject>().isDirectionalLight)
	                            ui.drawCenterMenu(layouts.LIGHTDIR);
	                            //ui.drawDirectionLightModificationMenu();
	                        else if (currentSelection.GetComponent<SceneObject>().isPointLight)
	                            ui.drawCenterMenu(layouts.LIGHTPOINT);
	                            //ui.drawPointLightModificationMenu();
	                        else if (currentSelection.GetComponent<SceneObject>().isSpotLight)
	                            ui.drawCenterMenu(layouts.LIGHTSPOT);
	                            //ui.drawSpotLightModificationMenu();
	                    }
	                    break;
	                case (Mode.lightSettingsMode):
                        if (currentSelection)
                        {
                            currentSelection.GetComponent<SceneObject>().hideLightVisualization(true);
                        }
	                    //ui.drawColorIntensityPicker();
	                    ui.drawLightSettingsWidget();
	                    break;
	                case (Mode.animationEditing):
	                    if (currentSelection)
	                    {
	                        ui.drawCenterMenu(layouts.ANIMATION);
	                        animationController.updateTimelineKeys();
	                    }
	                    break;
	                case (Mode.addMode):
	                    break;
	                case (Mode.scoutMode):
	                    ui.hideCenterMenu();
	                    break;
	                default:
	                    break;
	            }
	            //renew old state
	            oldState = activeMode;
	            oldSelection = currentSelection;
	        }
		}
}
}