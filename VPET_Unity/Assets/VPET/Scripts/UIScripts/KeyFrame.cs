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
using UnityEngine.UI;

namespace vpet
{
	public class KeyFrame : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
	
	    private RectTransform rectTransform;
	
	    private Vector3 lastPosition = Vector3.zero;
	
	    public float currentTime = 0f;
	
	    private UnityAction<float> callback;
	
	    public UnityAction<float> Callback
	    {
	        set { callback = value; }
	    }
	
	    protected override void Awake()
	    {
	        base.Awake();
	        // get rectTransform component
	        rectTransform = this.transform.GetComponent<RectTransform>();
	        if (rectTransform == null) Debug.LogError(string.Format("{0}: No RectTransform Component attached.", this.GetType()));
	    }
	
	    // DRAG
	    public void OnBeginDrag(PointerEventData data)
	    {
	       // Debug.Log("BEGIN DRAG");
	        lastPosition = rectTransform.position;
	    }
	
	    public void OnDrag(PointerEventData data)
	    {
	        //Debug.Log("DRAG");
	        // rectTransform.position = new Vector3(lastPosition.x + data.position.x - data.pressPosition.x, lastPosition.y, lastPosition.z);
	
	    }
	
	    public void OnEndDrag(PointerEventData data)
	    {
	        // Debug.Log("END DRAG");
	    }
	
	    public override void OnSelect(BaseEventData data)
	    {
	        base.OnSelect(data);
	        if (callback != null)
	        {
	            callback(rectTransform.position.x);
	        }
	    }

        public void SetLayerId(int layerId)
        {
            if (layerId > 0)
            {
                string layerIdString = layerId.ToString();
                Transform layerIdTransform = transform.FindChild("LayerId");
                if (layerIdTransform == null)
                    return;

                Text text = layerIdTransform.GetComponent<Text>();
                if (text != null)
                    text.text = layerIdString;
            }
        }
    }
}