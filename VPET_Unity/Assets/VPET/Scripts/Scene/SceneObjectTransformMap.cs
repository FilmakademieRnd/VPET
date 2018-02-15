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
using System.Collections;

//! 
//! This script should be applied to any gameObject that is able to move in the scene, is selectable by the user and should receive all physics (collisions & gravity).
//!
namespace vpet
{
	public class SceneObjectTransformMap : MonoBehaviour
    {

        private AnimatorObject animatorObject = null;

		//!
		//! maximum update interval for server communication in seconds
		//!
		static private float updateIntervall = 1.0f / 30.0f;
		//!
		//! is the translation update delayed since the last update was just recently
		//!
		bool translationUpdateDelayed = false;
		//!
		//! is the rotation update delayed since the last update was just recently
		//!
		bool rotationUpdateDelayed = false;
		//!
		//! last server update time of translation
		//!
		float lastTranslationUpdateTime = -1;
		//!
		//! number of frames after which the object is asumed as steady (no more physically driven translation changes)
		//!
		int translationStillFrameCount = 11;
		//!
		//! last server update time of rotation
		//!
		float lastRotationUpdateTime = -1;
		//!
		//! number of frames after which the object is asumed as steady (no more physically driven rotation changes)
		//!
		public int rotationStillFrameCount = 11;
		//!
		//! position of object at last server update
		//! used to track movement
		//!
		Vector3 lastPosition;
		//!
		//! rotation of object at last server update
		//! used to track rotation change
		//!
		Quaternion lastRotation;


		//!
		//! cached reference to serverAdapter
		//!
		ServerAdapter serverAdapter;

        //!
        //! cached reference to sceneLoader
        //!
        SceneLoader sceneLoader;

        //!
        //! lock auto detect movement & rotation
        //!
        public bool locked = false;

		//!
		//! lock for one frame (to avoid resending of incoming messages)
		//!
		public bool tempLock = false;

		//!
		//! allow changes to isKinematic property
		//!
		public bool lockKinematic = false;

		//smooth Translation variables

		//!
		//! slow down factor for smooth translation
		//!
		public float translationDamping = 1.0f;
		//!
		//! final target position of current translation
		//!
		private Vector3 targetTranslation = Vector3.zero;
		//!
		//! enable / disable smooth translation
		//!
		private bool smoothTranslationActive = false;
		//!
		//! Time since the last smooth translation of the camera has been started.
		//! Used to terminate the smooth translation after 3 seconds.
		//!
		private float smoothTranslateTime = 0;

		//store intial values for reset

		//!
		//! initial position at startup, used to reset object
		//!
		public Vector3 initialPosition;
		//!
		//! initial rotation at startup, used to reset object
		//!
		public Quaternion initialRotation;
		//!
		//! initial scale at startup, used to reset object
		//!
		public Vector3 initialScale;
		
		//!
		//! is the object currently selected
		//!
		public bool selected = false;
		//!
		//! should the slection visualization be drawn now
		//!
		private bool drawGlowAgain = true;


		//!
		//! extends of the object
		//!
		public Bounds bounds;

		//!
		//! target receiving all modifications (e.g. for light -> parent)
		//!
		Transform target;

        // target location for sending updates
        public Transform targetSend;


        // HACK
        // collider of the main scene object which needs to be moived according to the meshes
        public Transform targetCollider;


		//!
		//! Use this for initialization
		//!
		void Start () 
		{

            target = this.transform;

			//initalize cached references
			serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();



			//update initial parameters
			initialPosition = target.position;
			initialRotation = target.rotation;
			initialScale = target.localScale;

			lastPosition = initialPosition;
			lastRotation = initialRotation;



            animatorObject = transform.GetComponent < AnimatorObject>();
            if (animatorObject == null )
                animatorObject = transform.parent.GetComponent<AnimatorObject>();


        }


