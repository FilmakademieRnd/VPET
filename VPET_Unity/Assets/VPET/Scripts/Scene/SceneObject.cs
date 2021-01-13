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
using System.Collections;
#if SCENE_HOST
using UnityEditor;
using System;
#endif

//! 
//! This script should be applied to any gameObject that is able to move in the scene, is selectable by the user and should receive all physics (collisions & gravity).
//!
namespace vpet
{
	public class SceneObject : MonoBehaviour {

        //!
        //! ID of a scene object
        //!
        public int id = -1;

//#if !SCENE_HOST
        //!
        //! cached reference to animation controller
        //!
        private AnimationController animationController;
//#endif

        //!
        //! cached reference to animation data (runtime representation)
        //!
        private AnimationData animData = null;

		//!
		//! current animation layer
		//!
		public int animationLayer = -1;

		//!
		//! duration of the animation in seconds
		//!
		public float animationDuration = -1;

		//!
		//! curves for the current animation (currently playing or to be played)
		//!
		public AnimationCurve[] transCurves = new AnimationCurve[3];

		//!
		//! curves for the current animation (currently playing or to be played)
		//!
		public AnimationCurve[] rotationCurves = new AnimationCurve[4];

		//!
		//! curves for the current animation (currently playing or to be played)
		//!
		public AnimationCurve[] scaleCurves = new AnimationCurve[3];

		//!
		//! should the animation be looping
		//!
		public bool animationLooping = false;

		//!
		//! maximum update interval for server communication in seconds
		//!
		static private float updateIntervall = 1.0f / 30.0f;
		
		//!
		//! number of frames after which the object is asumed as steady (no more physically driven translation changes)
		//!
		protected int translationStillFrameCount = 11;
        //!
        //! number of frames after which the object is asumed as steady (no more physically driven rotation changes)
        //!
        protected int rotationStillFrameCount = 11;
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
        //! should a boxcollider be generated auomatically
        //!
#if !SCENE_HOST
        public bool generateBoxCollider = true;
#else
        public bool generateBoxCollider = false;
#endif

#if !SCENE_HOST
        //!
        //! cached reference to mainController
        //!
        protected MainController mainController;
#endif
        //!
        //! cached reference to serverAdapter
        //!
        protected ServerAdapter serverAdapter;
        //!
        //! cached reference to sceneLoader
        //!
        SceneLoader sceneLoader;

        //!
        //! lock auto detect movement & rotation
        //!
        public bool locked = false;

#if SCENE_HOST
        //!
        //! time until unlock is send
        //!
        [HideInInspector]
        public float unlockTime = -1;
#endif

        //!
        //! allow changes to isKinematic property
        //!
        public bool globalKinematic = true;

        //!
        //! is animated character
        //!
        public bool isAnimatedCharacter = false;

        //!
        //! slow down factor for smooth translation
        //!
        public float translationDamping = 1.0f;
		//!
		//! final target position of current translation
		//!
		public Vector3 targetTranslation = Vector3.zero;
        
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
		protected bool drawGlowAgain = true;
		//!
		//! extends of the object
		//!
		public Bounds bounds;

        //!
        //! target receiving all modifications (e.g. for light -> parent)
        //!
        protected Transform target;

        protected BoxCollider boxCollider;

        public OutlineEffect outlineEffect;

        public Outline outline;

        public bool isPlayingAnimation;

        public bool isPhysicsActive;

        private Rigidbody rigidbody;

