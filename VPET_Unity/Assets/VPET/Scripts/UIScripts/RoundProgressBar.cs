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
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace vpet
{
	public class ProgressEvent : UnityEvent<float> { }

	public class RoundProgressBar: MonoBehaviour
	{

		//!
		//! callback to send current value. If not null this function is called during update.
		//!
		public ProgressEvent OnProgressEvent = new ProgressEvent();

		//!
		//! this callback is called after progress is finish ( current value greate one )
		//!
		public UnityEvent OnFinishEvent = new UnityEvent();

        public Transform AnimatedGroup;
		public Transform LoadingBar;
		public Transform TextIndicator;
		public Transform TextLoading;

		private string text;

        private Text textIndicatorComponent;
        private Text textLoadingComponent;
        private Image lodaingBarComponent;

        bool isFinished = true;


        //!
        //! the progress value
        //!
        private float currentAmount			= 1.0f;
	    //!
	    //! draw animation or not. 
	    //!
		private bool animated				= true;
	    //!
	    //! amount the animation value is increased per second 
	    //!
		public float animationSpeed		= 100f;

	    //!
	    //! Called when object is created before start
	    //!
		void Awake()
		{	
			// create layout
		}
			
		//!
	    //! Use this for initialization
	    //!
		void Start()
		{

            textIndicatorComponent = TextIndicator.GetComponent<Text>(); 
            textLoadingComponent = TextLoading.GetComponent<Text>();
            lodaingBarComponent = LoadingBar.GetComponent<Image>(); 
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
		{
			if (currentAmount < 1)
			{
				if ( animated )
				{
					draw();
				}
                textIndicatorComponent.text = ((int)(currentAmount*100)).ToString() + "%";
                textLoadingComponent.text = text;
                lodaingBarComponent.fillAmount = currentAmount;
            }
            else if ( !isFinished )
            {
                isFinished = true;
                OnFinish();
            }
        }


        //!
        //! Show widget
        //!
        public void Show()
		{
            SetValue(0,"");
			this.gameObject.SetActive(true);
            isFinished = false;
		}

        //!
        //! Hide widget
        //!
        public void Hide()
		{
			this.gameObject.SetActive(false);
		}

		public void SetValue( float v, string msg )
		{
			currentAmount = v;
			text = msg;
        }
		
	    //!
		//! Draws (animated) placeholder and progress
	    //!
		protected virtual void draw()
		{
            // animate something here
            Vector3 rotation =  AnimatedGroup.localEulerAngles;
            rotation.z = (rotation.z - animationSpeed * Time.deltaTime) % 360;
            AnimatedGroup.localEulerAngles = rotation;
        }

        //!
        //! Execute on finish. Will call finish callback function 
        //!
        private void OnFinish()
		{
			currentAmount = 1f;
			OnFinishEvent.Invoke();
			// Make sure its clean for nexts
			OnFinishEvent.RemoveAllListeners();
		}
	
	}

}