        //!
        //! Update is called once per frame
        //!
        void Update () 
		{
			if (lastPosition != target.position)
			{
				lastPosition = target.position;
				translationStillFrameCount = 0;
			}
			else if (translationStillFrameCount < 11)
			{
				translationStillFrameCount++;
			}
			if (lastRotation != target.rotation)
			{
				lastRotation = target.rotation;
				rotationStillFrameCount = 0;
			}
			else if (rotationStillFrameCount < 11)
			{
				rotationStillFrameCount++;
			}
			if (translationStillFrameCount >= 10 && rotationStillFrameCount >= 10 && tempLock)
			{
				tempLock = false;
			}
			if (!locked && !tempLock)
			{
				//publish translation change
				if (true) //mainController.liveMode)
				{
					if (translationStillFrameCount == 0) //position just changed
					{
                        if ((Time.time - lastTranslationUpdateTime) >= updateIntervall)
						{
                            targetSend.localPosition = target.localPosition; //targetSend.localRotation = target.localRotation; targetSend.localScale = target.localScale

							// serverAdapter.sendTranslation(target, target.position, !selected);
							//serverAdapter.sendTranslation(targetSend, !selected );

							//serverAdapter.sendTransform(targetSend, !selected );
                            							serverAdapter.SendObjectUpdate(target, !selected );

							// update collider 
                            serverAdapter.sendColliderOffset(targetCollider, target.localPosition);

							lastTranslationUpdateTime = Time.time;
							translationUpdateDelayed = false;
						}
						else
						{
							translationUpdateDelayed = true;
						}
					}
					else if (translationUpdateDelayed) //update delayed, but object not moving
					{
                        targetSend.localPosition = target.localPosition; //targetSend.localRotation = target.localRotation; targetSend.localScale = target.localScale

                        // serverAdapter.sendTranslation(target, target.position, !selected);
                        //serverAdapter.sendTranslation(targetSend, !selected );
							serverAdapter.SendObjectUpdate(target, !selected );

						//serverAdapter.sendTransform(targetSend, !selected );
                        // update collider 
                        serverAdapter.sendColliderOffset(targetCollider, target.localPosition);

                        lastTranslationUpdateTime = Time.time;
						translationUpdateDelayed = false;
					}
				}
				else if (translationStillFrameCount == 10) //object is now no longer moving
				{
                    targetSend.localPosition = target.localPosition; //targetSend.localRotation = target.localRotation; targetSend.localScale = target.localScale

                    // serverAdapter.sendTranslation(target, target.position, !selected);
                    //serverAdapter.sendTranslation(targetSend, !selected );
							serverAdapter.SendObjectUpdate(target, !selected );

					//serverAdapter.sendTransform(targetSend, !selected );
                    // update collider 
                    serverAdapter.sendColliderOffset(targetCollider, target.localPosition);

                }

                //publish rotation change
                if (true) // mainController.liveMode)
				{
					if (rotationStillFrameCount == 0) //position just changed
					{
						if ((Time.time - lastRotationUpdateTime) >= updateIntervall)
						{
							// serverAdapter.sendRotation(target, target.rotation, !selected);
							//serverAdapter.sendRotation(targetSend, !selected );
							serverAdapter.SendObjectUpdate(target, !selected );

							//serverAdapter.sendTransform(targetSend, !selected );

							lastRotationUpdateTime = Time.time;
							rotationUpdateDelayed = false;
						}
						else
						{
							rotationUpdateDelayed = true;
						}
					}
					else if (rotationUpdateDelayed) //update delayed, but object not moving
					{
						//serverAdapter.sendRotation(target, target.rotation, !selected);
						//serverAdapter.sendRotation(targetSend, !selected );
							serverAdapter.SendObjectUpdate(target, !selected );


						//serverAdapter.sendTransform(targetSend, !selected );

						lastRotationUpdateTime = Time.time;
						rotationUpdateDelayed = false;
					}
				}
				else if (rotationStillFrameCount == 10) //object is now no longer moving
				{
					// serverAdapter.sendRotation(target, target.rotation, !selected);
					//serverAdapter.sendRotation(targetSend, !selected );
							serverAdapter.SendObjectUpdate(target, !selected );

					//serverAdapter.sendTransform(targetSend, !selected );

				}

			}

		}

    }
}