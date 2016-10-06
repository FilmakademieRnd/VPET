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
	    //! current animation value, will increase by animationSpeed and clamped betweem 0 and 1 by default
	    //!
		private float animation				= 0f;
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
	    //! map from [a1,b1] to [0,1], note this will not clamp values below a1 or greater b1
	    //! @param    x    value to map
	    //! @param    a1    intervall start
	    //! @param    b1    intervall end
	    //! @return   mapped value betweem 0 and 1
	    //!
		public float mapTo01( float x, float a1, float b1 )
		{
			return ( x - a1 ) / (b1-a1);
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