        //!
        //! Use this for initialization
        //!
        protected void Start()
        {

            target = this.transform;

            isPhysicsActive = false;

            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

#if !SCENE_HOST
            animationController = GameObject.Find("AnimationController").GetComponent<AnimationController>();
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
            sceneLoader = GameObject.Find("SceneAdapter").GetComponent<SceneLoader>();

            //cache Reference to animation data
            animData = AnimationData.Data;
#endif

            //update initial parameters
            initialPosition = target.localPosition;
            initialRotation = target.localRotation;
            initialScale = target.localScale;

            lastPosition = initialPosition;
            lastRotation = initialRotation;

            InvokeRepeating("checkUpdate", 0.0f, updateIntervall);

            if (gameObject.GetComponent<Animator>())
                isAnimatedCharacter = true;

            //generate colliding volumes
            if (generateBoxCollider)
            {
                //calculate bounds
                bounds = new Bounds(Vector3.zero, Vector3.zero);

                bool isSkinned = false;
                bool hasBounds = false;
                Renderer[] renderers = this.gameObject.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (!sceneLoader.isEditable(renderer.gameObject) || renderer.gameObject == this.gameObject)
                    {
                        if (hasBounds)
                        {
                            if (typeof(SkinnedMeshRenderer) == renderer.GetType())
                            {
                                bounds.Encapsulate(((SkinnedMeshRenderer)renderer).localBounds);
                                isSkinned = true;
                            }
                            else
                                bounds.Encapsulate(renderer.bounds);
                        }
                        else
                        {
                            if (typeof(SkinnedMeshRenderer) == renderer.GetType())
                            {
                                bounds = ((SkinnedMeshRenderer)renderer).localBounds;
                                isSkinned = true;
                            }
                            else
                                bounds = renderer.bounds;
                            hasBounds = true;
                        }
                    }
                }


                boxCollider = this.gameObject.AddComponent<BoxCollider>();
#if !SCENE_HOST
                if(this is SceneObjectCamera)
                    boxCollider.enabled = mainController.showCam;
#endif



                // col.isTrigger = true; // not interacting
                if ((this is SceneObject) && !(this is SceneObjectLight))
                {
                    if (isSkinned)
                    {
                        boxCollider.center = bounds.center;
                        boxCollider.size = bounds.size;
                    }
                    else
                    {
                        boxCollider.center = new Vector3((bounds.center.x - this.transform.position.x) / this.transform.lossyScale.x, (bounds.center.y - this.transform.position.y) / this.transform.lossyScale.y, (bounds.center.z - this.transform.position.z) / this.transform.lossyScale.z);
                        boxCollider.size = new Vector3(bounds.size.x / this.transform.lossyScale.x, bounds.size.y / this.transform.lossyScale.y, bounds.size.z / this.transform.lossyScale.z);
                    }

                    boxCollider.material = new PhysicMaterial();
                    boxCollider.material.bounciness = 1.0f;
                }
            }
            if (this is SceneObject)
            {
                rigidbody = this.gameObject.AddComponent<Rigidbody>();

                rigidbody.mass = 100f;
                rigidbody.drag = 2.5f;

                // TODO: temporary
                rigidbody.useGravity = true;
                rigidbody.isKinematic = true;
                globalKinematic = true;
            }
            else
                rigidbody = null;

#if !SCENE_HOST
            //Initalize animation loading if animation available
            AnimationSerializer asScript = target.gameObject.AddComponent<AnimationSerializer>();
            if (asScript.loadData())
            {
                //register the object in the animation Controller
                GameObject.Find("AnimationController").GetComponent<AnimationController>().registerAnimatedObject(gameObject.GetComponent<SceneObject>());
                gameObject.GetComponent<SceneObject>().setKinematic(true, false);
                updateAnimationCurves();
            }

            isPlayingAnimation = false;
#endif

        }

        private bool AlmostEqualPos(Vector3 a, Vector3 b, float precision)
        {
            if (Mathf.Abs(a.x - b.x) > precision) 
                return false;
            if (Mathf.Abs(a.y - b.y) > precision)
                return false;
            if (Mathf.Abs(a.z - b.z) > precision)
                return false;

            return true;
        }

        private bool AlmostEqualRot(Quaternion a, Quaternion b, float precision)
        {
            return (Quaternion.Angle(a, b) < precision);
        }

