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
﻿using UnityEngine;
using System.Collections;

//!
//! MainController part handling selection and deselection of objects
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! handles the case when a user points to any object on screen
	    //! @param      sObject     Pointer to the object, selected by user
	    //!
	    public void handleSelection(Transform sObject)
	    {
	        Debug.Log("Raycast executed: " + sObject.name);
	
	        if (currentSelection){
	            //currently an object is selected
	
	            //user is pointing at WorldCollider
	            if (sObject.tag == "WorldCollider"){
	                this.deselect();
					activeMode = Mode.idle;
	                return;
	            }
	
	            //user is pointing at the MoveToFloorQuad
	            if (sObject.name == "MoveToFloorQuad")
	            {
	                //move current selection to floor
	                if (currentSelection.GetComponent<BoxCollider>())
	                {
	                    currentSelection.position = new Vector3(currentSelection.position.x, currentSelection.position.y - currentSelection.GetComponent<BoxCollider>().bounds.min.y, currentSelection.position.z);
	                }
	                else if (currentSelection.GetComponent<KeyframeScript>())
	                {
	                    currentSelection.position = new Vector3(currentSelection.position.x, currentSelection.position.y - currentSelection.GetComponent<Renderer>().bounds.min.y, currentSelection.position.z);
	                    currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
	                }
	                return;
	            }
	
	            //user is pointing at currently selected object
	            if (currentSelection == sObject)
	            {
	                if (sObject.GetComponent<SceneObject>().isDirectionalLight || sObject.GetComponent<SceneObject>().isSpotLight || sObject.GetComponent<SceneObject>().isPointLight)
	                {
	                    activeMode = Mode.lightMenuMode;
	                }
	                else
	                {
	                    if (activeMode != Mode.animationEditing)
	                    {
	                        activeMode = Mode.objectMenuMode;
	                    }
	                    else
	                    {
	                        if (animationController.editingPosition)
	                        {
	                            if (!currentSelection.GetComponent<KeyframeScript>())
	                            {
	                                deselect();
	                            }
	                            else
	                            {
	                                deselect();
	                                animationController.enablePositionEditing();
	                            }
	                        }
	                        else
	                        {
	                            animationController.enablePositionEditing();
	                        }
	                    }
	                }
	                return;
	            }
	
	            //user is pointing at a keyframe
	            if (activeMode == Mode.animationEditing && sObject.GetComponent<KeyframeScript>())
	            {
	                backupSelection = currentSelection;
	                /////////////////////////////////////////////////////////////
	                /////////////////////////////////////////////////////////////
	
	                currentSelection = frameSphereContainer.transform.FindChild(sObject.name);
	
	
	
	                // ui.drawKeyframeMenu();
	                
	                
	                
	                
	                //animationController.editingPosition = true;
	                return;
	            }
	
	            //user is pointing at the object on which animation editing is currently applied (had selected a keyframe before)
	            if (backupSelection)
	            {
	                if (activeMode == Mode.animationEditing && sObject.name == backupSelection.name)
	                {
	                    currentSelection = backupSelection;
	                    backupSelection = null;
	                    animationController.editingPosition = true;
	                    return;
	                }
	            }
	
	            //user is pointing at another sceneObject
	            if (sObject.GetComponent<SceneObject>())
	            {
	                if (activeMode == Mode.addMode)
	                {
	                    print("TODO: Add mode!");
	                }
	                else
	                {
	                    this.deselect();
	                    if (activeMode != Mode.animationEditing)
	                    {
	                        // this.deselect();
	                        this.select(sObject);
	                    }
	                }
	            }
	            else {
	                //user is pointing at other object, not beeing sceneObject
	                if (activeMode != Mode.animationEditing)
	                {
	                    this.deselect();
						activeMode = Mode.idle;
	                }
	            }
	        }
	        else if (sObject.GetComponent<SceneObject>())
			{
	            this.select(sObject);
	        }
	
	    }
	
	
	    //!
	    //! select an object
	    //! @param      sObject     Pointer to the object, selected by user
	    //!
	    private void select(Transform sObject){
	
	        if (sObject.GetComponent<SceneObject>().locked)
	            return;
	
	        //select object and execute connected tasks

	        //cache current selection
	        currentSelection = sObject;

			print("Select " + currentSelection);

	        //show selection
	        sObject.gameObject.GetComponent<SceneObject>().selected = true;
	
	        if (ui.LayoutUI == layouts.ANIMATION )
	        {            
	            // TODO: ??
	            activeMode = Mode.animationEditing; 
	        }
	        else if ( ui.LayoutUI == layouts.SCOUT )
	        {
	
	        }
	        else
	        {
	            if (sObject.GetComponent<SceneObject>().isDirectionalLight || sObject.GetComponent<SceneObject>().isSpotLight || sObject.GetComponent<SceneObject>().isPointLight)
	            {
	                activeMode = Mode.lightMenuMode;
	            }
	            else
	            {
					if ( !(activeMode == Mode.translationMode || activeMode == Mode.rotationMode || activeMode == Mode.scaleMode) )
					{
		                activeMode = Mode.objectMenuMode;
					}
	            }
	        }
	
	        // update keys on timeline
	        if ( activeMode == Mode.animationEditing )
	        {
	            animationController.updateTimelineKeys();
	        }
	
	    }
	
	    //!
	    //! unselect an object or reset state
	    //!
	    private void deselect()
	    {

            if (currentSelection)
			{
	            if (animationController.isActive)
	            {
	                if (!animationController.editingPosition)
	                {
	                    //deactivate animation editing of previous object
	                    animationController.deactivate();
	                }
	                else
	                {
	                    if(currentSelection.GetComponent<KeyframeScript>())
	                    {
	                        currentSelection = backupSelection;
	                        backupSelection = null;
	                    }
	                    animationController.editingPosition = false;
	                    hideModifiers();
	                    return;
	                }
	            }

	            if (!currentSelection.GetComponent<Light>())
	            {
	                //currentSelection.GetComponent<SceneObject>().setKinematic(false);
	            }


                //reset Mode
                print("Deselect " + currentSelection);

                // make sure its not more locked
                serverAdapter.sendLock(currentSelection, false);

                if ( activeMode == Mode.objectLinkCamera)
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


                currentSelection.gameObject.GetComponent<SceneObject>().selected = false;
                currentSelection = null;


                // do not always set to idle: activeMode = Mode.idle;



	        }
	        
	    }
}
}