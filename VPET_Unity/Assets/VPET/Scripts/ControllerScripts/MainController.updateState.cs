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
		public Transform oldParent = null;
	
	    //!
		//! Update is called once per frame
		//!
	    void Update ()
	    {
	        //position modifiers if neccessary
	        if (currentSelection)
	        {
	            if (activeMode == Mode.translationMode || activeMode == Mode.animationEditing)
	            {
	                translateModifier.transform.position = currentSelection.position;
	                //translateModifier.transform.position = currentSelection.GetComponent<Collider>().bounds.center;
	
	                translateModifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 15)*(Camera.main.fieldOfView/30);
	                translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
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

                if (hasUpdatedProjectionMatrix)
                {
                    UpdateProjectionMatrixSecondaryCameras();
                    hasUpdatedProjectionMatrix = false;
                }

                // unlock if active mode is none editing mode
                //if (activeMode!=Mode.translationMode && activeMode!=Mode.rotationMode && activeMode != Mode.scaleMode && activeMode != Mode.objectLinkCamera && activeMode != Mode.animationEditing && activeMode != Mode.pointToMoveMode && activeMode != Mode.lightSettingsMode)
                if ( activeMode != oldState)
                {
                    serverAdapter.sendLock(currentSelection, false);
                }

                //properly disable old mode
                switch (oldState) {
	                case (Mode.idle):
	                    break;
	                case (Mode.translationMode):
	                    translateModifier.GetComponent<Modifier>().setVisible(false);
                        if ( activeMode != Mode.objectLinkCamera && activeMode != Mode.pointToMoveMode )
                            ui.drawSecondaryMenu(layouts.EDIT);
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
                            }
                            else
                            {
                                currentSelection.GetComponent<SceneObject>().setKinematic(false);
                                currentSelection.parent = oldParent;
                            }
                        }
                        if (activeMode != Mode.objectLinkCamera && activeMode != Mode.translationMode && activeMode != Mode.pointToMoveMode) ui.drawSecondaryMenu(layouts.EDIT);
                        break;
	                case (Mode.pointToMoveMode):
                        if (activeMode != Mode.objectLinkCamera && activeMode != Mode.translationMode && activeMode != Mode.pointToMoveMode) ui.drawSecondaryMenu(layouts.EDIT);
                        break;
	                case (Mode.objectMenuMode):
	                    break;
	                case (Mode.lightMenuMode):
	                    break;
	                case (Mode.animationEditing):
                        animationController.deactivate();
                        hideModifiers();
                        break;
	                case (Mode.lightSettingsMode):
	                    ui.hideLightSettingsWidget();
                        break;
	                case (Mode.addMode):
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
                        serverAdapter.sendLock(currentSelection, true);
                        translateModifier.GetComponent<Modifier>().setVisible(true);
	                    ui.drawSecondaryMenu(layouts.TRANSLATION);
	                    break;
	                case (Mode.rotationMode):
                        serverAdapter.sendLock(currentSelection, true);
                        rotationModifier.GetComponent<Modifier>().setVisible(true);
	                    break;
	                case (Mode.scaleMode):
                        scaleModifier.GetComponent<Modifier>().setVisible(true);
                        serverAdapter.sendLock(currentSelection, true);
                        break;
	                case (Mode.objectLinkCamera):
	                    if (currentSelection.GetComponent<SceneObject>().isSpotLight ||
	                        currentSelection.GetComponent<SceneObject>().isPointLight ||
	                        currentSelection.GetComponent<SceneObject>().isDirectionalLight)
	                    {
                            oldParent = currentSelection.parent;
	                        currentSelection.parent = Camera.main.transform;
                        }
                        else
	                    {
	                        currentSelection.GetComponent<SceneObject>().setKinematic(true);
							oldParent = currentSelection.parent;
	                        currentSelection.parent = Camera.main.transform;
	                    }
                        serverAdapter.sendLock(currentSelection, true);
                        break;
	                case (Mode.pointToMoveMode):
                        serverAdapter.sendLock(currentSelection, true);
                        break;
	                case (Mode.objectMenuMode):
	                    if (currentSelection && ui.LayoutUI != layouts.SCOUT )
	                    {
	                        if (ui.LayoutUI == layouts.ANIMATION)
	                        {
	                            ui.drawCenterMenu(layouts.ANIMATION);
                                ui.drawSecondaryMenu(layouts.ANIMATION);
	                        }
	                        else
	                        {
	                            ui.drawCenterMenu(layouts.OBJECT);
	                        }
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
	                    ui.drawLightSettingsWidget();
                        serverAdapter.sendLock(currentSelection, true);
                        break;
	                case (Mode.animationEditing):
                        animationController.activate();
                        translateModifier.GetComponent<Modifier>().setVisible(true);
                        serverAdapter.sendLock(currentSelection, true);
                        break;
	                case (Mode.addMode):
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