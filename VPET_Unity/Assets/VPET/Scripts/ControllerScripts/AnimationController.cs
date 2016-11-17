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
using System.Collections.Generic;

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

	public class AnimationController : MonoBehaviour {
	
	    //!
	    //! is animation editing currently active
	    //!
	    public bool isActive = false;
	
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
	    private LineRenderer lineRenderer;
	
	    //!
	    //! color of the line drawn by the lineRenderer
	    //!
	    public Color lineColor;
	
	    //!
	    //! cached reference to animation data (runtime representation)
	    //!
	    private AnimationData animData = null;
	
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
        AnimationLayer[] animationLayers = new AnimationLayer[6];
	
	
	    private GameObject keySpherePrefab;
	
	
	
	    //!
	    //! Use this for initialization
	    //!
	    void Awake()
	    {
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
	        frameSphereContainer = GameObject.Find("FrameSphereContainer");
	
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
	        lineRenderer.SetColors(lineColor, lineColor);
	        lineRenderer.SetWidth(0.02f, 0.02f);
	        lineRenderer.useWorldSpace = true;
	        lineRenderer.SetVertexCount(0);
	    }
	
	
	    void Start()
	    {
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
	        timeChanged();
	    }
	
	
	    public void setKeyFrame()
	    {
	        if (animationTarget != null)
	        {
	            // TODO: cheesy
	            currentAnimationTime = timeLine.CurrentTime;
	            animationTarget.GetComponent<SceneObject>().setKeyframe();
                updateTimelineKeys();
	            updateLine();
	        }
	    }
	
	
	    //!
	    //! updates the rendered line (animation path)
	    //!
	    private void updateLine()
	    {
	        if (animData.getAnimationClips(animationTarget) != null)
	        {
	            foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
	            {
	                //! AnimationCurves for X, Y and Z Translation of this clip
	                AnimationCurve transXcurve = animData.getAnimationCurve(clip, "m_LocalPosition.x");
	                AnimationCurve transYcurve = animData.getAnimationCurve(clip, "m_LocalPosition.y");
	                AnimationCurve transZcurve = animData.getAnimationCurve(clip, "m_LocalPosition.z");
	
	                int pointCount = 0;
	                List<Vector3[]> pointArraysList = new List<Vector3[]>(0);
	
	                for (int i = 0; i <= transXcurve.keys.Length - 1; i++)
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
	                        sphere.transform.position = new Vector3(transXcurve.keys[i].value, transYcurve.keys[i].value, transZcurve.keys[i].value);
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
	
	                lineRenderer.SetVertexCount(pointCount);
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
	                            lineRenderer.SetVertexCount(pointCount);
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
	                foreach (AnimationCurve animCurve in animData.getAnimationCurves(clip))
	                {
	                    timeLine.UpdateFrames(animCurve, animationTarget.GetComponent<SceneObject>().animationLayer);
	                }
	            }
	        }

            timeLine.setTime(currentAnimationTime);

	    }
	
	
	    //!
	    //! extend start/end
	    //!
	    public void setStartEndTimeline(float s, float e)
	    {
	        timeLine.StartTime = s;
	        timeLine.EndTime =  e > 0f ? e : 5;
            setTime(timeLine.StartTime);
        }
	

            
        //!
	    //! activates animation editing for the currently selected object
	    //!
	    public void activate()
	    {

            // Debug.Log("mainController.getCurrentSelection()", mainController.getCurrentSelection().gameObject);

            isActive = true;
	
	        mainController.ActiveMode = MainController.Mode.animationEditing;
	        animationTarget = mainController.getCurrentSelection().gameObject;
	
	        animationTarget.GetComponent<SceneObject>().setKinematic(true, false);
	        animationTarget.layer = 13; //noPhysics layer
	
	        //no animation available yet for this object -> create animation
	        if (doCreateClip && animData.getAnimationClips(animationTarget) == null)
	        {
	            if (!animationTarget.GetComponent<AnimationSerializer>())
	            {
	                animationTarget.AddComponent<AnimationSerializer>();
	            }
	
	
	            //create initial animation translation
	            AnimationClip clip = new AnimationClip();
	            AnimationCurve transXcurve = new AnimationCurve();
	            AnimationCurve transYcurve = new AnimationCurve();
	            AnimationCurve transZcurve = new AnimationCurve();
	
	            // create keys at current time
	            transXcurve.AddKey(new Keyframe(0, animationTarget.transform.position.x, -1, 1));
	            transYcurve.AddKey(new Keyframe(0, animationTarget.transform.position.y, -1, 1));
	            transZcurve.AddKey(new Keyframe(0, animationTarget.transform.position.z, -1, 1));
	
	
	            /*
	            transXcurve.AddKey(new Keyframe(1, animationTarget.transform.position.x + 1, 1, -1));
	            transYcurve.AddKey(new Keyframe(1, animationTarget.transform.position.y + 1, 1, -1));
	            transZcurve.AddKey(new Keyframe(1, animationTarget.transform.position.z + 1, 1, -1));
	            */
	
	            // timeline.GetComponent<TimelineScript>().updateFrames(transXcurve,clip);
	            // timeLine.updateFrames(transXcurve);
	
	            //add animation to runtime data representation
	            animData.addAnimationClip(animationTarget, clip);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalPosition.x", transXcurve);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalPosition.y", transYcurve);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalPosition.z", transZcurve);
	
	
	            //create initial animation Rotation curve
	            AnimationCurve rotXcurve = new AnimationCurve();
	            AnimationCurve rotYcurve = new AnimationCurve();
	            AnimationCurve rotZcurve = new AnimationCurve();
	            AnimationCurve rotWcurve = new AnimationCurve();
	
	            // create keys at current time
	            rotXcurve.AddKey(new Keyframe(0, animationTarget.transform.rotation.x, -1, 1));
	            rotYcurve.AddKey(new Keyframe(0, animationTarget.transform.rotation.y, -1, 1));
	            rotZcurve.AddKey(new Keyframe(0, animationTarget.transform.rotation.z, -1, 1));
	            rotWcurve.AddKey(new Keyframe(0, animationTarget.transform.rotation.w, -1, 1));
	
	            /*
	            rotXcurve.AddKey( new Keyframe( 1, animationTarget.transform.rotation.x, -1, 1 ) );
	            rotYcurve.AddKey( new Keyframe( 1, animationTarget.transform.rotation.y, -1, 1 ) );
	            rotZcurve.AddKey( new Keyframe( 1, animationTarget.transform.rotation.z, -1, 1 ) );
	            rotWcurve.AddKey( new Keyframe( 1, animationTarget.transform.rotation.w, -1, 1 ) );
	            */
	
	            // timeline.GetComponent<TimelineScript>().updateFrames( transXcurve, clip );
	            // timeLine.updateFrames( transXcurve );
	
	
	            //add animation to runtime data representation
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalRotation.x", rotXcurve);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalRotation.y", rotYcurve);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalRotation.z", rotZcurve);
	            animData.addAnimationCurve(clip, typeof(Transform), "m_LocalRotation.w", rotWcurve);

	            animatedObjects.Add(animationTarget.GetComponent<SceneObject>());
	        }
	
	        animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
	
	
	        lineRenderer.SetVertexCount(0);
	        lineRenderer.enabled = true;
	
	        updateLine();
	
	        updateTimelineKeys();
	
	    }
	
	    //!
	    //! deactivates animation editing
	    //!
	    public void deactivate()
	    {
	        //animationTarget.GetComponent<SceneObject>().setKinematic(false, false);
	        //animationTarget.layer = 0;
	        isActive = false;
	        //animationTarget.GetComponent<SceneObject>().selected = false;
	
	        //timeline.gameObject.SetActive(false);
	
	        lineRenderer.SetVertexCount(0);
	        lineRenderer.enabled = false;
	        foreach (GameObject sphere in keyframeSpheres)
	        {
	            Destroy(sphere, 0);
	        }
	        keyframeSpheres.Clear();
	        animationTarget = null;
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
	        }
	        else
	        {
	            playing = false;
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
            float newTime = timeLine.StartTime; ;
	
	        foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
	        {
	            foreach (AnimationCurve animCurve in animData.getAnimationCurves(clip))
	            {
	                for (int i = 0; i < animCurve.keys.Length; i++)
	                {
	                    if ( animCurve.keys[i].time < currentAnimationTime && animCurve.keys[i].time > newTime )
	                    {
	                        newTime = animCurve.keys[i].time;
	                    }
	                }
	            }
	        }
	
	        currentAnimationTime = newTime;
	
	        timeChanged();
	
	    }
	
	
	    public void nextKeyframe()
	    {
	        float newTime = timeLine.EndTime;
	
	        foreach (AnimationClip clip in animData.getAnimationClips(animationTarget))
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
	        int subdivisionCount = Mathf.RoundToInt((Vector3.Angle(startOutTangent, endPoint - startPoint) + Vector3.Angle(endInTangent, endPoint - startPoint)) / hermitInterpolationRate) + 2;
	        outPointArray = new Vector3[subdivisionCount];
	        float s = 0;
	        for (int i = 0; i < subdivisionCount; i++)
	        {
	            s = (1.0f / (subdivisionCount - 1)) * i;
	            float H1 = 2 * s * s * s - 3 * s * s + 1;
	            float H2 = -2 * s * s * s + 3 * s * s;
	            float H3 = s * s * s - 2 * s * s + s;
	            float H4 = s * s * s - s * s;
	            outPointArray[i] = new Vector3(H1 * startPoint.x + H2 * endPoint.x + H3 * startOutTangent.x * deltaTime + H4 * endInTangent.x * deltaTime,
	                                           H1 * startPoint.y + H2 * endPoint.y + H3 * startOutTangent.y * deltaTime + H4 * endInTangent.y * deltaTime,
	                                           H1 * startPoint.z + H2 * endPoint.z + H3 * startOutTangent.z * deltaTime + H4 * endInTangent.z * deltaTime);
	        }
	
	        return outPointArray;
	    }
	
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
            if (oldLayerIdx < 0) {
                if (animatedObjects.Contains(selectedObject))
                    animatedObjects.Remove(selectedObject);
            }
            else if (animationLayers[oldLayerIdx].layerObjects.Contains(selectedObject))
                animationLayers[oldLayerIdx].layerObjects.Remove(selectedObject);

            if (layerIdx < 0)
            {
                animatedObjects.Add(selectedObject);
                selectedObject.animationLayer = layerIdx;
            }
            else if (layerIdx < animationLayers.Length)
            {
                animationLayers[layerIdx].layerObjects.Add(selectedObject);
                selectedObject.animationLayer = layerIdx;
            }

            updateTimelineKeys();
        }

        //!
        //! play animation layer
        //!
        public void playAnimationLayer(int layerIdx)
        {
            if (layerIdx < animationLayers.Length)
            {
                animationLayers[layerIdx].offset = -currentAnimationTime;
                animationLayers[layerIdx].currentAnimationTime = 0.0f;
                animationLayers[layerIdx].isPlaying = true;
            }
        }

        //!
        //! stop animation layer
        //!
        public void stopAnimationLayer(int layerIdx)
        {
            if (layerIdx < animationLayers.Length)
                animationLayers[layerIdx].isPlaying = false;
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
	        //animationTarget.GetComponent<SceneObject>().updateAnimationCurves();
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