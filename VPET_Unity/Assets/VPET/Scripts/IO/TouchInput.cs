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
//! script receiving touch inputs and interpreting it to gestures & states
//!
namespace vpet
{
	public class TouchInput : MonoBehaviour
	{
	
	    //!
	    //! cached reference to human input controller
	    //!
		private InputAdapter inputAdapter;
		public InputAdapter InputAdapter
		{
			set { inputAdapter = value; }
		}
	
	
	    //!
	    //! is single pointer down & moving
	    //!
	    private bool singlePointerDrag = false;
	    //!
	    //! are two pointer down & moving
	    //!
	    private bool twoPointerDrag = false;
	    //!
	    //! are three pointer down & moving
	    //!
	    private bool threePointerDrag = false;
	
	    //!
	    //! variable to pause interactions during direct switch between touchCount
	    //! e.g. to avoud touchCount = 3 -> touchCount = 2 but allow touchCount = 3 -> touchCount = 0 -> touchCount = 2
	    //!
	    private bool pause = false;
	
	    //!
	    //! Use this for initialization
	    //!
	    void Start()
	    {
	        inputAdapter = GameObject.Find("InputAdapter").GetComponent<InputAdapter>();	
	    }
	
	    //!
	    //! Update is called once per frame
	    //!
	    void Update()
	    {
			if (!pause)
	        {
				if (Input.touchCount == 1)
	            {
	                //single touch gesture
	                if (Input.GetTouch(0).phase == TouchPhase.Began)
	                {
	                    //finger down
						inputAdapter.singlePointerStarted(Input.mousePosition);
	                    singlePointerDrag = true;
	                }
	                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
	                {
	                    //finger up
						inputAdapter.singlePointerEnded(Input.mousePosition);
	                    singlePointerDrag = false;
	                    pause = true;
	                }
	                else if (singlePointerDrag)
	                {
	                    //pointer down and moving
						inputAdapter.singlePointerDrag(Input.mousePosition);
	                }
	            }
	            else if (Input.touchCount == 2)
	            {
	                //2 pointer touch gesture
	                if (Input.GetTouch(1).phase == TouchPhase.Began)
	                {
	                    //finger down
	                    if (singlePointerDrag)
	                    {
							inputAdapter.singlePointerEnded(Input.mousePosition);
	                        singlePointerDrag = false;
	                    }
						inputAdapter.twoPointerStarted(Input.mousePosition);
	                    twoPointerDrag = true;
	                }
	                else if ((Input.GetTouch(0).phase == TouchPhase.Ended ||
	                            Input.GetTouch(1).phase == TouchPhase.Ended))
	                {
	                    //finger up
						inputAdapter.twoPointerEnded(Input.mousePosition);
	                    twoPointerDrag = false;
	                    pause = true;
	                }
	                else if (twoPointerDrag)
	                {
                        //pointer down and moving
                        Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
                        Vector2 touchOnePrevPos = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
	                    float touchDeltaMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;
	                    float deltaMagnitudeScale = touchDeltaMag/prevTouchDeltaMag;

                        // TODO: add pinchtToZoom at input adapter if needed 
                        //inputAdapter.pinchToZoom(deltaMagnitudeScale);
	
						inputAdapter.twoPointerDrag(Input.mousePosition);
	
	                }
	            }
	            else if (Input.touchCount == 3)
	            {
	                //3 pointer touch gesture
	                if (Input.GetTouch(2).phase == TouchPhase.Began)
	                {
	                    //finger down
						inputAdapter.threePointerStarted(Input.mousePosition);
	                    threePointerDrag = true;
	                }
	                else if ((Input.GetTouch(0).phase == TouchPhase.Ended ||
	                            Input.GetTouch(1).phase == TouchPhase.Ended ||
	                            Input.GetTouch(2).phase == TouchPhase.Ended))
	                {
	                    //finger up
						inputAdapter.threePointerEnded(Input.mousePosition);
	                    threePointerDrag = false;
	                    pause = true;
	                }
	                else if (threePointerDrag)
	                {
	                    //pointer down and moving
						inputAdapter.threePointerDrag(Input.mousePosition);
	                }
	            }
	        }
	        else if (Input.touchCount == 0 && pause)
	        {
	            pause = false;
	        }
	    }
}}