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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace vpet
{
	public class KeyFrame : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
	
	    private RectTransform rectTransform;
	
	    private Vector3 lastPosition = Vector3.zero;
	
	    public float currentTime = 0f;
	
	    // TODO: use events
	    private CallbackFloat callback;
	
	    public CallbackFloat Callback
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
            if (layerId >= 0)
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