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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using System;

//!
//! class managing animation editing tasks
//!
namespace vpet
{
    public class AnimationLayer
    {
        public bool isPlaying = false;
        public float offset = 0.0f;
        public float currentAnimationTime = 0.0f;
        public List<SceneObject> layerObjects = new List<SceneObject>();
    }

	public class AnimationController : MonoBehaviour
    {
        
        bool updateAppearance;

        // private delegate float FieldFloatDelegate();
        
	
	    //!
	    //! is animation editing currently active
	    //!
        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        //!
        //! is animation currently playing
        //!
        private bool playing = false;
	
	    //!
	    //! system time when the animation was at frame 0
	    //!
	    private float systemTimeOffset = 0;
	
	    //!
	    //! current time of the animation
	    //!
	    public float currentAnimationTime = 0;
	    public float CurrentAnimationTime
	    {
	        set { currentAnimationTime = value;
	            timeChanged();
	        }
	    }
	
	    //!
	    //! 
	    //!
	    private float timeLineStartInit = 0;
	
	    //!
	    //! 
	    //!
	    private float timeLineEndInit = 5;
	
	    //!
	    //! is the position of the sceneObject currently edited
	    //!
	    public bool editingPosition = false;
	
	    //!
	    //! maximum angle between tangents for which the creation of an interpolated point is rejected
	    //!
	    public float hermitInterpolationRate = 10f;
	
	    //!
	    //! create default clip
	    //!
	    private bool doCreateClip = true;
	
	    //!
	    //! value scaling the key handles
	    //!
	    public float keyHandleScale = 1.0f;
	
	    //!
	    //! Cached reference to the main controller.
	    //!
	    private MainController mainController;
	
	    //!
	    //! Currently selected object.
	    //!
	    private GameObject animationTarget;
	
	    //!
	    //! initial pointer x position when dragging is started
	    //!
	    private float initialDragPositionX = 0;
	
	    //!
	    //! time of animation at drag start
	    //!
	    private float animationTimeDragStart = 0;
	
	    //!
	    //! is dragging of the timeline currently active
	    //!
	    // private bool dragging = false;
	
	    //!
	    //! timeline shown to navigate within an animation
	    //!
	    private GameObject timeLineObject;
	
	
	    //!
	    //! timeline control script
	    //!
	    private TimeLineWidget timeLine = null;
	
	
	    //!
	    //! container object holding all keyframe spheres in the scene
	    //!
	    private GameObject frameSphereContainer;
	
	    //!
	    //! overlay for the timeline to track pointer sliding
	    //!
	    private GameObject dragArea;
	
	    //!
	    //! basic line renderer (can only draw streight lines)
	    //!
	    private LineRenderer lineRenderer = null;
	
	    //!
	    //! color of the line drawn by the lineRenderer
	    //!
	    public Color lineColor;
	
	    //!
	    //! cached reference to animation data (runtime representation)
	    //!
	    private AnimationData animData = null;

        private ServerAdapter serverAdapter;
	
	    //!
	    //! list of keyframe representing spheres
	    //!
	    private List<GameObject> keyframeSpheres;
	
	    //!
	    //! list storing all animated objects of the scene
	    //!
	    private List<SceneObject> animatedObjects;

        //!
        //! layer list
        //!
        //private List<int> animationLayers;
        AnimationLayer[] animationLayers = new AnimationLayer[3];
	
	
	    private GameObject keySpherePrefab;

        private string[] animationProperties = new string[] { };

        private PropertyInfo[] animationFields;

        private System.Object animationInstance;

