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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace vpet
{
	public delegate void CallbackFloat( float v );
	
	public class RangeSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
	
	    private Sprite[] sprites;
	
	    private int size = 20;
	
	    private float currentValue = 0f;
	    public float Value
	    {
	        set { currentValue = value;
	            text.text = ((int)(currentValue * precision) / precision).ToString();
	            updateSprites();
	        }
	    }
	    public float velocity = 0f;
	
	    private float sensitivity = 1f;
	    public float Sensitivity
	    {
	        set { sensitivity = value; }
	    }
	
	    public  int numDigits = 4;
	    public int numDecimals = 2;
	
	    private Image[] digits;
	
	    private Image[] decimals;
	
	    private bool isActive = false;
	    public bool IsActive
	    {
	        get { return isActive;  }
	    }
	
	    private CallbackFloat callback;
	    public CallbackFloat Callback
	    {
	        set { callback = value; }
	    }
	
	    private float minValue = 0.01f;
	    public float MinValue
	    {
	        set { minValue = value;  }
	    }
	
	    private float maxValue = float.MaxValue;
	    public float MaxValue
	    {
	        set { maxValue = value; }
	    }
	    
	
	    private Text text;
	
	    private float precision = 100;
	
	
	    void Awake()
	    {
	        sprites = Resources.LoadAll<Sprite>("VPET/Images/numbers-sprite-100");
	
	        // get text component
	        if (this.transform.FindChild("Text") != null ) text = this.transform.FindChild("Text").GetComponent<Text>();
	        if (text == null) Debug.LogError(string.Format("{0}: Cant get Component: Text.", this.GetType()));
	
	
	        // create image objects
	        digits = new Image[numDigits];
	        for (int i = numDigits - 1; i >= 0; --i)
	        {
	            GameObject obj = new GameObject("int_"+i.ToString());
	            obj.transform.parent = this.transform;
	            obj.transform.localScale = Vector3.one;
	            obj.transform.localPosition = new Vector3(-size * (i + 1), 0, 0);
	            RectTransform rectTrans = obj.AddComponent<RectTransform>();
	            rectTrans.sizeDelta = new Vector2(size, size);
	            digits[i] = obj.gameObject.AddComponent<Image>();
	            digits[i].sprite = sprites[0];
	        }
	        decimals = new Image[numDecimals];
	        for (int i = 0; i <numDecimals; i++)
	        {
	            GameObject obj = new GameObject("dec_" + i.ToString());
	            obj.transform.parent = this.transform;
	            obj.transform.localScale = Vector3.one;
	            obj.transform.localPosition = new Vector3(size * (i+1), 0, 0);
	            RectTransform rectTrans =  obj.AddComponent<RectTransform>();
	            rectTrans.sizeDelta = new Vector2(size, size);
	            decimals[i] = obj.gameObject.AddComponent<Image>();
	            decimals[i].sprite = sprites[0];
	        }
	
	    }
		
		// Update is called once per frame
		void Update ()
	    {
		    if ( velocity != 0 )
	        {
	
	            currentValue += velocity * Time.deltaTime * sensitivity ;
	
	
	            currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
	
	            if ( Mathf.Abs(velocity) < 0.01f ) velocity = 0f;
	
	            text.text = ((int)(currentValue*precision)/precision).ToString();
	
	
	            updateSprites();
	
	            if (callback != null) callback(currentValue);
	
	
	        }
	    }
	
	    private void updateSprites()
	    {
	        float intValue = Mathf.Floor(currentValue);
	        float frac = (currentValue - intValue) * Mathf.Pow(10, numDecimals);
	
	        for (int i = numDigits - 1; i >= 0; --i)
	        {
	            int modNumber = (int)(intValue / Mathf.Pow(10, i)) % 10;
	            digits[i].sprite = sprites[modNumber];
	        }
	
	        for (int i = 0; i < numDecimals; i++)
	        {
	            int modNumber = (int)(frac / Mathf.Pow(10, numDecimals-1-i)) % 10;
	            decimals[i].sprite = sprites[modNumber];
	        }
	
	    }
	
	
	    // DRAG
	    public void OnBeginDrag(PointerEventData eventData)
	    {
	        // Debug.Log("BEGIN DRAG");
	        isActive = true;
	
	    }
	
	    public void OnDrag(PointerEventData data)
	    {
	        // Debug.Log("ON DRAG delta: " + data.delta.ToString());
	        velocity = data.position.x- data.pressPosition.x;
	    }
	
	    public void OnEndDrag(PointerEventData eventData)
	    {
	        // Debug.Log("END DRAG");
	        velocity = 0;
	        isActive = false;
	    }
	
	
}
}