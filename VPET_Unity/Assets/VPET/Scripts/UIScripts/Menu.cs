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
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace vpet
{
	public enum layouts { SPLASH, DEFAULT, PERSPECTIVES, MODES, FOV, EDIT, SCOUT, TRANSFORM, OBJECT, LIGHT, LIGHTSPOT, LIGHTDIR, LIGHTPOINT, ANIMATION, TRANSLATION }

    public class MenuBoolEvent : UnityEvent<bool> { }

    //!
    //! This class represents a general menu with variable amount of buttons (maximum 10).
    //! To create a menu you need to inherit from this class
    //!
    abstract public class Menu : MonoBehaviour // , System.Collections.IEnumerable
	{
		
	    //! 
	    //! Array containing references to all Buttons in this Menu.
	    //!
	    protected  List<GameObject> buttons = new List<GameObject>();
	    //! 
	    //! Array containing menu layouts used to switch bewtween different button sets
	    //!
		protected Dictionary<layouts, List<GameObject> > layoutDict = new Dictionary<layouts, List<GameObject> >();
	    //! 
	    //! This is the current active layout
	    //!
	    public layouts currentLayout = layouts.DEFAULT;
        //! 
        //! Use this to restore previous layou
        //!
        public layouts PreviousLayout = layouts.DEFAULT;
        //!
        //! Reference to the button beeing active and on active position currently.
        //!
        protected GameObject activeButton;
		public GameObject ActiveButton
		{
			set{ activeButton = value; }
		}
	    //!
	    //! Returns the number of buttons visible in the current layout
	    //!
	    public int ActiveButtonCount
	    {
			get { return layoutDict[currentLayout].Count; }
	    }
        //!
        //! is animation currently in progress
        //!
        private bool animate = false;
        //!
        //! speed of the draw animation (roll out of the circular menu) (1 -> immediate, 0 -> never)
        //!
        public float animationSpeed = 8.0f;
        //!
        //! incremention of the current value per update, equals +-animationSpeed
        //!
        private float animationIncrement = 0f;
        //!
        //! current delta of the animation
        //!
        private float currentDelta = 0;
        //!
        //! target delta of the animation
        //!
        private float targetDelta = 0;

		public bool isOpen = false;

		//!
        //! Called when object is created before start
        //!
        void Awake()
	    {
			layoutDict.Add( layouts.DEFAULT, new List<GameObject>() );
	    }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            //animate the appearing of the menu
            if (animate)
            {
                // increase
                currentDelta += animationIncrement * Time.deltaTime;
                // call draw, overwritten by sub class
                animatedDraw();
                // is value reached?
                if ((animationIncrement < 0 && currentDelta < targetDelta) || (animationIncrement > 0 && currentDelta > targetDelta))
                {
                    animate = false;
                    if (targetDelta == 0)
                    {
                        animatedDrawFinishOut();
                    }
                    else
                    {
                        animatedDrawFinishIn();
                    }
                }
            }
        }

		//!
		//! Empty this menu and remove all buttons from scene
		//!
		public void clear()
		{
			for (int i=0; i<buttons.Count; i++ )
			{
				GameObject.Destroy( buttons[i] );
			}
			buttons.Clear();
			layoutDict.Clear();
			currentLayout = layouts.DEFAULT;
			GameObject activeButton = null;
		}


		public void addLayout(layouts layout )
		{
			layoutDict.Add( layout, new List<GameObject>() );
		}

        //!
        //! adds a button to the menu
        //! @param    active    image displayed when button is active
        //! @param    toggle    indicate if this is a toggle button
        //!
		public void addButton(IMenuButton newButton, layouts layout=layouts.DEFAULT )
	    {
			GameObject newButtonObj = ((Button)newButton).gameObject;
			buttons.Add( newButtonObj );
			newButtonObj.transform.SetParent(this.transform, false);
			newButtonObj.SetActive(false);
			addObjectToLayout( newButtonObj, layout );
			newButton.Menu = this;
	    }
			
		public void addButtonToLayout(IMenuButton newButton, layouts layout )
		{
			if ( !layoutDict.ContainsKey(layout) )
			{
				layoutDict.Add( layout, new List<GameObject>() );
			}

			layoutDict[layout].Add( ((Button)newButton).gameObject );
		}


		public void addObjectToLayout(GameObject newObj, layouts layout )
		{
			if ( !layoutDict.ContainsKey(layout) )
			{
				layoutDict.Add( layout, new List<GameObject>() );
			}

			layoutDict[layout].Add(newObj);
		}

        //!
        //! adds a arbitrary gameobject to the menu which will displayed like buttons
        //! @param    obj    object to add
        //!
		public void addObject( GameObject obj,  layouts layout=layouts.DEFAULT )
	    {
	        buttons.Add(obj);
	        obj.SetActive(false);
			addObjectToLayout( obj,  layout);
	    }

        /*
	    //!
	    //! Return generator 
	    //!
	    public System.Collections.IEnumerator GetEnumerator()
	    {
	        foreach (int i in layoutList[currentLayout.ToString()])
	        {
	            yield return buttons[i];
	        }
	    }
	    */


        //!
        //! Generator to access all buttons
        //!
        public System.Collections.IEnumerable Buttons()
	    {
	        for (int i=0; i<buttons.Count; i++ )
	        {
	            yield return buttons[i];
	        }
	    }

        //!
        //! Generator to access active buttons
        //!
        public System.Collections.IEnumerable ButtonsActive()
	    {
			foreach (GameObject mb in layoutDict[currentLayout])
	        {
				yield return mb;
	        }
	    }

        //!
        //! Generator to access inactive buttons
        //!
        public System.Collections.IEnumerable ButtonsInActive()
	    {
	        for ( int i=0; i<buttons.Count; i++ )
	        {
				if ( !layoutDict[currentLayout].Contains<GameObject>(buttons[i]) )
	            {
	                yield return buttons[i];
	            }
	        }
	    }

	    //!
	    //! Change the current button layout. Hide all buttons and set layout. Next call to method show will set buttons defined in new layout to visible.
	    //!
	    public void switchLayout(layouts layout)
	    {	        
	        if (layoutDict.ContainsKey(layout))
	        {
				// print("switchLayout " + layout.ToString());
                PreviousLayout = currentLayout;
                currentLayout = layout;
	
	            if (activeButton)
	            {
	                activeButton.SetActive(false);
	                activeButton = null;
	            }
                	
	            // hide all buttons
				foreach (GameObject g in Buttons())
				{
					g.SetActive(false);
				}	       

			}
	    }
	
	    //!
	    //! Show all buttons of the menu without animation.
	    //!
	    public void show()
	    {
            foreach (GameObject g in ButtonsActive())
            {
                g.SetActive(true);
            }

            if (currentDelta != 1)
            {
                currentDelta = 0;
                targetDelta = 1;
                arrange();
                animationIncrement = animationSpeed; // * Time.deltaTime;
                animate = true;
            }

			isOpen = true;

        }

        //!
        //! Hide. The animatedDraw method needs to be overwritten
        //!
        public void hide()
	    {
            if (currentDelta != 0)
            {
                currentDelta = 1;
                targetDelta = 0;
                animationIncrement = -animationSpeed; // * Time.deltaTime;
                animate = true;
            }
			isOpen = false;
        }
			

	    //!
	    //! animate buttons in this menu set on every update (can be overwritten). by default fade buttons
	    //!
	    protected virtual void animatedDraw()
        {
            // animated transpareny
            foreach (GameObject button in ButtonsActive())
            {
                Color matColor = button.GetComponent<Image>().color;
				matColor.a = currentDelta; // Mathf.Min(button.GetComponent<MenuButton>().maxTransparency, currentDelta);
                button.GetComponent<Image>().color = matColor;

                // fade all text children (eg. animation layer index)
                Text[] textChilds = button.GetComponentsInChildren<Text>();
                foreach(Text text in textChilds)
                {
                    Color textColor = text.color;
                    textColor.a = currentDelta;
                    text.color = textColor;
                }
            }

        }

        //!
        //! called when animation is finish and currentDelta is zero (can be overwritten). by default this will hide buttons 
        //!
        protected virtual void animatedDrawFinishOut()
        {
            foreach (GameObject g in Buttons())
            {
                if (g != activeButton)
                    g.SetActive(false);
            }
        }

        //!
        //! called when animation is finish and currentDelta is greater zero (can be overwritten).
        //!
        protected virtual void animatedDrawFinishIn()
        {
        }

        //! 
        //! move button to active position  (need to be overwritten)
        //! @param      button      currently active button, to be moved
        //!
		public virtual void animateActive(GameObject button) {}
	
	    //!
	    //! move the center of the menu relative to the last position (need to be overwritten)
	    //! @param    pos     xy position offset
	    //!
		public virtual void moveRelative(Vector2 pos) {}
	
	    //!
	    //! move the center of the menu relative to the last position but only on x axis (need to be overwritten)
	    //! @param    posX     x position offset
	    //!
		public virtual void moveRelativeX(float posX) {}
	
	    //! 
	    //! move the center of the menu relative to the last position but only on y axis (need to be overwritten)
	    //! @param    posY     y position offset
	    //!
		public virtual void moveRelativeY(float posY) {}
	
	    //!
	    //! move the center of the menu to the given position (need to be overwritten)
	    //! @param    pos     new xy position of the menu
	    //!
		public virtual void moveAbsolute(Vector2 pos) {}
	
	    //!
	    //! move the center of the menu to the given position only on the x axis (need to be overwritten)
	    //! @param    pos     new x position of the menu
	    //!
		public virtual void moveAbsoluteX(float posX) {}
	
	    //!
	    //! move the center of the menu to the given position only on the y axis (need to be overwritten)
	    //! @param    pos     new y position of the menu
	    //!
		public virtual void moveAbsoluteY(float posY) {}
	
	    //!
	    //! place all available buttons (need to be overwritten)
	    //!
	    abstract protected void arrange();

	    //!
	    //! reset all buttons in this menu
	    //!
	    public void reset()
		{
            for (int i = 0; i < buttons.Count; i++)
	        {
				if ( buttons[i] != activeButton && buttons[i].GetComponent<IMenuButton>() != null )
					buttons[i].GetComponent<IMenuButton>().reset();
            }
	    }
	
	}

}