        //!
        //! Use this for initialization
        //!
        void Awake()
	    {
            updateAppearance = true;

	        //cache reference to main Controller
	        mainController = GameObject.Find("MainController").GetComponent<MainController>();

	        //cache reference to animation timeline
			timeLineObject = GameObject.Find("GUI/Canvas/UI/TimeLine");
			if (timeLineObject == null) Debug.LogError(string.Format("{0}: Cant Find TimeLine (GUI/Canvas/UI/TimeLine).", this.GetType()));
	
			timeLine = timeLineObject.GetComponent<TimeLineWidget>();
	        if (timeLine == null) Debug.LogError(string.Format("{0}: No TimeLine script attached.", this.GetType()));
	        // assign callback for frame changes on timeline (on user drag)
	        timeLine.Callback = this.setTime;

            //cache reference to keyframe Sphere container
            if (!frameSphereContainer)
            {
                frameSphereContainer = new GameObject("FrameSphereContainer");
                frameSphereContainer.transform.parent = GameObject.Find("Scene").transform;
                frameSphereContainer.transform.localPosition = Vector3.zero;
                frameSphereContainer.transform.localRotation = Quaternion.identity;
                frameSphereContainer.transform.localScale = Vector3.one;
            }
	
	        // cache key prefab
	        keySpherePrefab = Resources.Load<GameObject>("VPET/Prefabs/KeySphere");
	        if (keySpherePrefab == null) Debug.LogError(string.Format("{0}: Cant find Resources: KeySphere.", this.GetType()));
	
	        //cache reference to dragArea and deactivate it
	        //dragArea = timeline.transform.GetChild(7).gameObject;
	        //dragArea.SetActive(false);
	
	        //cache Reference to animation data
	        animData = AnimationData.Data;
	
	        //initalize keyframe list
	        keyframeSpheres = new List<GameObject>();
	
	        //initalize animated scene objects list
	        if (animatedObjects == null)
	        {
	            animatedObjects = new List<SceneObject>();
	        }

            //initialize animation layers
            for (int i = 0; i < animationLayers.Length; ++i)
            {
                animationLayers[i] = new AnimationLayer();
            }
	
	        //initalize lineRenderer
	        lineRenderer = gameObject.AddComponent<LineRenderer>();
	        lineRenderer.material = Resources.Load<Material>("VPET/Materials/LineRendererMaterial");
	        lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 0;
	    }
	
	
	    void Start()
	    {
            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

            timeLine.StartTime = timeLineStartInit;
	        timeLine.EndTime = currentAnimationTime = timeLineEndInit;
	    }
	
	    //!
	    //! Update is called once per frame
	    //!
	    void Update()
	    {
	
	        if (playing ) // && !dragging)
	        {
	            setTime( currentAnimationTime + Time.deltaTime );
	        }
	
	        /*
	        if (isActive)
	        {
	            // timeline.GetComponent<TimelineScript>().setTime(currentAnimationTime, animationTarget.GetComponent<SceneObject>().animationDuration);
	            timeLine.setTime(currentAnimationTime);
	
	            if (editingPosition)
	            {
	                if (!mainController.getCurrentSelection().GetComponent<KeyframeScript>())
	                {
	                    print("set/add keframe value");
	                    animationTarget.GetComponent<SceneObject>().setKeyframe();
	                }
	                else
	                {
	                    print("updateAnimationCurves");
	                    animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
	                }
	            }
	
	            if (dragging)
	            {
	                currentAnimationTime = animationTimeDragStart + ((initialDragPositionX - Input.mousePosition.x) / (25 * (Screen.dpi / 15.0f)));
	                foreach (SceneObject animatedObject in animatedObjects)
	                {
	                    animatedObject.setAnimationState(currentAnimationTime);
	                }
	                if (Input.GetMouseButtonUp(0))
	                {
	                    stopDrag();
	                }
	            }
	
	            updateLine();
	        }
	        */
	    }
	
	    private void setTime(float x)
	    {
	        currentAnimationTime = Mathf.Clamp(x, timeLine.StartTime, timeLine.EndTime );
            if (currentAnimationTime == timeLine.EndTime) currentAnimationTime = timeLine.StartTime;
	        timeChanged();
	    }

