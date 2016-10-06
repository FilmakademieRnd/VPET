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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

public class SplashWidget : MonoBehaviour
{

    //!
    //! entry point to listen for finish event
    //!
    public UnityEvent OnFinishEvent = new UnityEvent();
    //!
    //! fading time in. dont set this to zero
    //!
    private float fadeTimeIn = 0.2f;
    //!
    //! fading time out. dont set this to zero
    //!
    private float fadeTimeOut = 0.5f;
    //!
    //! time between fade in and fade out
    //!
    private float duration = 1;
    //!
    //! time before fade in start
    //!
    private float delay = 1f;
    //!
    //! is animation currently in progress
    //!
    private bool animate = false;
    //!
    //! animation increment per update. initialized with time delta when calling Show
    //!
    private float animationIncrement = 0f;

    //!
    //! current delta of the animation
    //!
    private float currentDelta = 0;
    public float CurrentDelta
    {
        set { currentDelta = value;
                setAlphaFromDelta();
                }
    }
    //!
    //! target delta of the animation
    //!
    private float targetDelta = 0;
    //!
    //! reference to foreground image
    //!
    private Image foreground = null;
    //!
    //! reference to background image
    //!
    private Image background = null;

    //!
    //! Called when object is created before start
    //!
    void Awake()
    {

        foreground = transform.FindChild("Foreground").GetComponent<Image>();

        background = transform.FindChild("Background").GetComponent<Image>();
    }

    //!
    //! Use this for initialization
    //!
    void Start()
    {
        Show();
    }

    //!
    //! Update is called once per frame
    //!
    void Update()
    {

        if (animate)
        {
            currentDelta += animationIncrement;

            setAlphaFromDelta();

            if ( (animationIncrement>0 && currentDelta>=targetDelta) || (animationIncrement<0 && currentDelta <= targetDelta)) 
            {
                currentDelta = targetDelta;
                animate = false;

                if (currentDelta == 0)
                {
                    gameObject.SetActive(false);
                    OnFinish();
                }
                else
                {
                    // fade out when duration is set
                    if (duration > 0)
                        Hide();
                }
            }
        }

    }


    //!
    //! set alpha to foreground and background image
    //!
    private void setAlphaFromDelta()
    {
        Color bgColor = background.color;
        bgColor.a = currentDelta;
        background.color = bgColor;

        Color fgColor = foreground.color;
        fgColor.a = currentDelta;
        foreground.color = fgColor;
    }

    //!
    //! Show widget
    //!
    public void Show()
    {
        CurrentDelta = 0f;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine( WaitAndStart(1f));
    }

    //!
    //! Hide widget
    //!
    public void Hide()
    {
        Color bgColor = background.color;
        CurrentDelta = bgColor.a;
        StopAllCoroutines();
        StartCoroutine(WaitAndFinish(0f));
    }


    IEnumerator WaitAndStart( float targetValue )
    {
        yield return new WaitForSeconds(delay);
        animationIncrement = 1f/fadeTimeIn * Time.deltaTime;
        if (targetValue < currentDelta) { animationIncrement *= -1;  }
        targetDelta = targetValue;
        animate = true;
    }


    IEnumerator WaitAndFinish( float targetValue)
    {
        yield return new WaitForSeconds(duration);
        animationIncrement = 1f/fadeTimeOut * Time.deltaTime;
        if (targetValue < currentDelta) { animationIncrement *= -1; }
        targetDelta = targetValue;
        animate = true;
    }

    private void OnFinish()
    {
        OnFinishEvent.Invoke();
        OnFinishEvent.RemoveAllListeners();
    }

}