        private void checkUpdate()
        {
#if !SCENE_HOST
            if (mainController.ActiveMode != MainController.Mode.objectLinkCamera)
            {
                if (!AlmostEqualPos(lastPosition, target.localPosition, 0.0001f))
                {
                    lastPosition = target.localPosition;
                    translationStillFrameCount = 0;
                }
                else if (translationStillFrameCount < 11)
                {
                    translationStillFrameCount++;
                }
                if (!AlmostEqualRot(lastRotation, target.localRotation, 0.0001f))
                {
                    lastRotation = target.localRotation;
                    rotationStillFrameCount = 0;
                }
                else if (rotationStillFrameCount < 11)
                {
                    rotationStillFrameCount++;
                }
            }
            else
#endif
            {
                if (!AlmostEqualPos(lastPosition, target.position, 0.0001f))
                {
                    lastPosition = target.position;
                    translationStillFrameCount = 0;
                }
                else if (translationStillFrameCount < 11)
                {
                    translationStillFrameCount++;
                }
                if (!AlmostEqualRot(lastRotation, target.rotation, 0.0001f))
                {
                    lastRotation = target.rotation;
                    rotationStillFrameCount = 0;
                }
                else if (rotationStillFrameCount < 11)
                {
                    rotationStillFrameCount++;
                }
            }
#if !SCENE_HOST
            if (!locked && !mainController.lockScene)
            {
#else
            if (Array.Exists(UnityEditor.Selection.gameObjects, selection => selection == this.gameObject) && !selected
                && (translationStillFrameCount == 0 || rotationStillFrameCount == 0))
            {
                this.unlockTime = 0.5f;
                locked = false;
                selected = true;
                serverAdapter.SendObjectUpdate(this, ParameterType.LOCK);
            }

            if (!locked)
            {
#endif
                if (translationStillFrameCount == 0) //position just changed
                {

                    if (!selected && !isPlayingAnimation && !isPhysicsActive && !rigidbody.isKinematic)
                    {
                        isPhysicsActive = true;
                        serverAdapter.SendObjectUpdate(this, ParameterType.HIDDENLOCK);
                        serverAdapter.SendObjectUpdate(this, ParameterType.KINEMATIC);
                    }

#if !SCENE_HOST               
                    if (isAnimatedCharacter)
                    {
                        if (mainController.ActiveMode == MainController.Mode.translationMode &&
                            mainController.isTranslating)
                            serverAdapter.SendObjectUpdate(this, ParameterType.POS);
                    }
                    else
#endif
                        serverAdapter.SendObjectUpdate(this, ParameterType.POS);
                }
                else  //update delayed, but object not moving
                {
                    if (isPhysicsActive)
                    {
                        isPhysicsActive = false;
                        serverAdapter.SendObjectUpdate(this, ParameterType.HIDDENLOCK);
                        serverAdapter.SendObjectUpdate(this, ParameterType.KINEMATIC);
                    }
                }

                
                if (rotationStillFrameCount == 0) //position just changed
                {

                    if (!selected && !isPlayingAnimation && !isPhysicsActive && !rigidbody.isKinematic)
                    {
                        isPhysicsActive = true;
                        serverAdapter.SendObjectUpdate(this, ParameterType.HIDDENLOCK);
                        serverAdapter.SendObjectUpdate(this, ParameterType.KINEMATIC);
                    }

#if !SCENE_HOST
                    if (isAnimatedCharacter)
                    {
                        if (mainController.ActiveMode == MainController.Mode.rotationMode &&
                            mainController.isRotating)
                            serverAdapter.SendObjectUpdate(this, ParameterType.ROT);
                    }
                    else
#endif
                        serverAdapter.SendObjectUpdate(this, ParameterType.ROT);

                }
                else  //update delayed, but object not moving
                {
                    if (isPhysicsActive)
                    {
                        isPhysicsActive = false;
                        serverAdapter.SendObjectUpdate(this, ParameterType.HIDDENLOCK);
                        serverAdapter.SendObjectUpdate(this, ParameterType.KINEMATIC);
                    }
                }
            }
        }

        //!
        //! Update is called once per frame
        //!
        protected void Update () 
		{
#if SCENE_HOST
            //Automatically unlock object on all clients after
            if (unlockTime > 0)
            {
                unlockTime -= Time.deltaTime;
                if (unlockTime < 0)
                {
                    selected = false;
                    serverAdapter.SendObjectUpdate(this, ParameterType.LOCK);
                }               
            }
#else

            //turn on highlight modes
            if (selected && drawGlowAgain)
            {
                if ((this.GetType() == typeof(SceneObject)) ||
                    (this.GetType() == typeof(SceneObjectCamera)))
                {
                    this.showHighlighted(this.gameObject);
                }
                if (this.GetType() != typeof(SceneObjectLight))
                    drawGlowAgain = false;
            }

            //turn off highlight mode
            else if (!selected && !drawGlowAgain && this.GetType() != typeof(SceneObjectLight))
            {
                this.showNormal(this.gameObject);
                drawGlowAgain = true;
            }

            //execute smooth translate
            if (smoothTranslationActive)
			{
				target.position = Vector3.Lerp(target.position, targetTranslation, Time.deltaTime * translationDamping);
				if (Vector3.Distance(target.position, targetTranslation) < 0.0001f)
				{
                    target.position = targetTranslation;
					smoothTranslationActive = false;
				}
				if ((Time.time - smoothTranslateTime) > 3.0f)
				{
					smoothTranslationActive = false;
				}
			}
#endif
        }

        //!
        //! move this object to the floor (the lowest point of the object will be set to have y position = 0)
        //!
        public void moveToFloor()
		{
			target.position = new Vector3(target.position.x, this.GetComponent<BoxCollider>().bounds.size.y / 2, target.position.z);
		}


		//!
		//! reset rotation to values present on startup
		//!
		public void resetRotation()
		{
            target.localRotation = initialRotation;
            serverAdapter.SendObjectUpdate(this, ParameterType.ROT);
        }

        //!
        //! reset position to values present on startup
        //!
        public void resetPosition()
		{
            target.localPosition = initialPosition;
            serverAdapter.SendObjectUpdate(this, ParameterType.POS);
        }

        //!
        //! reset scale to values present on startup
        //!
        public void resetScale()
		{
			target.localScale = initialScale;
            serverAdapter.SendObjectUpdate(this, ParameterType.SCALE);
        }

        //!
        //! reset all parameters to inital values present on startup
        //!
        public void resetAll()
        {
			locked = false;
			serverAdapter.SendObjectUpdate(this, ParameterType.LOCK);

            target.localRotation = initialRotation;
            serverAdapter.SendObjectUpdate(this, ParameterType.ROT);

            target.localPosition = initialPosition;
            serverAdapter.SendObjectUpdate(this, ParameterType.POS);

            target.localScale = initialScale;
            serverAdapter.SendObjectUpdate(this, ParameterType.SCALE);

            this.setKinematic(true);
#if !SCENE_HOST
            mainController.deselect();
#endif
        }


		//void OnDrawGizmos()
		//{
		//    Collider collider = this.GetComponent<BoxCollider>();
		//    if (collider)
		//    {
		//        Debug.Log(collider.bounds.ToString());
		//        Bounds colliderBounds = collider.bounds;
		//        Gizmos.color = new Color(1, 1, 0, 0.5f);
		//        Gizmos.DrawCube(colliderBounds.center, colliderBounds.size);
		//    }
		//}

		//!
		//! recursively apply highlight shader to object
		//! @param  obj    gameObject on which to apply the highlight shader
		//!
		public void showHighlighted(GameObject obj)
		{
            //do it for parent object
            if (obj.GetComponent<Renderer>() != null) //is object rendered?
            {
                if (Camera.main.transform.GetChild(0).GetComponent<OutlineEffect>())
                {
                    Outline outline = obj.GetComponent<Outline>();
                    if (outline == null)
                        outline = obj.AddComponent<Outline>();
                    else
                        outline.enabled = true;

                    // outline.setLineColor(new Color(1.0f, 0.8f, 0.3f));  // yellow. no need to set because default is yellow
                }
            }
            foreach (Transform child in obj.transform)
            {
                this.showHighlighted(child.gameObject);
            }
        }


        //!
        //! recursively delete highlight shader of object
        //! @param  obj    gameObject on which to delete the highlight shader
        //!
        public void showNormal(GameObject obj)
        {
            //do it for parent object
            if (obj.GetComponent<Renderer>() != null) //is object rendered?
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;

            }
            foreach (Transform child in obj.transform)
            {
                this.showNormal(child.gameObject);
            }
        }


