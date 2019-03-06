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
*/﻿

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//!
//! 
//! Controller receiving inputs from devicces, for example mouse & keyboard inputs or touch gestures
//! connects to scipts providing device implementations and calls funtion on main controller
//!
namespace vpet
{
	public class InputAdapter : MonoBehaviour
	{
	
		//!
		//! Cached reference to main controller.
		//!
		private MainController mainController;

        //!
        //! Cached reference to scene
        //!
        private GameObject scene;

        //!
        //! Cached reference to main controller.
        //!
        private HelpScreen helpMenu;
	
		//!
		//! null Vector alternative
		//!
		static Vector3 nullVector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	
		//!
		//! camera movement buffer
		//!
		Vector3 camMovePos = Vector3.zero;
		//!
		//! is the finger or mouse currently on a modifier
		//!
		private bool pointerOnModifier = false;
		//!
		//! last screen position a raycast hit an object on
		//!
		private Vector3 hitPositionBuffer = nullVector;
        //!
        //! pause the triggering of events (avoids double executions)
        //!
        public bool pause = false;
	
		//!
		//! multiply move forward
		//!
		// private float forwardSpeed = 200f;
	
		//!
		//! plane on wich raycast is executed when in pointToMove mode
		//!
        private Plane targetPlane;
	
		//!
		//! is the user currently editing lights
		//!
		private bool editingLight = false;
	
		//!
		//! is currently one pointer down (pressed)
		//!
		public bool pointerDown = false;
	
		//!
		//! layers to raycast against in the default configuration
		//!
		static int defaultLayermask = (1 << 0) | (1 << 13);
	
		private TouchInput touchInput = null;
	
		private MouseInput mouseInput = null;
	
		void Awake()
		{
	
			mainController = GameObject.Find("MainController").GetComponent<MainController>();
            scene = GameObject.Find("Scene");
            helpMenu = GameObject.Find("GUI/Canvas/HelpScreen").GetComponent<HelpScreen>();

            // declare touch input
            // #if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN) && !UNITY_EDITOR
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN) && !UNITY_EDITOR
			touchInput = gameObject.AddComponent<TouchInput>();
			mainController.TouchInputActive = true;
#endif


            // declare mouse input
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            mouseInput = gameObject.AddComponent<MouseInput>();
            mainController.MouseInputActive = true;
#endif

        }
	
	    //!
	    //! single touch start (called by Mouse or Touch Input)
	    //! @param      pos     screen position of pointer
	    //!
	    public void singlePointerStarted(Vector3 pos){

			pointerDown = true;
	
            if (mainController.helpActive)
            {
                helpMenu.initPos(pos);
                return;
            }
			if (!pointerOnGUI())
			{
				//pointToMove active
				if (mainController.ActiveMode == MainController.Mode.pointToMoveMode)
				{
                    Transform selection = mainController.getCurrentSelection();
                    targetPlane = new Plane(Vector3.up, scene.transform.position);
                    mainController.showPointToMoveIdentifier(planeRaycast(pos, targetPlane));
					return;
				}
	
				//standard modification modes (modifier active)
				if (mainController.ActiveMode == MainController.Mode.translationMode ||
					mainController.ActiveMode == MainController.Mode.rotationMode ||
					mainController.ActiveMode == MainController.Mode.scaleMode ||
					mainController.ActiveMode == MainController.Mode.animationEditing)
				{
					//editing mode is active
					GameObject hitObject = cameraRaycast(pos, 256); //raycast only RenderInFront Layer (layer 9 -> 256)
					if (hitObject)
					{
						//Modifier object was hit
						if (hitObject.tag == "Modifier")
						{
							//modifier was hit
							mainController.handleModifier(hitObject.transform);
                            hitPositionBuffer = planeRaycast(pos, mainController.helperPlane);
                            pointerOnModifier = true;
						}
						hitObject = null;
					}
					else
					{
						//no modifier was hit
						hitObject = cameraRaycast(pos, defaultLayermask);
						if (hitObject)
						{
							//other object was hit
							mainController.handleSelection(hitObject.transform);
							pause = true;
							hitObject = null;
						}
                        else
                        {
                            mainController.handleSelection();
                        }
					}
				}
			}
			else
			{
				pause = true;
			}
		}
	
