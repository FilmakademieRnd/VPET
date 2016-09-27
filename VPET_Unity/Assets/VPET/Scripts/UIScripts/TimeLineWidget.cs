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
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace vpet
{
	public class TimeLineWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
	
	    // TODO: use events
	    private CallbackFloat callback;
	
	    public CallbackFloat Callback
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
	        set { startTime = value;
	            m_a1 = startTime * 25;
	            startFrameDisplay.text = Mathf.RoundToInt(startTime * 25).ToString();
	        }
	    }
	
	    //!
	    //! visible end time of the timeline
	    //!
	    private float endTime = 5;
	    public float EndTime
	    {
	        set {
	            endTime = value;
	            m_b1 = endTime * 25;
	            endFrameDisplay.text = Mathf.RoundToInt(endTime * 25).ToString();
	        }
	    }
	
	
	
	    //!
	    //! is the currently edited animation looped
	    //!
	    public bool isLooping = false;
	
	
		// mapping start end values
	    float m_a1 = 0; // start time in frames
	    float m_b1 = 1; // end time in frames
	    float m_a2 = 0; // timeline start position global
		float m_b2 = 1; // timeline end position global
		float m_a3 = 0; // timeline start position local
		float m_b3 = 1; // timeline end position local
	
	
	
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
	        startFrameDisplay = this.transform.FindChild("StartFrameNumber").GetComponent<Text>();
	        if (startFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component: StartFrameNumber.", this.GetType()));
	
	        // get text component
	        endFrameDisplay = this.transform.FindChild("EndFrameNumber").GetComponent<Text>();
	        if (endFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component: EndFrameNumber.", this.GetType()));
	
	        // get text component
	        currentFrameDisplay = redLine.GetChild(0).GetComponent<Text>();
	        if (currentFrameDisplay == null) Debug.LogError(string.Format("{0}: Cant Find Text component below RedLine.", this.GetType()));

	    }
	
	    void Start()
	    {

			initMappingValues();

	        /*
	        Keyframe[] keys = new Keyframe[3];
	        keys[0] = new Keyframe(-10, -1);
	        keys[1] = new Keyframe(2, 0);
	        keys[2] = new Keyframe(5, 1);
	        AnimationCurve testCurve = new AnimationCurve(keys);
	        updateFrames(testCurve);
	        */
	
	        /*
	        addFrame(-15f);
	        addFrame(-10f);
	        addFrame(0f);
	        addFrame(5f);
	        */
	
	        setTime(2f);
	    }
	
		public void initMappingValues()
		{
			m_a1 = startTime * 25;
			m_b1 = endTime * 25;
			m_a2 = ( frameContainer.position.x- frameContainer.sizeDelta.x * VPETSettings.Instance.canvasScaleFactor /2 ) ; 
			m_b2 = m_a2 + frameContainer.sizeDelta.x * VPETSettings.Instance.canvasScaleFactor; 
			m_a3 = -frameContainer.sizeDelta.x / 2;
			m_b3 = frameContainer.sizeDelta.x  / 2;

			startFrameDisplay.text = Mathf.RoundToInt( startTime*25 ).ToString();
			endFrameDisplay.text = Mathf.RoundToInt( endTime * 25).ToString();

			print("a1: " + m_a1 + "b1: " + m_b1 + "a2: " + m_a2 + "b2: " + m_b2 + "a3: " + m_a3 + "b3: " + m_b3);
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
	        img.GetComponent<RectTransform>().localPosition = new Vector3( _map(time * 25, startTime*25, endTime*25, -frameContainer.sizeDelta.x/2, frameContainer.sizeDelta.x/2), 0, 0);
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
	        return (x * (m_b3 - m_a3) - m_a1 * m_b3 + m_a3 * m_b1) / (m_b1 - m_a1);
	    }
	
	    private float mapToCurrentTime( float x )
	    {
	        return (x * (m_b1 - m_a1) - m_a2 * m_b1 + m_a1 * m_b2) / (m_b2 - m_a2);
	    }
	
	    //!
	    //! set the current time (of the animation) in the timeline
	    //! @param      time        current time at the red line of the timeline
	    //!
	    public void setTime(float time)
	    {
	        currentTime = time;
	        // redLine.localPosition = new Vector3(_map( Mathf.Clamp(time, startTime, endTime)  * 25, startTime * 25, endTime * 25, -frameContainer.sizeDelta.x / 2, frameContainer.sizeDelta.x / 2), 0, 0);
			redLine.localPosition = new Vector3( mapToTimelinePosition(currentTime*25), 0, 0);
	        currentFrameDisplay.text = Mathf.RoundToInt(time * 25) + " f";
	    }
	
	    //!
	    //! updates the displayed frames according to the given animation curve
	    //! will add/remove frames if neccessary
	    //! @param      curve       animation curve for which to display the timeline
	    //!
	    public void updateFrames(AnimationCurve curve, int layer)
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
	                addFrame(curve.keys[i].time, layer);
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
	
	
	    // DRAG
	    public void OnBeginDrag(PointerEventData data)
	    {
	        //Debug.Log("BEGIN DRAG");
	    }
	
	    public void OnDrag(PointerEventData data)
	    {
	        // setTime( mapToCurrentTime(data.position.x - Screen.width / 2) / 25f );
	        if (callback != null)
	        {
				callback( Mathf.Clamp( mapToCurrentTime(data.position.x) / 25f, startTime, endTime ) );
	        }
	        else
	        {
				setTime( Mathf.Clamp( mapToCurrentTime(data.position.x) / 25f, startTime, endTime ) );
	        }
	    }
	
	    public void OnEndDrag(PointerEventData data)
	    {
	        //Debug.Log("END DRAG");
	    }
	
	
	    private void setTimeFromGlobalPositionX(float x)
	    {
	        if (callback != null)
	        {
	            callback(mapToCurrentTime(x) / 25f);
	        }
	        else
	        {
	            setTime(mapToCurrentTime(x) / 25f);
	        }
	    }
	
}
}