        //!
        //!
        //!
        public void updateLockView()
        {
            if(this.locked)
                this.showLocked(this.gameObject);
            else
                this.showUnlocked(this.gameObject);
        }

        //!
        //! recursively apply highlight shader to locked
        //! @param  obj    gameObject on which to apply the highlight shader
        //!
        private void showLocked(GameObject obj)
        {
            //do it for parent object
            if (obj.GetComponent<Renderer>() != null) //is object rendered?
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline == null)
                    outline = obj.AddComponent<Outline>();
                else
                    outline.enabled = true;

                outline.setLineColor(new Color(0.7f, 0f, 0.03f));  // override default yellow with deep red color

            }
            foreach (Transform child in obj.transform)
            {
                this.showLocked(child.gameObject);
            }
        }

        //!
        //! recursively delete highlight shader of object
        //! @param  obj    gameObject on which to delete the highlight shader
        //!
        private void showUnlocked(GameObject obj)
        {
            //do it for parent object
            if (obj.GetComponent<Renderer>() != null) //is object rendered?
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;
            }
            foreach (Transform child in obj.transform)
            {
                this.showUnlocked(child.gameObject);
            }
        }

		//!
		//! initalize a smooth translation of the object to a given point
		//! @param    pos    world position to send the object to
		//!
		public void smoothTranslate(Vector3 pos)
		{
			smoothTranslationActive = true;
			smoothTranslateTime = Time.time;
			targetTranslation = pos;
		}


		// TODO: Why local ??

		//!
		//! translate the object immediately
		//! @param    pos    world position to send the object to
		//!
		public void translate(Vector3 pos)
		{
            if (isAnimatedCharacter) {
                CharacterAnimationController charController = GetComponent<CharacterAnimationController>();
                charController.bodyPosition = charController.bodyPosition + (pos - target.transform.position);
            }
            target.transform.position = pos;
		}

        public float TranslateX
        {
            get{ return target.transform.localPosition.x; }
            set{ target.transform.localPosition = new Vector3(value, target.transform.localPosition.y, target.transform.localPosition.z); }
        }

        public float TranslateY
        {
            get { return target.transform.localPosition.y; }
            set { target.transform.localPosition = new Vector3(target.transform.localPosition.x, value, target.transform.localPosition.z); }
        }

        public float TranslateZ
        {
            get { return target.transform.localPosition.z; }
            set { target.transform.localPosition = new Vector3(target.transform.localPosition.x, target.transform.localPosition.y, value); }
        }

        //!
        //! scale the object immediately
        //! @param    scl    local scale to send the object to
        //!
        public void scale( Vector3 scl )
		{
			target.transform.localScale = scl;
			serverAdapter.SendObjectUpdate(this, ParameterType.SCALE);
		}

        public float ScaleX
        {
            get { return target.transform.localScale.x; }
            set { target.transform.localScale = new Vector3(value, target.transform.localScale.y, target.transform.localScale.z); }
        }

        public float ScaleY
        {
            get { return target.transform.localScale.y; }
            set { target.transform.localScale = new Vector3(target.transform.localScale.x, value, target.transform.localScale.z); }
        }

        public float ScaleZ
        {
            get { return target.transform.localScale.z; }
            set { target.transform.localScale = new Vector3(target.transform.localScale.x, target.transform.localScale.y, value); }
        }


        public float RotateQuatX
        {
            get { return target.transform.localRotation.x; } 
        }

        public float RotateQuatY
        {
            get { return target.transform.localRotation.y; }
        }

        public float RotateQuatZ
        {
            get { return target.transform.localRotation.z; }
        }

        public float RotateQuatW
        {
            get { return target.transform.localRotation.w; }
        }

        public float RotateX
        {
            get { return target.transform.localRotation.eulerAngles.x; }
            set { target.transform.localRotation = Quaternion.Euler(value, target.transform.localRotation.eulerAngles.y, target.transform.localRotation.eulerAngles.z); }
        }

        public float RotateY
        {
            get { return target.transform.localRotation.eulerAngles.y; }
            set { target.transform.localRotation = Quaternion.Euler(target.transform.localRotation.eulerAngles.x, value, target.transform.localRotation.eulerAngles.z); }
        }

        public float RotateZ
        {
            get { return target.transform.localRotation.eulerAngles.z; }
            set { target.transform.localRotation = Quaternion.Euler(target.transform.localRotation.eulerAngles.x, target.transform.localRotation.eulerAngles.y, value); }
        }


        //!
        //! set the kinematic parameter of the object
        //! @param      set     new value for kinematic
        //! @param      send    should this modification be published to the server     
        //!
        public void setKinematic(bool set, bool send = true)
        {
            if (!(this.GetType() == typeof(SceneObjectLight) || this.GetType() == typeof(SceneObjectCamera))) 
            {
                globalKinematic = set;
                rigidbody.isKinematic = set;
                if (send)
                    serverAdapter.SendObjectUpdate(this, ParameterType.KINEMATIC);
                if (!set)
                    rigidbody.WakeUp();
            }
        }

        public void enableRigidbody(bool set)
        {
            if (set)
                rigidbody.isKinematic = globalKinematic;
            else
                rigidbody.isKinematic = true;

        }

        //!
        //! send updates of the last modifications to the network server
        //!
        protected void sendUpdate()
		{
#if !SCENE_HOST
            if (mainController.ActiveMode == MainController.Mode.scaleMode)
			{
                serverAdapter.SendObjectUpdate(this, ParameterType.SCALE);
            }
#endif
        }

        //!
        //! set the sceneObjects to a given position (time) in the animation
        //! @param      time        time within the animation
        //!
        public void setAnimationState(float time)
	    {
            // print("setAnimationState at time: " + time);

            float _time = animationLooping ? time % animationDuration : time;

            if ( transCurves[0] != null )
            {
                this.transform.localPosition = new Vector3(transCurves[0].Evaluate(_time),
                                                      transCurves[1].Evaluate(_time),
                                                      transCurves[2].Evaluate(_time));
            }

            if (rotationCurves[0] != null)
            {
                this.transform.localRotation = new Quaternion(rotationCurves[0].Evaluate(_time),
                                                          rotationCurves[1].Evaluate(_time),
                                                          rotationCurves[2].Evaluate(_time),
                                                          rotationCurves[3].Evaluate(_time));
            }

            if (scaleCurves[0] != null)
            {
                this.transform.localScale = new Vector3(scaleCurves[0].Evaluate(_time),
                                                      scaleCurves[1].Evaluate(_time),
                                                      scaleCurves[2].Evaluate(_time));
            }

        }

        //!
        //! update the animation curves
        //! normally called after the animation curve has changed
        //!
        public void updateAnimationCurves()
	    {
	        if (animData.getAnimationClips(target.gameObject) != null)
	        {
	            transCurves[0] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalPosition.x");
	            transCurves[1] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalPosition.y");
	            transCurves[2] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalPosition.z");
	
	            rotationCurves[0] = animData.getAnimationCurve( animData.getAnimationClips( target.gameObject )[0], "m_LocalRotation.x" );
	            rotationCurves[1] = animData.getAnimationCurve( animData.getAnimationClips( target.gameObject )[0], "m_LocalRotation.y" );
	            rotationCurves[2] = animData.getAnimationCurve( animData.getAnimationClips( target.gameObject )[0], "m_LocalRotation.z" );
	            rotationCurves[3] = animData.getAnimationCurve( animData.getAnimationClips( target.gameObject )[0], "m_LocalRotation.w" );

                scaleCurves[0] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalScale.x");
                scaleCurves[1] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalScale.y");
                scaleCurves[2] = animData.getAnimationCurve(animData.getAnimationClips(target.gameObject)[0], "m_LocalScale.z");

                // TODO: fix one length for all curves
                animationDuration = animData.getAnimationClips(target.gameObject)[0].length;
	            setAnimationState(animationController.currentAnimationTime);
	        }
	    }


        //!
        //! hide or show the visualization (cone, sphere, arrow) of the light
        //! @param      set     hide-> true, show->false   
        //!
        public virtual void hideVisualization(bool set) { }

        //!
        //! sets the keyframe at the current time of the animation to a new value for the currently animated object
        //! automatically adds keyframe if there is none at this time of the animation
        //!
        public void setKeyframe()
	    {
	        bool movedSuccessfully = false;
	        int keyIndex = -1;
	
	        //x value
	        Keyframe key = new Keyframe(animationController.currentAnimationTime, this.transform.position.x);
	        for (int i = 0; i < this.transCurves[0].length; i++)
	        {
	            if (Mathf.Abs(this.transCurves[0].keys[i].time - animationController.currentAnimationTime) < 0.04)
	            {
	                this.transCurves[0].MoveKey(i, key);
	                keyIndex = i;
	                movedSuccessfully = true;
	            }
	        }
	        if (!movedSuccessfully)
	        {
	            keyIndex = this.transCurves[0].AddKey(key);
	            movedSuccessfully = false;
	        }
	        if (transCurves[0].keys.Length > 1)
	        {
	            if (keyIndex == 0) this.transCurves[0].SmoothTangents(1, 0);
	            if (keyIndex == transCurves[0].keys.Length - 1) this.transCurves[0].SmoothTangents(transCurves[0].keys.Length - 2, 0);
	        }
	        this.transCurves[0].SmoothTangents(keyIndex, 0);
	
	        //y value
	        key = new Keyframe(animationController.currentAnimationTime, this.transform.position.y);
	        for (int i = 0; i < this.transCurves[1].length; i++)
	        {
	            if (Mathf.Abs(this.transCurves[1].keys[i].time - animationController.currentAnimationTime) < 0.04)
	            {
	                this.transCurves[1].MoveKey(i, key);
	                keyIndex = i;
	                movedSuccessfully = true;
	            }
	        }
	        if (!movedSuccessfully)
	        {
	            keyIndex = this.transCurves[1].AddKey(key);
	            movedSuccessfully = false;
	        }
	        if (transCurves[1].keys.Length > 1)
	        {
	            if (keyIndex == 0) this.transCurves[1].SmoothTangents(1, 0);
	            if (keyIndex == transCurves[1].keys.Length - 1) this.transCurves[1].SmoothTangents(transCurves[1].keys.Length - 2, 0);
	        }
	        this.transCurves[1].SmoothTangents(keyIndex, 0);
	
	        //z value
	        key = new Keyframe(animationController.currentAnimationTime, this.transform.position.z);
	        for (int i = 0; i < this.transCurves[2].length; i++)
	        {
	            if (Mathf.Abs(this.transCurves[2].keys[i].time - animationController.currentAnimationTime) < 0.04)
	            {
	                this.transCurves[2].MoveKey(i, key);
	                keyIndex = i;
	                movedSuccessfully = true;
	            }
	        }
	        if (!movedSuccessfully)
	        {
	            keyIndex = this.transCurves[2].AddKey(key);
	            movedSuccessfully = false;
	        }
	
	        if (transCurves[2].keys.Length > 1)
	        {
	            if (keyIndex == 0) this.transCurves[2].SmoothTangents(1, 0);
	            if (keyIndex == transCurves[2].keys.Length - 1) this.transCurves[2].SmoothTangents(transCurves[2].keys.Length - 2, 0);
	        }
	        this.transCurves[2].SmoothTangents(keyIndex, 0);
	
	        animData.changeAnimationCurve(animData.getAnimationClips(target.gameObject)[0], typeof(Transform), "m_LocalPosition.x", transCurves[0]);
	        animData.changeAnimationCurve(animData.getAnimationClips(target.gameObject)[0], typeof(Transform), "m_LocalPosition.y", transCurves[1]);
	        animData.changeAnimationCurve(animData.getAnimationClips(target.gameObject)[0], typeof(Transform), "m_LocalPosition.z", transCurves[2]);
	
	        updateAnimationCurves();
	    }

    }
}