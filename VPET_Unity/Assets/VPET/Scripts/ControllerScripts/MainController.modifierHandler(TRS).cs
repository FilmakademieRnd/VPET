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
//! MainController part handling interactions with the modifiers
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! ignore dragging
	    //!
	    private bool ignoreDrag = false;
	
	    //!
	    //! prepare scene & selected pbject for modification
	    //! lock the axis for modifications according to the selected modifier
	    //! @param      modifier        link to currently active modifier
	    //!
	    public void handleModifier(Transform modifier){
	        Debug.Log("Hit modifier " + modifier.name + "!");
	        modifier.parent.GetComponent<Modifier>().isUsed();
	        if (activeMode != Mode.animationEditing)
	        {
	            currentSelection.GetComponent<SceneObject>().setKinematic(true);
	        }
	        if (modifier.parent.name == "TranslateModifier"){
	            //translation modifier
	            translateModifier.GetComponent<Modifier>().makeTransparent();
	            modifier.GetComponent<Renderer>().material.color = new Color(1.0f, 185.0f / 255.0f, 55.0f / 255.0f, 1.0f);
	            planeCollider.gameObject.transform.position = currentSelection.position;
	            helperCollider = planeCollider;

				// HACK for orthographic view
				if ( Camera.main.orthographic )
				{
					// same orientation as camera 
					planeCollider.gameObject.transform.rotation = Camera.main.transform.rotation;
					if (modifier.name == "X-Axis"){
						axisLocker = new Vector3(1, 0, 0);
					}
					else if (modifier.name == "Y-Axis"){
						axisLocker = new Vector3(0, 1, 0);
					}
					else if (modifier.name == "Z-Axis"){
						axisLocker = new Vector3(0, 0, 1);
					}
					else if (modifier.name == "XYAxis"){
						axisLocker = new Vector3(1, 1, 0);
					}
					else if (modifier.name == "YZAxis"){
						axisLocker = new Vector3(0, 1, 1);
					}
					else if (modifier.name == "XZAxis"){
						axisLocker = new Vector3(1, 0, 1);
					}
					else if (modifier.name == "MoveToFloorQuad"){
						currentSelection.GetComponent<SceneObject>().moveToFloor();
						translateModifier.transform.position = currentSelection.position;
						translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
						ignoreDrag = true;
					}

				}
				else
				{
		            if (modifier.name == "X-Axis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
		                axisLocker = new Vector3(1, 0, 0);
		            }
		            else if (modifier.name == "Y-Axis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
		                axisLocker = new Vector3(0, 1, 0);
		            }
		            else if (modifier.name == "Z-Axis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
		                axisLocker = new Vector3(0, 0, 1);
		            }
		            else if (modifier.name == "XYAxis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
		                axisLocker = new Vector3(1, 1, 0);
		            }
		            else if (modifier.name == "YZAxis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);
		                axisLocker = new Vector3(0, 1, 1);
		            }
		            else if (modifier.name == "XZAxis"){
		                planeCollider.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
		                axisLocker = new Vector3(1, 0, 1);
		            }
		            else if (modifier.name == "MoveToFloorQuad"){
		                currentSelection.GetComponent<SceneObject>().moveToFloor();
		                translateModifier.transform.position = currentSelection.position;
		                translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
		                ignoreDrag = true;
		            }
				}
	        }
	        else if (modifier.parent.name == "RotationModifier"){
	            //rotation modifier
	            rotationModifier.GetComponent<Modifier>().makeTransparent();
	            modifier.GetComponent<Renderer>().material.color = new Color(modifier.GetComponent<Renderer>().material.color.r, modifier.GetComponent<Renderer>().material.color.g, modifier.GetComponent<Renderer>().material.color.b, 1.0f);
	            rotationCollider.gameObject.transform.position = currentSelection.position;
	            rotationCollider.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 100);
	            helperCollider = rotationCollider;
	            if (modifier.name == "xRotationModifier"){
	                axisLocker = new Vector3(1, 0, 0);
	            }
	            else if (modifier.name == "yRotationModifier"){
	                axisLocker = new Vector3(0, 1, 0);
	            }
	            else if (modifier.name == "zRotationModifier"){
	                axisLocker = new Vector3(0, 0, 1);
	            }
	        }
	        else if (modifier.parent.name == "ScaleModifier"){
	            //scale modifier
	            scaleModifier.GetComponent<Modifier>().makeTransparent();
	            modifier.GetComponent<ModifierComponent>().setColor(new Color(1.0f, 185.0f / 255.0f, 55.0f / 255.0f, 1.0f));
	            planeCollider.gameObject.transform.position = currentSelection.position;
	            planeCollider.gameObject.transform.rotation = currentSelection.rotation;
	            if (modifier.name == "xScale"){
	                axisLocker = new Vector3(1, 0, 0);
	            }
	            else if (modifier.name == "yScale"){
	                axisLocker = new Vector3(0, 1, 0);
	            }
	            else if (modifier.name == "zScale"){
	                planeCollider.gameObject.transform.Rotate(90, 0, 0);
	                axisLocker = new Vector3(0, 0, 1);
	            }
                else
                {
                    axisLocker = Vector3.one;
                }
	        }
	    }
	
	    //!
	    //! reset modifiers and push changes to server if neccessary
	    //!
	    public void resetModifiers(){
	        //reset transparency
	        translateModifier.GetComponent<Modifier>().resetColors();
	        rotationModifier.GetComponent<Modifier>().resetColors();
	        scaleModifier.GetComponent<Modifier>().resetColors();
	        ignoreDrag = false;
	        initialScaleDistance = float.NaN;
	        if (currentSelection && AnimationData.Data.getAnimationClips(currentSelection.gameObject) == null && !animationController.editingPosition) currentSelection.GetComponent<SceneObject>().setKinematic(false);
	
	        //add modification to undo/redo stack
	        if (activeMode == Mode.translationMode)
	        {
	            undoRedoController.addAction();
	        }
	        else if (activeMode == Mode.rotationMode)
	        {
	            undoRedoController.addAction();
	        }
	        else if (activeMode == Mode.scaleMode)
	        {
	            undoRedoController.addAction();
	        }
	
	        //push changes to server
	        if (!liveMode && activeMode != Mode.animationEditing){
	            if (currentSelection) currentSelection.GetComponent<SceneObject>().sendUpdate();
	        }
	    }
	
	    //!
	    //! hide all modifiers
	    //!
	    public void hideModifiers()
	    {
	        translateModifier.GetComponent<Modifier>().setVisible(false);
	        rotationModifier.GetComponent<Modifier>().setVisible(false);
	        scaleModifier.GetComponent<Modifier>().setVisible(false);
	    }

        private Vector3 getModifierScale()
        {
            if (Camera.main.orthographic)
            {
                return Vector3.one * Camera.main.orthographicSize / 4f;
            }
            else
            {
                return Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.position) / 15) * (Camera.main.fieldOfView / 30);
            }
        }


    }
}