        public void setKeyFrame()
        {

            currentAnimationTime = timeLine.CurrentTime;

            if (animationTarget != null)
            {
                AnimationClip clip = initAnimationClip();

                

                // add animation curves if not there already
                if (animData.getAnimationCurve(clip, animationProperties[0]) == null )
                {
                    foreach (string prop in animationProperties)
                    {
                        // add property curve
                        AnimationCurve _curve = new AnimationCurve();

                        //add curve to runtime data representation
                        animData.addAnimationCurve(clip, typeof(Transform), prop, _curve);
                    }
                }


                for ( int n=0; n<animationProperties.Length; n++)
                {
                    // named property in curve
                    string prop = animationProperties[n];
                    // property delegate
                    PropertyInfo field = animationFields[n];


                    // get or create curve
                    AnimationCurve _curve = animData.getAnimationCurve(clip, prop);
                    if ( _curve == null )
                    {
                        _curve = new AnimationCurve();
                    }

                    // add or move keyframe
                    bool movedSuccessfully = false;
                    int keyIndex = -1;
                    Keyframe key = new Keyframe(currentAnimationTime, (float)field.GetValue(animationInstance, null) );
                    for (int i = 0; i < _curve.length; i++)
                    {
                        if (Mathf.Abs(_curve.keys[i].time - currentAnimationTime) < 0.04)
                        {
                            _curve.MoveKey(i, key);
                            keyIndex = i;
                            movedSuccessfully = true;
                        }
                    }
                    if (!movedSuccessfully)
                    {
                        keyIndex = _curve.AddKey(key);
                        movedSuccessfully = false;
                    }
                    if (_curve.keys.Length > 1)
                    {
                        if (keyIndex == 0) _curve.SmoothTangents(1, 0);
                        if (keyIndex == _curve.keys.Length - 1) _curve.SmoothTangents(_curve.keys.Length - 2, 0);
                    }
                    _curve.SmoothTangents(keyIndex, 0);

                    // update animation data
                    animData.changeAnimationCurve( clip, typeof(Transform), prop, _curve);

                }

                // TODO: cheesy
                // animationTarget.GetComponent<SceneObject>().setKeyframe();
                animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
                updateTimelineKeys();
                updateLine();
            }
        }


        //!
        //! updates the rendered line (animation path)
        //!
        private void updateLine()
	    {
	        if (animData.getAnimationClips(animationTarget) != null )
	        {
	            foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
	            {
                    if (animData.getAnimationCurve(clip, "m_LocalPosition.x") == null)
                        continue;
                    //! AnimationCurves for X, Y and Z Translation of this clip
                    AnimationCurve transXcurve = animData.getAnimationCurve(clip, "m_LocalPosition.x");
	                AnimationCurve transYcurve = animData.getAnimationCurve(clip, "m_LocalPosition.y");
	                AnimationCurve transZcurve = animData.getAnimationCurve(clip, "m_LocalPosition.z");
	
	                int pointCount = 0;
	                List<Vector3[]> pointArraysList = new List<Vector3[]>(0);

                    if (updateAppearance)
                    {
                        Vector3 lineMiddle = Vector3.zero;
                        for (int i = 0; i < transXcurve.keys.Length; i++)
                        {
                            lineMiddle += animationTarget.transform.parent.TransformPoint(new Vector3(transXcurve.keys[i].value, transYcurve.keys[i].value, transZcurve.keys[i].value));
                        }
                        lineMiddle /= transXcurve.keys.Length;
                        keyHandleScale = Vector3.Distance(Camera.main.transform.position, lineMiddle) / 100f;
                        lineRenderer.startWidth = keyHandleScale /3f;
                        lineRenderer.endWidth = lineRenderer.startWidth;
                        updateAppearance = false;
                    }

	
	                for (int i = 0; i < transXcurve.keys.Length; i++)
	                {
	                    if (i != transXcurve.keys.Length - 1)
	                    {
	                        Vector3[] pointArray = getHermiteInterpolationLine(transXcurve.keys[i], transYcurve.keys[i], transZcurve.keys[i], transXcurve.keys[i + 1], transYcurve.keys[i + 1], transZcurve.keys[i + 1]);
	                        pointArraysList.Add(pointArray);
	                        pointCount += pointArray.Length;
	                    }
	                    if (i < keyframeSpheres.Count)
	                    {
	                        keyframeSpheres[i].transform.position = new Vector3(transXcurve.keys[i].value, transYcurve.keys[i].value, transZcurve.keys[i].value);
	                    }
	                    else
	                    {
	                        GameObject sphere = GameObject.Instantiate<GameObject>(keySpherePrefab);
                            sphere.transform.position = animationTarget.transform.parent.TransformPoint(new Vector3(transXcurve.keys[i].value, transYcurve.keys[i].value, transZcurve.keys[i].value));
                            sphere.transform.localScale = new Vector3(keyHandleScale, keyHandleScale, keyHandleScale);
	                        sphere.transform.parent = frameSphereContainer.transform;
	                        sphere.layer = 13;
	                        sphere.name = i.ToString();
	                        keyframeSpheres.Add(sphere);
	                    }
	                }
	                if (transXcurve.keys.Length < keyframeSpheres.Count)
	                {
	                    for (int i = transXcurve.keys.Length; i < keyframeSpheres.Count; i++)
	                    {
	                        Destroy(keyframeSpheres[i], 0);
	                    }
	                    keyframeSpheres.RemoveRange(transXcurve.keys.Length, keyframeSpheres.Count - transXcurve.keys.Length);
	                }
	
                    lineRenderer.positionCount = pointCount;
	                int currentPosition = 0;
	                Vector3 lastPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	
	                for (int i = 0; i < pointArraysList.Count; i++)
	                {
	                    for (int j = 0; j < pointArraysList[i].Length; j++)
	                    {
	                        if (lastPoint != pointArraysList[i][j])
	                        {
	                            lineRenderer.SetPosition(currentPosition, pointArraysList[i][j]);
	                            lastPoint = pointArraysList[i][j];
	                            currentPosition++;
	                        }
	                        else
	                        {
	                            pointCount--;
                                lineRenderer.positionCount = pointCount;
	                        }
	                    }
	                }
	            }
	        }
	    }
	
	
	    //!
	    //! add keyframes to timeline from all curves of current selection
	    //!
	    public void updateTimelineKeys()
	    {
	        // clear
	        timeLine.clearFrames();
	        // add
	        if (animationTarget != null && animData.getAnimationClips(animationTarget) != null)
	        {
                foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
                {
                    if (animationProperties.Length > 1)
                    {
                        foreach (string prop in animationProperties)
                        {
                            AnimationCurve animCurve = animData.getAnimationCurve(clip, prop);
                            if ( animCurve != null)
                               timeLine.UpdateFrames(animCurve, animationTarget.GetComponent<SceneObject>().animationLayer);
                        }
                    }
                    else
                    {
                        foreach (AnimationCurve animCurve in animData.getAnimationCurves(clip))
                        {
                            if (animCurve != null)
                                timeLine.UpdateFrames(animCurve, animationTarget.GetComponent<SceneObject>().animationLayer);
                        }
                    }
                }
	        }

            timeLine.setTime(currentAnimationTime);

	    }
	
	
	    //!
	    //! extend start/end
	    //!
	    private  void setStartEndTimeline(float s, float e)
	    {
	        timeLine.StartTime = s;
	        timeLine.EndTime =  e > 0f ? e : 5;
            // setTime(timeLine.StartTime);
        }