		//!
		//! single touch ended (called by Mouse or Touch Input)
		//! @param      pos     screen position of pointer
		//!
		public void singlePointerEnded(Vector3 pos)
		{
	
			pointerDown = false;
	
            if (mainController.helpActive)
            {
                helpMenu.startSnap();
                return;
            }

			if (mainController.ActiveMode == MainController.Mode.lightSettingsMode && editingLight)
			{
				editingLight = false;
				return;
			}
				
			if (!pause)
			{ //pause is active when avaoiding double interactions
				if (!pointerOnGUI())
				{

					//pointToMove active
					if (mainController.ActiveMode == MainController.Mode.pointToMoveMode)
					{
                        mainController.hidePointToMoveIdentifier(planeRaycast(pos, targetPlane));
						return;
					}
	
					if (!(mainController.ActiveMode == MainController.Mode.translationMode ||
						mainController.ActiveMode == MainController.Mode.rotationMode ||
						mainController.ActiveMode == MainController.Mode.scaleMode ||
						mainController.ActiveMode == MainController.Mode.animationEditing))
					{
						//selection mode is active
						GameObject hitObject = cameraRaycast(pos,defaultLayermask);
						if (hitObject)
                        {
							//Object was hit
							mainController.handleSelection(hitObject.transform);
							hitObject = null;
						}
                        else
                        {
                            mainController.handleSelection();
                        }
					}
					else
                    {
						//editing mode is active
						if (pointerOnModifier)
						{
							pointerOnModifier = false;
							mainController.resetModifiers();
							hitPositionBuffer = nullVector;

                            if (mainController.UIAdapter.LayoutUI == layouts.ANIMATION)
                            // if (mainController.AnimationController.IsActive)				
                            {
                                mainController.AnimationController.setKeyFrame();
							}
						}
					}
				}
			}
			else
			{
				pause = false;
			}
	
			//reset variables
			pointerOnModifier = false;
		}
	
		//!
		//! single pointer down & moving (drag) (called by Mouse or Touch Input)
		//! @param      pos     screen position of pointer
		//!
		public void singlePointerDrag(Vector3 pos) {
            if(mainController.helpActive && pos.x < 4000f) //Fix for iPad (TaskManager swipe up will generate huge pos value)
            {
                helpMenu.moveMenu(pos);
                return;
            }
			if (!pointerOnGUI())
			{
                if (!pause)
				{
					//pointToMove active
					if (mainController.ActiveMode == MainController.Mode.pointToMoveMode)
					{
                        mainController.movePointToMoveIdentifier(planeRaycast(pos, targetPlane));
						return;
					}
	
					//standard modification modes (modifier active)
					if (mainController.ActiveMode == MainController.Mode.translationMode ||
						mainController.ActiveMode == MainController.Mode.rotationMode ||
						mainController.ActiveMode == MainController.Mode.scaleMode ||
						mainController.ActiveMode == MainController.Mode.animationEditing)
					{
						if (pointerOnModifier){
                            //Pointer is down on modifier
                            Vector3 newHitPosition = planeRaycast(pos, mainController.helperPlane);

                            if ( newHitPosition != nullVector && hitPositionBuffer != nullVector )
							{
								mainController.pointerDrag( hitPositionBuffer, newHitPosition );
							}
						}
					}
				}
			}
		}


        //!
        //! dual touch started (called by Mouse or Touch Input)
        //! @param      pos     screen position of pointer
        //!
        public void twoPointerStarted(Vector3 pos)
        {
            if (!pointerOnGUI())
            {
                camMovePos = pos;
            }
		}
	
		//!
		//! dual touch ended (called by Mouse or Touch Input)
		//! @param      pos     screen position of pointer
		//!
		public void twoPointerEnded(Vector3 pos)
        {
            if (!pointerOnGUI())
            {
                camMovePos = Vector3.zero;

                // HACK: to key in linktocamera mode
                //
                //if (mainController.ActiveMode == MainController.Mode.objectLinkCamera && mainController.UIAdapter.LayoutUI == layouts.ANIMATION)
                //    mainController.AnimationController.setKeyFrame();


                // save the camera offset to restore it next time the app is started
                mainController.saveCameraOffset();
            }
		}
	
