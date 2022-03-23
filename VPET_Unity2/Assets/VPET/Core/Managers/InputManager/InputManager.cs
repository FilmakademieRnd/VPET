/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "InputManager.cs"
//! @brief Implementation of the VPET Input Manager, managing all user inupts and mapping.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Paulo Scatena
//! @version 0
//! @date 14.02.2022


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace vpet
{
    //!
    //! Class implementing the input manager, managing all user inupts and mapping.
    //!
    public class InputManager : Manager
    {
        //!
        //! Enumeration defining supported input event types.
        //!
        public enum InputEventType
        {
            TAP,
            DRAG,
            STARTED,
            PERFORMED
        }
        //!
        //! Class defining input event arguments.
        //!
        public class InputEventArgs : EventArgs
        {
            public InputEventType type;
            public InputActionPhase phase;
            public Vector2 point;
            public Vector2 delta;
            public double time;
        }

        //!
        //! The default input event.
        //!
        public event EventHandler<InputEventArgs> inputEvent;

        // TRS Development events
        // Touch start
        public event EventHandler<InputEventArgs> InputPressStart;
        //! Touch end
        public event EventHandler<InputEventArgs> InputPressEnd;
        //! Touch move
        public event EventHandler<InputEventArgs> InputMove;


        // Monitor variables - temporary?
        // todo: drags on ui (e.g. spinner) should somehow invoke an event to let there's a drag happening
        bool pressingOne = false;
        bool pressingTwo = false;
        bool pressingThree = false;
        bool movingOne = false;
        bool movingTwo = false;
        bool movingThree = false;

        //!
        //! Class defining pinch input event arguments.
        //!
        public class PinchEventArgs : EventArgs
        {
            public float distance;
        }
        //!
        //! The two finger pinch input event.
        //!
        public event EventHandler<PinchEventArgs> pinchEvent;
        //!
        //! Class defining drag input event arguments.
        //!
        public class DragEventArgs : EventArgs
        {
            public Vector2 delta;
        }
        //!
        //! The two finger drag input event.
        //!
        public event EventHandler<DragEventArgs> twoDragEvent;
        //!
        //! The three finger drag input event.
        //!
        public event EventHandler<DragEventArgs> threeDragEvent;

        // Auxiliary variables
        // todo: replace with more elegant ways
        bool doOnce = true;
        Vector2 posBuffer;
        float distBuffer;
        bool isDrag = true;

        //!
        //! The generated Unity input class defining all available user inputs.
        //!
        private Inputs m_inputs;

        // [REVIEW]
        // please replace, just for testing!
        public ref Inputs touchInputs
        {
            get { return ref m_inputs; }
        }
        //!
        //! Constructor initializing member variables.
        //!
        public InputManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_inputs = new Inputs();
            m_inputs.Enable();

            // bind individual input events
            //m_inputs.Map.TouchPress.started += ctx => InputFunction(ctx);
            //m_inputs.Map.TouchPosition.started += ctx => InputFunction(ctx);

            m_inputs.VPETMap.Click.performed += ctx => TapFunction(ctx);
            //m_inputs.tonioMap.Click.canceled += ctx => TapFunction(ctx);

            // TRS development bindings
            m_inputs.VPETMap.Click.performed += ctx => PressStart(ctx);
            m_inputs.VPETMap.Click.canceled += ctx => PressEnd(ctx);

            // Keep track of cursor/touch move
            m_inputs.VPETMap.Position.performed += ctx => MovePoint(ctx);

            // Touch interface
            EnhancedTouchSupport.Enable();
            // Subscription to new touch or lift gestures
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;
            // Subscription to finger movement 
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += FingerMove;
            // Additional subscriptions for specific input
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += TwoFingerMove;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += ThreeFingerMove;


        }

        ~InputManager()
        {
            m_inputs.VPETMap.Click.performed -= ctx => TapFunction(ctx);
        }

        // Single tap/touch operation
        private void TapFunction(InputAction.CallbackContext c)
        {
            //Helpers.Log("Tapped");
            InputEventArgs e = new InputEventArgs();

            if (c.performed)
            {
                e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
                if (!TappedUI(e.point) && !Tapped3DUI(e.point))
                    inputEvent?.Invoke(this, e);
            }

            // just an exampe, needs different code to discover correct type and values!
            // we need to define VPET actions like tap, hold, drag, etc. and map it to
            // multiple bindings like keyboard, mouse click and touch (see referenced video)
            // please watch https://youtu.be/rMlcwtoui4I

            // at start we should check if we are on object, canvas or UI element
            if (c.started)
            {
                //e.type = InputEventType.STARTED;
                //e.delta = Vector2.zero;
                //e.time = 0f;
            }
        }

        // Monitor single cursor/touch movement
        private void MovePoint(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            InputMove?.Invoke(this, e);

            if (pressingOne)
                movingOne = true;
        }

        // Single press operation - start
        private void PressStart(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            if (!TappedUI(e.point))
                InputPressStart?.Invoke(this, e);
        }

        // Single press operation - end
        private void PressEnd(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            InputPressEnd?.Invoke(this, e);

            // Reset monitor variables
            pressingOne = false;
            movingOne = false;
        }

        // Function to handle any new finger touching the screen
        private void FingerDown(Finger fgr)
        {
            // In case there is a single cursor/touch in progress, do not accept new input
            if (movingOne)
                return;

            // Reset monitor variables
            pressingOne = false;
            pressingTwo = false;
            pressingThree = false;

            // Poll touch count 
            int touchCount = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;

            // Single touch
            pressingOne = touchCount == 1;
            // Double touch
            pressingTwo = touchCount == 2;
            // Triple touch - ignored in case of a two finger operation in progress
            pressingThree = touchCount == 3 && !movingTwo;
        }

        // Function to handle any finger being lifted from the screen
        private void FingerUp(Finger fgr)
        {
            // Suspend the touch input
            pressingTwo = false;
            pressingThree = false;

            // Also the moving
            movingTwo = false;
            movingThree = false;
        }

        // Function to handle initial finger movement on the screen
        private void FingerMove(Finger fgr)
        {
            // If a specific gesture is in progress, do not accept new input
            if (movingOne || movingTwo || movingThree)
                return;

            // Else (i.e., touch was made, but not moved)
            // and if operating with multi-finger input,
            // force the suspension of active selection.
            if (pressingTwo || pressingThree)
            {
                // todo: factor this out of find more elegant solutions
                // Invoke end of press event
                InputPressEnd?.Invoke(this, null);

                // Clear monitor variables
                doOnce = true;
                pressingOne = false;

                // Force an empty selection
                InputEventArgs e = new();
                e.point = new(-5, -5);
                inputEvent?.Invoke(this, e);
            }
        }

        // Function to handle initial specifically two-finger gestures
        private void TwoFingerMove(Finger fgr)
        {
            if (!pressingTwo)
                return;

            // Register the gesture
            movingTwo = true;

            // Monitor touches
            var tcs = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            // Are they moving in the same direction?
            // Then trigger two finger drag
            if (Vector2.Dot(tcs[0].delta, tcs[1].delta) > .2f)
            {
                // Reset control variables
                if (!isDrag)
                    doOnce = true;
                isDrag = true;

                // Grab the average position
                Vector2 pos = .5f * (tcs[0].screenPosition + tcs[1].screenPosition);

                // Store it once
                if(doOnce)
                {
                    posBuffer = pos;
                    doOnce = false;
                }

                // Invoke event
                DragEventArgs e = new();
                e.delta = pos - posBuffer;
                twoDragEvent?.Invoke(this, e);

                // Update buffer
                posBuffer = pos;
            }
            // Else trigger two finger pinch
            else
            {
                // Reset control variables
                if (isDrag)
                    doOnce = true;
                isDrag = false;

                // Grab the distance
                float dist = Vector2.Distance(tcs[0].screenPosition, tcs[1].screenPosition);

                // Store it once
                if (doOnce)
                {
                    distBuffer = dist;
                    doOnce = false;
                }

                // Invoke event
                PinchEventArgs e = new();
                e.distance = dist - distBuffer;
                pinchEvent?.Invoke(this, e);

                // Update buffer
                distBuffer = dist;
            }
        }

        // Function to handle initial specifically three-finger gestures
        private void ThreeFingerMove(Finger fgr)
        {
            if (!pressingThree)
                return;

            // Register the gesture
            movingThree = true;

            // Monitor touches
            var tcs = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            // Grab the average position
            Vector2 pos = 1f / 3f * (tcs[0].screenPosition + tcs[1].screenPosition + tcs[2].screenPosition);

            // Store it once
            if (doOnce)
            {
                posBuffer = pos;
                doOnce = false;
            }

            // Invoke event
            DragEventArgs e = new();
            e.delta = pos - posBuffer;
            threeDragEvent?.Invoke(this, e);

            // Update buffer
            posBuffer = pos;
        }


        //!
        //! returns true if tap was over any UI element (it goes over all raycaster in the scene - ideally that would be GraphicRaycaster from the 2D UI)
        //! @param pos position of the tap
        //!
        private bool TappedUI(Vector2 pos)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = pos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
        }

        //!
        //! returns true if tap was over the 3D manipulator objects (layerMask 5 for UI)
        //! @param pos position of the tap
        //!
        private bool Tapped3DUI(Vector2 pos, int layerMask = 1 << 5)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out _, Mathf.Infinity, layerMask))
                return true;

            return false;
        }
    }

}