        private void setStartEndTimeline()
        {
            float timeStart = 0, timeEnd = 4f;
            if (animationTarget != null && animData.getAnimationClips(animationTarget) != null)
            {
                foreach (AnimationClip _clip in animData.getAnimationClips(animationTarget))
                {
                    foreach (AnimationCurve _curve in animData.getAnimationCurves(_clip))
                    {
                        if (_curve.length > 0)
                        {
                            timeStart = Mathf.Min(timeStart, _curve[0].time);
                            timeEnd = Mathf.Max(_curve[_curve.length - 1].time, timeEnd);
                        }
                    }
                }
            }
            setStartEndTimeline(timeStart, timeEnd);
            // triger set time to clamp to [start,end]
            setTime(currentAnimationTime);

            
        }

        public void activate( bool updateTimeLine = true )
        {
            if (mainController.getCurrentSelection() == null) return;

            animationTarget = mainController.getCurrentSelection().gameObject;

            isActive = true;

            animationTarget.GetComponent<SceneObject>().setKinematic(true, false);

            animationTarget.layer = 13; //noPhysics layer

            animationProperties = new string[] { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z",
                                                 "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w",
                                                 "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z"};
            animationFields = new PropertyInfo[] { animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateZ") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatZ") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatW") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleZ") };
            animationInstance = animationTarget.GetComponent<SceneObject>();

            //
			// this was created on request to keep keys individually from current selection mode but turned out to be not practical,
            // keep for potential future keying options
			//
			
