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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace vpet
{
	public class TimeLineWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
	{
	    //!
	    //! width of the timeline (is adjusted based on the screen resolution and DPI)
	    //!
	    //int width = 600;
	
	    //!
	    //! height of the timeline (is adjusted based on the screen resolution and DPI)
	    //!
	   // int height = 25;
	
	    //!
	    //! text displayed above the timeline, showing the start frame number
	    //!
	    Text startFrameDisplay;
	
	    //!
	    //! text displayed above the timeline, showing the end frame number
	    //!
	    Text endFrameDisplay;
	    
	    //!
	    //! text displayed above the timeline, showing the current frame number
	    //!
	    Text currentFrameDisplay;
	
	    //!
	    //! reference to the prefab of a keyframe box in the timeline
	    //!
	    private GameObject keyframeImagePrefab;
	
	    //!
	    //! container object containing all keyframes displayed in the GUI timeline
	    //!
	    RectTransform frameContainer;
	
	    //!
	    //! list in which all GUI keyframes are registered
	    //!
	    List<GameObject> frameList;
	
	    private RectTransform redLine;
	
	    //!
	    //! currently associated animationclip
	    //!
	    AnimationClip clip;
	
	    private UnityAction<float> callback;
	
	    public UnityAction<float> Callback
	    {
	        set { callback = value; }
	    }
	
	    //!
	    //! current time of the animation
	    //!
	    private float currentTime = 0;
	    public float CurrentTime
	    {
	        get { return currentTime;  }
	    }
	
	    //!
	    //! visible start time of the timeline
	    //!
	    private float startTime = -10;
	    public float StartTime
	    {
            get { return startTime; }
	        set { startTime = value;
	            startFrameDisplay.text = Mathf.RoundToInt(startTime * 25).ToString();
	        }
	    }
	
	    //!
	    //! visible end time of the timeline
	    //!
	    private float endTime = 5;
	    public float EndTime
	    {
            get { return endTime; }
	        set {
	            endTime = value;
	            endFrameDisplay.text = Mathf.RoundToInt(endTime * 25).ToString();
	        }
	    }
	
	    //!
	    //! is the currently edited animation looped
	    //!
	    public bool isLooping = false;
	

        private float startTimeDragInit = 0;
        private float endTimeDragInit = 1;
        private float timeDragStart = 0;
        private float dragVelocity = 0;
        private float pinchInitDistance = 0;
        private float widthPinchStart = 1;


        private bool doPinch = false;

        void Awake()
	    {
	        frameList = new List<GameObject>();
	        frameContainer = this.GetComponent<RectTransform>();
	        
	        // get keyframe prefab
	        keyframeImagePrefab = Resources.Load<GameObject>("VPET/KeyFrameTemplate");
	        if (keyframeImagePrefab == null) Debug.LogError(string.Format("{0}: Cant find Resources: KeyFrameTemplate.", this.GetType()));
	
	        // get redline component
	        redLine = this.transform.GetChild(0).GetComponent<RectTransform>();
	        if (redLine == null) Debug.LogError(string.Format("{0}: No RectTransform Component attached.", this.GetType()));
	        
	        // get text component
	        startFrameDisplay = this.transform.Find("StartFrameNumber").GetComponent<Text>();
	        if (startFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component: StartFrameNumber.", this.GetType()));
	
	        // get text component
	        endFrameDisplay = this.transform.Find("EndFrameNumber").GetComponent<Text>();
	        if (endFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component: EndFrameNumber.", this.GetType()));
	
	        // get text component
	        currentFrameDisplay = redLine.GetChild(0).GetComponent<Text>();
	        if (currentFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component below RedLine.", this.GetType()));
	    }
	
	    void Start()
	    {

            startFrameDisplay.text = Mathf.RoundToInt(startTime * 25).ToString();
            endFrameDisplay.text = Mathf.RoundToInt(endTime * 25).ToString();

            /* debug
	        Keyframe[] keys = new Keyframe[3];
	        keys[0] = new Keyframe(-10, -1);
	        keys[1] = new Keyframe(2, 0);
	        keys[2] = new Keyframe(5, 1);
	        AnimationCurve testCurve = new AnimationCurve(keys);
	        updateFrames(testCurve);
	        */

            /* debug
	        addFrame(-15f);
	        addFrame(-10f);
	        addFrame(0f);
	        addFrame(5f);
	        */

            setTime(2f);

	    }

	    //!
	    //! add a frame representing image to the timeline
	    //! @param      time    time in animation at which to add the keyframe to the timeline
	    //!
	    private void addFrame(float time, int layer)
	    {
	        GameObject img = GameObject.Instantiate<GameObject>(keyframeImagePrefab);
	        img.transform.SetParent(frameContainer, false);
	        img.transform.SetAsFirstSibling();
	        img.GetComponent<KeyFrame>().currentTime = time;
            img.GetComponent<KeyFrame>().SetLayerId(layer);


            img.name = frameList.Count.ToString();
	        frameList.Add(img);
	
	        if (startTime <= time && time <= endTime)
	        {
	            img.SetActive(true);
	        }
	        else
	        {
	            img.SetActive(false);
	        }
	
	        KeyFrame keyframeScript = img.GetComponent<KeyFrame>();
	        keyframeScript.Callback = this.setTimeFromGlobalPositionX;
	    }
	
	    private float _map(float x, float a1, float b1, float a2, float b2)
	    {
	        return (x * (b2 - a2) - a1 * b2 + a2 * b1) / (b1 - a1);
	    }
	
	    private float mapToTimelinePosition( float x)
	    {
            return _map(x, startTime, endTime, -frameContainer.sizeDelta.x / 2f, frameContainer.sizeDelta.x / 2f);
	    }
	
	    private float mapToCurrentTime( float x )
	    {
            return _map(x, -frameContainer.sizeDelta.x / 2f, frameContainer.sizeDelta.x / 2f, startTime, endTime );
        }


	    //!
	    //! set the current time (of the animation) in the timeline
	    //! @param      time        current time at the red line of the timeline
	    //!
	    public void setTime(float time)
	    {
	        currentTime = time;
            redLine.localPosition = new Vector3( mapToTimelinePosition(currentTime), 0, 0);
	        currentFrameDisplay.text = Mathf.RoundToInt(time * 25) + " f";
	    }
	
	    //!
	    //! updates the displayed frames according to the given animation curve
	    //! will add/remove frames if neccessary
	    //! @param      curve       animation curve for which to display the timeline
	    //!
	    public void UpdateFrames(AnimationCurve curve, int layer)
	    {
	        // clip = associatedClip;
	        for (int i = 0; i < curve.keys.Length; i++)
	        {
	            bool exists = false;
	            // check if there is already a key // TODO: smarter search
	            foreach( GameObject img in frameList )
	            {
	                if ( img.GetComponent<KeyFrame>().currentTime == curve.keys[i].time )
	                {
	                    exists = true;
	                    break;
	                }
	            }
	            if ( !exists )
	                addFrame(curve.keys[i].time, layer+1);
	        }

            UpdateFrames();

	    }

        //!
        //! updates position of the displayed frames. will show or hide keyframes according to [start,end]. E.g. called after timeline visible range changed
        //!
        public void UpdateFrames()
        {
            foreach (GameObject img in frameList)
            {
                float _time = img.GetComponent<KeyFrame>().currentTime;
                if (_time < startTime || _time > endTime)
                    img.SetActive(false);
                else
                    img.SetActive(true);
                img.GetComponent<RectTransform>().localPosition = new Vector3(mapToTimelinePosition(_time ), 0, 0);
            }
        }

        public void clearFrames()
	    {
	        foreach (GameObject g in frameList)
	        {
	            GameObject.Destroy(g);
	        }
	        frameList.Clear();
	    }

        public void OnPointerClick(PointerEventData data)
        {
            if (callback != null)
            {
                callback(mapToCurrentTime(frameContainer.InverseTransformPoint(data.position).x));
            }
            else
            {
                setTime(mapToCurrentTime(frameContainer.InverseTransformPoint(data.position).x));
            }
        }

        // DRAG
        public void OnBeginDrag(PointerEventData data)
	    {
            // Debug.Log("BEGIN DRAG TIMELINE " + data.position);

            startTimeDragInit = startTime;
            endTimeDragInit = endTime;
            timeDragStart = mapToCurrentTime(frameContainer.InverseTransformPoint(data.position).x);

            if (Input.touchCount > 1 ) // start drag or pinch timeline
            {
                pinchInitDistance = Mathf.Abs(Input.GetTouch(0).position.x - Input.GetTouch(1).position.x);
            }
            else if (Input.GetKey(KeyCode.LeftAlt) )
            {
                pinchInitDistance = Input.mousePosition.x;
            }
        }

        public void OnDrag(PointerEventData data)
	    {
            // Debug.Log("DRAG TIMELINE");
            if (Input.touchCount > 1 ) // drag or pinch timeline
            {
                if ( Mathf.Abs(Mathf.Abs(Input.GetTouch(0).position.x - Input.GetTouch(1).position.x) - pinchInitDistance) > 20 ) // pinch
                {
                    float pinchFactor = 1 + (Mathf.Abs(Input.GetTouch(0).position.x - Input.GetTouch(1).position.x) - pinchInitDistance) / Screen.width * 2f; ;
                    float widthPrev = endTimeDragInit - startTimeDragInit;
                    float widthDeltaHalf = (widthPrev * pinchFactor - widthPrev);
                    StartTime = startTimeDragInit + widthDeltaHalf;
                    EndTime = endTimeDragInit - widthDeltaHalf;
                }
                else // move
                {
                    float timeOffset = timeDragStart - _map(frameContainer.InverseTransformPoint(data.position).x, -frameContainer.sizeDelta.x / 2f, frameContainer.sizeDelta.x / 2f, startTimeDragInit, endTimeDragInit);
                    StartTime = startTimeDragInit + timeOffset;
                    EndTime = endTimeDragInit + timeOffset;
                }

                UpdateFrames();

            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                if ( Input.GetKey( KeyCode.LeftControl )) // pinch
                {
                    // normalized distance
                    float pinchFactor = 1f + (Input.mousePosition.x - pinchInitDistance) / Screen.width * 2f; 
                    float widthPrev = endTimeDragInit - startTimeDragInit;
                    float widthDeltaHalf = (widthPrev * pinchFactor - widthPrev) / 2f;
                    StartTime = startTimeDragInit + widthDeltaHalf;
                    EndTime = endTimeDragInit - widthDeltaHalf;
                }
                else // move
                {
                    float timeOffset = timeDragStart - _map(frameContainer.InverseTransformPoint(data.position).x, -frameContainer.sizeDelta.x / 2f, frameContainer.sizeDelta.x / 2f, startTimeDragInit, endTimeDragInit);
                    StartTime = startTimeDragInit + timeOffset;
                    EndTime = endTimeDragInit + timeOffset;
                }
                
                UpdateFrames();
            }
            else // move time cursor
            {
                if (callback != null)
                {
                    callback( mapToCurrentTime(frameContainer.InverseTransformPoint(data.position).x ) );
                }
                else
                {
                    setTime(mapToCurrentTime(frameContainer.InverseTransformPoint(data.position).x));
                }
            }
	    }
	
	    public void OnEndDrag(PointerEventData data)
	    {
	        // Debug.Log("END DRAG TIMELINE");
	    }
	
	
	    private void setTimeFromGlobalPositionX(float x)
	    {
            float _x = frameContainer.InverseTransformPoint(new Vector3(x, frameContainer.position.y, frameContainer.position.z) ).x;

	        if (callback != null)
	        {
	            callback(mapToCurrentTime(_x));
	        }
	        else
	        {
	            setTime(mapToCurrentTime(_x));
	        }
	    }
	
}
}