		//!
		//! dual touch down & moving (called by Mouse or Touch Input)
		//! @param      pos     screen position of second pointer
		//!
		public void twoPointerDrag(Vector3 pos)
        {
            if (!pointerOnGUI())
            {
                mainController.moveCameraObject(((camMovePos - pos) * Time.deltaTime)); //  * forwardSpeed);
            }
		}
	
		//!
		//! pinch to zoom gesture (called by Mouse or Touch Input)
		//! @param      delta     delta distance between fingers since last frame
		//!
		public void pinchToZoom(float delta)
        {
			mainController.scaleSelectionUniform(delta);
		}
	
		//!
		//! triple touch started (called by Mouse or Touch Input)
		//! @param      pos     screen position of third pointer
		//!
		public void threePointerStarted(Vector3 pos)
        {
    		camMovePos = new Vector3(0, 0, pos.y);
		}
	
		//!
		//! triple touch ended (called by Mouse or Touch Input)
		//! @param      pos     screen position of third pointer
		//!
		public void threePointerEnded(Vector3 pos)
        {
			camMovePos = Vector3.zero;

            // HACK: to key in linktocamera mode
            //
            //if ( mainController.ActiveMode == MainController.Mode.objectLinkCamera && mainController.UIAdapter.LayoutUI == layouts.ANIMATION)
            //    mainController.AnimationController.setKeyFrame();


            // save the camera offset to restore it next time the app is started
            mainController.saveCameraOffset();
		}
	
		//!
		//! triple touch down & moving (called by Mouse or Touch Input)
		//! @param      pos     screen position of third pointer
		//!
		public void threePointerDrag(Vector3 pos)
        {
            if (Camera.main.orthographic == false)
            {
                mainController.moveCameraObject(((new Vector3(0, 0, pos.y) - camMovePos) * Time.deltaTime)); // * forwardSpeed);
            }
            else
            {
                Camera.main.orthographicSize = Camera.main.orthographicSize + (((camMovePos.z - pos.y) * Time.deltaTime) * VPETSettings.Instance.controllerSpeed);
                foreach (Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>())
                {
                    cam.orthographicSize = Camera.main.orthographicSize;
                }
            }
		}
	
		//!
		//! execute a raycast through the current position on a specific Unity layer (& ignore other layers)
		//! @param      pos             screen position of second pointer
		//! @param      layerMask       layer on which to execute the raycast
		//!                             3 -> defaul layer
		//!                             256 -> RenderInFront Layer (layer 8)
		//! @return     returns reference to the hit object (null if nothing was hit)             
		//!
		private GameObject cameraRaycast(Vector3 pos, int layerMask = 1) {
			Ray ray = Camera.main.GetComponent<Camera>().ScreenPointToRay(pos);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100000f, layerMask)) {
				//raycast was executed and hit an object
				return hit.collider.gameObject;
			}
			else {
				return null;
			}
		}
	
		//!
		//! execute a raycast onto a specific object
		//! @param      pos         screen position of second pointer
		//! @param      target      collider of the gameObject to raycast against
		//! @return     3D position on the object where the ray hit (nullVector if object was not hit)
		//!
		private Vector3 objectRaycast(Vector3 pos, Collider target)
        {
			Ray ray = Camera.main.ScreenPointToRay(pos);
			RaycastHit hit;
		
			if ( target.Raycast( ray, out hit, 100000f ) )
			{
				//raycast was executed and hit an object
				return hit.point;
			}
			else {
				return nullVector;
			}
		}

        private Vector3 planeRaycast(Vector3 pos, Plane target)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);

            float distance = 0.0f;
            if (target.Raycast(ray, out distance))
                return ray.GetPoint(distance);
            else
                return nullVector;
        }
	
		//!
		//! is any pointer currently over a GUI element
		//! @return     point is over GUI element yes/no
		//!
		private bool pointerOnGUI()
		{
            if ( Input.touchCount > 0 )
            {
                foreach (Touch touch in Input.touches)
                {
                    int pointerID = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(pointerID))
                    {
                        // at least on touch is over a canvas UI
                        return true;
                    }
                }
            }
            else
            {
                if (EventSystem.current.IsPointerOverGameObject()) return true;
            }

			return false;
		}
	}
	
	
	
	

}