            /*
            switch (mainController.ActiveMode)
            {
                case MainController.Mode.translationMode: case MainController.Mode.pointToMoveMode: case MainController.Mode.objectLinkCamera:
                    animationProperties = new string[] { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z" };
                    animationFields = new PropertyInfo[] { animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("TranslateZ") };
                    animationInstance = animationTarget.GetComponent<SceneObject>();
                    break;
                case MainController.Mode.rotationMode:
                    animationProperties = new string[] { "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w" };
                    animationFields = new PropertyInfo[] { animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatZ") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("RotateQuatW") };
                    animationInstance = animationTarget.GetComponent<SceneObject>();
                    break;
                case MainController.Mode.scaleMode:
                    animationProperties = new string[] { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" };
                    animationFields = new PropertyInfo[] { animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleX")  ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleY") ,
                                                         animationTarget.GetComponent<SceneObject>().GetType().GetProperty("ScaleZ") };
                    animationInstance = animationTarget.GetComponent<SceneObject>();
                    break;
            }
            print("Activate Animation in Mode: " + mainController.ActiveMode);
            */


            if ( updateTimeLine ) setStartEndTimeline();


            animationTarget.GetComponent<SceneObject>().updateAnimationCurves();

            lineRenderer.positionCount = 0;
            lineRenderer.enabled = true;

            updateAppearance = true;

            updateLine();

            updateTimelineKeys();


        }

        private AnimationClip initAnimationClip()
        {
            if (!animationTarget.GetComponent<AnimationSerializer>())
            {
                animationTarget.AddComponent<AnimationSerializer>();
            }

            AnimationClip clip;

            //no animation available yet for this object -> create animation
            if (animData.getAnimationClips(animationTarget) == null)
            {
                clip = new AnimationClip();
                animData.addAnimationClip(animationTarget, clip);
                animatedObjects.Add(animationTarget.GetComponent<SceneObject>());
            }
            else
            {
                clip = animData.getAnimationClips(animationTarget)[0];
            }

            return clip;
        }

        //!
        //! deactivates animation editing
        //!
        public void deactivate()
	    {
            if (animationTarget == null) return;

            animationProperties = new string[] { };

            //animationTarget.GetComponent<SceneObject>().setKinematic(false, false);
            animationTarget.layer = 0;
            isActive = false;
	
            lineRenderer.positionCount = 0;
	        lineRenderer.enabled = false;
	        foreach (GameObject sphere in keyframeSpheres)
	        {
	            Destroy(sphere, 0);
	        }
	        keyframeSpheres.Clear();
	        animationTarget = null;

            timeLine.clearFrames();

	    }
	
	    //!
	    //! switches between play and pause mode of the animation
	    //! receiving function of Play/Pause Button
	    //!
	    public void togglePlayPause()
	    {
	        if (!playing)
	        {
	            editingPosition = false;
	            mainController.hideModifiers();
	            playing = true;
	            systemTimeOffset = Time.time - currentAnimationTime;
                // mainController.liveMode = true;
	        }
	        else
	        {
                // mainController.liveMode = false;
                playing = false;
                // stop all layers
                foreach ( AnimationLayer animLayer in animationLayers )
                {
                    animLayer.isPlaying = false;
                }
            }

            foreach (SceneObject obj in animatedObjects)
            {
                obj.isPlayingAnimation = playing;
                serverAdapter.SendObjectUpdate(obj, ParameterType.HIDDENLOCK);
            }
            
            foreach (AnimationLayer layer in animationLayers)
            {
                //animation is playing but not on this layer
                if (!layer.isPlaying && playing)
                    foreach (SceneObject obj in layer.layerObjects)
                    {
                        obj.isPlayingAnimation = false;
                        serverAdapter.SendObjectUpdate(obj, ParameterType.HIDDENLOCK);
                    }
                else
                    foreach (SceneObject obj in layer.layerObjects)
                    {
                        obj.isPlayingAnimation = playing;
                        serverAdapter.SendObjectUpdate(obj, ParameterType.HIDDENLOCK);
                    }
            }
        }
	
	    //!
	    //! 
	    //!
	    private void timeChanged()
	    {
            foreach (SceneObject animatedObject in animatedObjects)
            {
                animatedObject.setAnimationState(currentAnimationTime);
            }

            for (int i = 0; i < animationLayers.Length; ++i)
            {
                AnimationLayer animationLayer = animationLayers[i];
                if (!animationLayer.isPlaying)
                    continue;

                //float currentLayerTime = currentAnimationTime + animationLayer.offset;
                //if (currentAnimationTime > animationLayer.offset)
                animationLayer.currentAnimationTime += Time.deltaTime;

                foreach (SceneObject layerObject in animationLayer.layerObjects)
                {
                    //if (currentLayerTime <= layerObject.animationDuration)
                    //    layerObject.setAnimationState(currentLayerTime);
                    
                    // TODO: NILS: bug since Unity 5. Fix pending. Animation duration always 1.
                    //if (animationLayer.currentAnimationTime <= layerObject.animationDuration)
                    layerObject.setAnimationState(animationLayer.currentAnimationTime);
                }
            }

            timeLine.setTime(currentAnimationTime);
	    }
	
	
	    //!
	    //! 
	    //!
	    public void previousKeyframe()
	    {
            if (animationTarget == null || animData.getAnimationClips(animationTarget) == null) return;

            float newTime = timeLine.StartTime; ;
	
	        foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
	        {
                if (animationProperties.Length > 0)
                {
                    foreach (string prop in animationProperties)
                    {
                        AnimationCurve animCurve = animData.getAnimationCurve(clip, prop);
                        for (int i = 0; i < animCurve.keys.Length; i++)
                        {
                            if (animCurve.keys[i].time < currentAnimationTime && animCurve.keys[i].time > newTime)
                            {
                                newTime = animCurve.keys[i].time;
                            }
                        }
                    }
                }
                else
                {
                    foreach (AnimationCurve animCurve in animData.getAnimationCurves(clip))
                    {
                        for (int i = 0; i < animCurve.keys.Length; i++)
                        {
                            if (animCurve.keys[i].time < currentAnimationTime && animCurve.keys[i].time > newTime)
                            {
                                newTime = animCurve.keys[i].time;
                            }
                        }
                    }
                }
	        }


	
	        currentAnimationTime = newTime;
	
	        timeChanged();
	
	    }
	
	
	    public void nextKeyframe()
	    {
            if (animationTarget == null || animData.getAnimationClips(animationTarget) == null) return;

            float newTime = timeLine.EndTime;
	
	        foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
	        {
                if (animationProperties.Length > 0)
                {
                    foreach (string prop in animationProperties)
                    {
                        AnimationCurve animCurve = animData.getAnimationCurve(clip, prop);
                        for (int i = 0; i < animCurve.keys.Length; i++)
                        {
                            if (animCurve.keys[i].time > currentAnimationTime && animCurve.keys[i].time < newTime)
                            {
                                newTime = animCurve.keys[i].time;
                            }
                        }
                    }
                }
                else
                {
                    foreach (AnimationCurve animCurve in animData.getAnimationCurves(clip))
                    {
                        for (int i = 0; i < animCurve.keys.Length; i++)
                        {
                            if (animCurve.keys[i].time > currentAnimationTime && animCurve.keys[i].time < newTime)
                            {
                                newTime = animCurve.keys[i].time;
                            }
                        }
                    }
                }

            }
	
	        currentAnimationTime = newTime;
	
	        timeChanged();
	
	    }
	
	
	    /*
	    //!
	    //! starts timeline drag (shifts the time on the timeline)
	    //!
	    public void startDrag()
	    {
	        initialDragPositionX = Input.mousePosition.x;
	        animationTimeDragStart = currentAnimationTime;
	        dragArea.SetActive(true);
	        dragging = true;
	        editingPosition = false;
	        mainController.hideModifiers();
	    }
	
	    //!
	    //! stop timeline drag (shifts the time on the timeline)
	    //!
	    public void stopDrag()
	    {
	        dragging = false;
	        initialDragPositionX = 0;
	        dragArea.SetActive(false);
	    }
        */
	
	    //!
	    //! calculate an array of interpolated waypoints of a Hermit curve defined by two keyframes
	    //! it only adds subdivisions where needed
	    //! smoothness might be controlled by gobal variable "hermitInterpolationRate" (0 -> high amount of subdivisions)
	    //!
	    private Vector3[] getHermiteInterpolationLine(Keyframe startXKey, Keyframe startYKey, Keyframe startZKey, Keyframe endXKey, Keyframe endYKey, Keyframe endZKey)
	    {
	        Vector3 startPoint = new Vector3(startXKey.value, startYKey.value, startZKey.value);
	        Vector3 startOutTangent = new Vector3(startXKey.outTangent, startYKey.outTangent, startZKey.outTangent);
	        Vector3 endPoint = new Vector3(endXKey.value, endYKey.value, endZKey.value);
	        Vector3 endInTangent = new Vector3(endXKey.inTangent, endYKey.inTangent, endZKey.inTangent);
	        float deltaTime = endXKey.time - startXKey.time;
	
	        Vector3[] outPointArray;
            int subdivisionCount = Mathf.RoundToInt((Vector3.Angle(startOutTangent, endPoint - startPoint) + Vector3.Angle(endInTangent, endPoint - startPoint)) / hermitInterpolationRate)*2 + 2;
	        outPointArray = new Vector3[subdivisionCount];
	        float s = 0;
	        for (int i = 0; i < subdivisionCount; i++)
	        {
	            s = (1.0f / (subdivisionCount - 1)) * i;
	            float H1 = 2 * s * s * s - 3 * s * s + 1;
	            float H2 = -2 * s * s * s + 3 * s * s;
	            float H3 = s * s * s - 2 * s * s + s;
	            float H4 = s * s * s - s * s;
                outPointArray[i] = animationTarget.transform.parent.TransformPoint(new Vector3(H1 * startPoint.x + H2 * endPoint.x + H3 * startOutTangent.x * deltaTime + H4 * endInTangent.x * deltaTime,
	                                                                                           H1 * startPoint.y + H2 * endPoint.y + H3 * startOutTangent.y * deltaTime + H4 * endInTangent.y * deltaTime,
                                                                                               H1 * startPoint.z + H2 * endPoint.z + H3 * startOutTangent.z * deltaTime + H4 * endInTangent.z * deltaTime));
	        }
	
	        return outPointArray;
	    }
	
        /*
	    //!
	    //! starts the tracking of the currently selected sceneObject's keyframe
	    //!
	    public void enablePositionEditing()
	    {
	        editingPosition = true;
	        playing = false;
            if ( animationTarget )
    	        animationTarget.GetComponent<SceneObject>().setKeyframe();
	    }
	    */

        
	    //!
	    //! updates the curves applied on the current animation target
	    //!
	    public void updateAnimationTarget()
	    {
	        animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
	    }
	
	    //!
	    //! registers an animated object
	    //! this registration is needed to be able to apply a new time (triggered by user or play mode) to the entire scene
	    //! @param      obj     SceneObject to be registered
	    //!
	    public void registerAnimatedObject(SceneObject obj)
	    {
	        if (animatedObjects == null)
	        {
	            animatedObjects = new List<SceneObject>();
	        }
	        animatedObjects.Add(obj);
	    }

        //!
        //! add current object to layer
        //!
        public void addSelectedObjectToLayer(int layerIdx)
        {
            SceneObject selectedObject = mainController.getCurrentSelection().GetComponent<SceneObject>();
            int oldLayerIdx = selectedObject.animationLayer;

            // remove from default playlist
            if (animatedObjects.Contains(selectedObject))
                animatedObjects.Remove(selectedObject);

            // remove from previous layer if was assigned
            if ( oldLayerIdx>0 &&  animationLayers[oldLayerIdx].layerObjects.Contains(selectedObject))
                animationLayers[oldLayerIdx].layerObjects.Remove(selectedObject);
            
            // add to new layer
            animationLayers[layerIdx].layerObjects.Add(selectedObject);
            selectedObject.animationLayer = layerIdx;

            updateTimelineKeys();
        }

        public void removeSelectedObjectFromLayer( int layerIdx )
        {
            SceneObject selectedObject = mainController.getCurrentSelection().GetComponent<SceneObject>();

            // remove from previous layer if was assigned
            if (animationLayers[layerIdx].layerObjects.Contains(selectedObject))
                animationLayers[layerIdx].layerObjects.Remove(selectedObject);


            selectedObject.animationLayer = -1;
            animatedObjects.Add(selectedObject);

        }


        //!
        //! play animation layer
        //!
        public void playAnimationLayer(int layerIdx)
        {
            if (layerIdx < animationLayers.Length)
            {
                print("Animation Layer Playback: " + layerIdx);
                animationLayers[layerIdx].offset = -currentAnimationTime;
                animationLayers[layerIdx].currentAnimationTime = 0.0f;
                animationLayers[layerIdx].isPlaying = true;
                if (playing)
                    foreach(SceneObject obj in animationLayers[layerIdx].layerObjects)
                            obj.isPlayingAnimation = true;
            }
        }

        //!
        //! stop animation layer
        //!
        public void stopAnimationLayer(int layerIdx)
        {
            if (layerIdx < animationLayers.Length)
                animationLayers[layerIdx].isPlaying = false;
            if (playing)
                foreach (SceneObject obj in animationLayers[layerIdx].layerObjects)
                    obj.isPlayingAnimation = false;
        }

        //!
        //! reset animation layer
        //!
        private void resetLayer(int layerIdx)
        {
            if (layerIdx < animationLayers.Length) {
                AnimationLayer animationLayer = animationLayers[layerIdx];
                foreach (SceneObject layerObject in animationLayer.layerObjects)
                    layerObject.setAnimationState(0.0f);
            }
        }

        public bool IsCurrentSelectionOnLayer(int layerIdx )
        {
            if ( mainController.getCurrentSelection() != null )
            {
                return mainController.getCurrentSelection().GetComponent<SceneObject>().animationLayer == layerIdx;
            }
            else
            {
                return false;
            }
        }


	    //!
	    //! delete the animation attached to the currently selected object
	    //!
	    public void deleteAnimation()
	    {
	        mainController.getCurrentSelection().GetComponent<SceneObject>().setKinematic(false, false);
	        mainController.getCurrentSelection().gameObject.layer = 0;
	        Destroy(mainController.getCurrentSelection().GetComponent<AnimationSerializer>());
	        if (animData.getAnimationClips(mainController.getCurrentSelection().gameObject) != null)
	        {
	            animData.deleteAnimationCurves(animData.getAnimationClips(mainController.getCurrentSelection().gameObject)[0], typeof(Transform), "m_LocalPosition.x");
	            animData.deleteAnimationCurves(animData.getAnimationClips(mainController.getCurrentSelection().gameObject)[0], typeof(Transform), "m_LocalPosition.y");
	            animData.deleteAnimationCurves(animData.getAnimationClips(mainController.getCurrentSelection().gameObject)[0], typeof(Transform), "m_LocalPosition.z");
	            animData.removeAnimationClips(mainController.getCurrentSelection().gameObject);
	        }
	        
	        animatedObjects.Remove(mainController.getCurrentSelection().GetComponent<SceneObject>());
	        animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
            deactivate();
        }
	
		/*
	    //!
	    //! delete a keyframe
	    //! @param  index   index of the keyframe to be deleted
	    //!
	    public void deleteKeyframe(int index)
	    {
	        AnimationCurve[] transCurves = new AnimationCurve[3];
	        if (animData.getAnimationClips(animationTarget) != null)
	        {
	            transCurves[0] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.x");
	            transCurves[1] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.y");
	            transCurves[2] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.z");
	            transCurves[0].RemoveKey(index);
	            transCurves[1].RemoveKey(index);
	            transCurves[2].RemoveKey(index);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.x", transCurves[0]);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.y", transCurves[1]);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.z", transCurves[2]);
	
	            animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
	            timeline.GetComponent<TimelineScript>().updateFrames(transCurves[0], animData.getAnimationClips(animationTarget.gameObject)[0]);
	            updateLine();
	        }
	    }
		*/

		/*
	    //!
	    //! smooth the tangents of a keyframe
	    //! @param  index   index of the keyframe to be deleted
	    //!
	    public void smoothKeyframeTangents(int index)
	    {
	        AnimationCurve[] transCurves = new AnimationCurve[3];
	        if (animData.getAnimationClips(animationTarget) != null)
	        {
	            transCurves[0] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.x");
	            transCurves[1] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.y");
	            transCurves[2] = animData.getAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], "m_LocalPosition.z");
	            transCurves[0].SmoothTangents(index,0);
	            transCurves[1].SmoothTangents(index,0);
	            transCurves[2].SmoothTangents(index,0);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.x", transCurves[0]);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.y", transCurves[1]);
	            animData.changeAnimationCurve(animData.getAnimationClips(animationTarget.gameObject)[0], typeof(Transform), "m_LocalPosition.z", transCurves[2]);
	
	            animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
	            timeline.GetComponent<TimelineScript>().updateFrames(transCurves[0], animData.getAnimationClips(animationTarget.gameObject)[0]);
	            updateLine();
	        }

	    }
		*/
}
}