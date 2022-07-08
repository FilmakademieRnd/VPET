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
//! @date 24.03.2022


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
        // [REVIEW]
        // Doesn't seem to be in use
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
        //! Class defining pinch input event arguments.
        //!
        public class PinchEventArgs : EventArgs
        {
            public float distance;
        }
        //!
        //! Class defining drag input event arguments.
        //!
        public class DragEventArgs : EventArgs
        {
            public Vector2 delta;
        }

        //!
        //! The default input event.
        //!
        public event EventHandler<InputEventArgs> objectSelectionEvent;

        //!
        //! Press start event, i.e. the begin of a click.
        //!
        public event EventHandler<InputEventArgs> inputPressStart;
        //!
        //! Press start event, i.e. the begin of a click.
        //!
        public event EventHandler<InputEventArgs> inputPressPerformed;
        //!
        //! Press end event, i.e. the end of a click.
        //!
        public event EventHandler<InputEventArgs> inputPressEnd;
        //!
        //! Press move event, i.e. the moving of the cursor/finger.
        //!
        public event EventHandler<InputEventArgs> inputMove;

        //!
        //! The two finger pinch input event.
        //!
        public event EventHandler<PinchEventArgs> pinchEvent;
        //!
        //! The two finger drag input event.
        //!
        public event EventHandler<DragEventArgs> twoDragEvent;
        //!
        //! The three finger drag input event.
        //!
        public event EventHandler<DragEventArgs> threeDragEvent;

        // Candidate event to stop the UI drag operations (snap select)
        public event EventHandler<InputEventArgs> suspendDrag;

        //!
        //! Enumeration describing possible touch input gestures.
        //!
        private enum InputTouchType
        {
            ONE,
            TWO,
            THREE,
            NONE
        }
        //!
        //! The touch input gesture type.
        //!
        private InputTouchType m_touchType;
        //!
        //! Flag to determine if a touch drag gesture is being performed.
        //!
        private bool m_isTouchDrag;
        //!
        //! Flag to specify type of gesture. 
        //!
        private bool m_isPinch;
        //!
        //! Simple latch for buffering operations. 
        //!
        private bool m_doOnce;
        //!
        //! Buffer Vector2 for input position comparison.
        //!
        private Vector2 m_posBuffer;
        //!
        //! Buffer float for input distance comparison.
        //!
        private float m_distBuffer;
        //!
        //! Buffers the main cameras initial rotation.
        //!
        private Quaternion m_cameraMainOffset;
        //!
        //! Buffers the sensors initial attitude.
        //!
        private Quaternion m_invAttitudeSensorOffset;
        //!
        //! A reference to the attitude button.
        //!
        private MenuButton m_attitudeButton;
        //!
        //! Enum defining the automatic camera control state.
        //!
        public enum CameraControl
        {
            NONE,
            ATTITUDE,
            AR
        }
        //!
        //! Flag defining if the camera is controlled by the attitide sensor.
        //!
        private CameraControl m_cameraControl = CameraControl.NONE;
        public CameraControl cameraControl
        {
            get => m_cameraControl;
            set => m_cameraControl = value;
        }
        //!
        //! The generated Unity input class defining all available user inputs.
        //!
        private Inputs m_inputs;

        //!
        //! Constructor initializing member variables.
        //!
        public InputManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            // Enable input
            m_inputs = new Inputs();
            m_inputs.VPETMap.Enable();

            // Binding of the click event
            m_inputs.VPETMap.Tap.performed += ctx => TapFunction(ctx);

            // Dedicated bindings for monitoring touch and drag interactions
            m_inputs.VPETMap.Click.started += ctx => PressStart(ctx);
            m_inputs.VPETMap.Click.performed += ctx => PressPerformed(ctx);
            m_inputs.VPETMap.Click.canceled += ctx => PressEnd(ctx);

            // Keep track of cursor/touch move
            m_inputs.VPETMap.Position.performed += ctx => MovePoint(ctx);

            // Enhaced touch interface API
            EnhancedTouchSupport.Enable();

            // Subscription to new touch or lift gestures
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;

            // Subscription to finger movement 
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += FingerMove;

            // Additional subscriptions for specific input gestures
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += TwoFingerMove;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += ThreeFingerMove;
        }

        //! 
        //! Function called when Unity initializes the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            base.Init(sender, e);
            // Global variables initialization
            m_isPinch = false;
            m_doOnce = true;
            m_touchType = InputTouchType.NONE;
        }

        //! 
        //! Virtual function called when Unity calls it's Start function.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Start(object sender, EventArgs e)
        {
            base.Start(sender, e);
            if(m_cameraControl == CameraControl.NONE)
                enableAttitudeSensor();
        }

        //! 
        //! Function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            m_inputs.VPETMap.Tap.performed -= ctx => TapFunction(ctx);

            m_inputs.VPETMap.Click.performed -= ctx => PressPerformed(ctx);
            m_inputs.VPETMap.Click.canceled -= ctx => PressEnd(ctx);

            m_inputs.VPETMap.Position.performed -= ctx => MovePoint(ctx);

            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= FingerUp;

            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= FingerMove;

            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= TwoFingerMove;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= ThreeFingerMove;
        }

        public void enableAttitudeSensor()
        {
            // Enable attitude sensor and bind it to the camera update
            if (m_cameraControl == CameraControl.NONE)
            {
                if (AttitudeSensor.current != null)
                {
                    InputSystem.EnableDevice(AttitudeSensor.current);

                    m_attitudeButton = new MenuButton("Attitude", useAttitude);
                    m_attitudeButton.setIcon("Images/button_frame_BG");
                    core.getManager<UIManager>().addButton(m_attitudeButton);
                }
                else
                    Helpers.Log("No attitude sensor found, feature will not be available.", Helpers.logMsgType.WARNING);
            }
        }

        public void disableAttitudeSensor()
        {
            // Enable attitude sensor and bind it to the camera update

            if (AttitudeSensor.current != null)
            {
                InputSystem.DisableDevice(AttitudeSensor.current);
                core.getManager<UIManager>().removeButton(m_attitudeButton);
                m_cameraControl = CameraControl.NONE;
            }
            else
                Helpers.Log("No attitude sensor found, feature will not be available.", Helpers.logMsgType.WARNING);
        }

        //!
        //! Single tap/touch operation.
        //!
        private void TapFunction(InputAction.CallbackContext c)
        {
            
            InputEventArgs e = new InputEventArgs();

            if (c.performed)
            {
                e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
                if (!TappedUI(e.point) && !Tapped3DUI(e.point))
                {
                    objectSelectionEvent?.Invoke(this, e);
                }
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

        //!
        //! Input move function, for monitoring the moving of the cursor/finger.
        //!
        private void MovePoint(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            inputMove?.Invoke(this, e);

            if (!m_isTouchDrag && m_touchType == InputTouchType.ONE)
                m_isTouchDrag = true;
        }

        //!
        //! Input press start function, for monitoring the start of touch/click interactions.
        //!
        private void PressStart(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            if (!TappedUI(e.point))
                inputPressStart?.Invoke(this, e);
        }

        //!
        //! Input press start function, for monitoring the start of touch/click interactions.
        //!
        private void PressPerformed(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            if (!TappedUI(e.point))
                inputPressPerformed?.Invoke(this, e);
        }

        //!
        //! Input press end function, for monitoring the end of touch/click interactions.
        //!
        private void PressEnd(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();

            inputPressEnd?.Invoke(this, e);

            // Reset monitor variables
            m_touchType = InputTouchType.NONE;
            m_isTouchDrag = false;
        }

        //!
        //! Function to handle  any new finger touching the screen.
        //!
        private void FingerDown(Finger fgr)
        {
            // If a specific gesture is in progress, do not accept new input
            if (m_isTouchDrag)
                return;

            // Reset monitor variables
            m_touchType = InputTouchType.NONE;

            // Poll touch count 
            int touchCount = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;

            // Single touch
            if (touchCount == 1)
                m_touchType = InputTouchType.ONE;
            // Double touch
            if (touchCount == 2)
                m_touchType = InputTouchType.TWO;
            // Triple touch - ignored in case of a two finger operation in progress
            if (touchCount == 3)
                m_touchType = InputTouchType.THREE;
        }

        //!
        //! Function to handle any finger being lifted from the screen.
        //!
        private void FingerUp(Finger fgr)
        {
            // Suspend the touch input
            m_touchType = InputTouchType.NONE;

            // Also the moving
            m_isTouchDrag = false;
        }

        //!
        //! Function to handle initial finger movement on the screen.
        //!
        private void FingerMove(Finger fgr)
        {
            // If a specific gesture is in progress, do not accept new input
            if (m_isTouchDrag)
                return;

            // Else (i.e., touch was made, but not moved)
            // and if operating with multi-finger input,
            // force the suspension of active selection.
            if (m_touchType == InputTouchType.TWO || m_touchType == InputTouchType.THREE)
            {
                LockUIOperation();
                //ClearClickInput();
            }
        }

        //!
        //! Function to handle specifically two-finger gestures.
        //!
        private void TwoFingerMove(Finger fgr)
        {
            if (m_touchType != InputTouchType.TWO)
                return;

            // Register the gesture
            m_isTouchDrag = true;

            // Monitor touches
            var tcs = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            // Are they moving in the same direction?
            // Then trigger two finger drag
            if (Vector2.Dot(tcs[0].delta, tcs[1].delta) > .2f)
            {
                // Reset control variables
                if (m_isPinch)
                    m_doOnce = true;
                m_isPinch = false;

                // Grab the average position
                Vector2 pos = .5f * (tcs[0].screenPosition + tcs[1].screenPosition);

                // Store it once
                if (m_doOnce)
                {
                    m_posBuffer = pos;
                    m_doOnce = false;
                }

                // Invoke event
                DragEventArgs e = new();
                e.delta = pos - m_posBuffer;
                twoDragEvent?.Invoke(this, e);

                // Update buffer
                m_posBuffer = pos;
            }
            // Else trigger two finger pinch
            else
            {
                // Reset control variables
                if (!m_isPinch)
                    m_doOnce = true;
                m_isPinch = true;

                // Grab the distance
                float dist = Vector2.Distance(tcs[0].screenPosition, tcs[1].screenPosition);

                // Store it once
                if (m_doOnce)
                {
                    m_distBuffer = dist;
                    m_doOnce = false;
                }

                // Invoke event
                PinchEventArgs e = new();
                e.distance = dist - m_distBuffer;
                pinchEvent?.Invoke(this, e);

                // Update buffer
                m_distBuffer = dist;
            }
        }

        //!
        //! Function to handle specifically three-finger gestures.
        //!
        private void ThreeFingerMove(Finger fgr)
        {
            if (m_touchType != InputTouchType.THREE)
                return;

            // Register the gesture
            m_isTouchDrag = true;

            // Monitor touches
            var tcs = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            // Grab the average position
            Vector2 pos = 1f / 3f * (tcs[0].screenPosition + tcs[1].screenPosition + tcs[2].screenPosition);

            // Store it once
            if (m_doOnce)
            {
                m_posBuffer = pos;
                m_doOnce = false;
            }

            // Invoke event
            DragEventArgs e = new();
            e.delta = pos - m_posBuffer;
            threeDragEvent?.Invoke(this, e);

            // Update buffer
            m_posBuffer = pos;
        }

        //!
        //! Helper function to stop UI operations while moving camera
        //!
        private void LockUIOperation()
        {
            // Clear monitor variables
            m_doOnce = true;

            // Invoke end of press event
            inputPressEnd?.Invoke(this, null);
            suspendDrag?.Invoke(this, null);
        }

        //!
        //! Helper function to reset existing operations of an input click (e.g. object selection)
        //!
        private void ClearClickInput()
        {
            // Clear monitor variables
            m_doOnce = true;

            // Invoke end of press event
            // [REVIEW]
            // Doesn't seem to be needed - are we overlooking something if leaving it out? 
            //inputPressEnd?.Invoke(this, null);

            // Force an empty selection
            // [REVIEW]
            // Is this too much of a hack?
            InputEventArgs e = new();
            e.point = new(-5, -5);
            objectSelectionEvent?.Invoke(this, e);
        }

        //!
        //! returns true if tap was over any UI element (it goes over all raycaster in the scene - ideally that would be GraphicRaycaster from the 2D UI)
        //!
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
        //!
        //! @param pos position of the tap
        //!
        private bool Tapped3DUI(Vector2 pos, int layerMask = 1 << 5)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out _, Mathf.Infinity, layerMask))
                return true;

            return false;
        }

        //!
        //! Function that overwrites the main cameras rotation by the attitude sensors values.
        //! Connected to VPETMap.Look which triggers when the input system fires a attitude sensor performed event.  
        //!
        private void updateCameraRotation(InputAction.CallbackContext ctx)
        {
            Transform cam = Camera.main.transform;
            cam.localRotation = ctx.ReadValue<Quaternion>() * Quaternion.Euler(0f, 0f, 180f);
            cam.rotation = m_cameraMainOffset * m_invAttitudeSensorOffset * cam.rotation;
        }

        //!
        //! Function that stores the current main camera and attitude sensors rotation offset.
        //!
        public void setCameraAttitudeOffsets()
        {
            m_cameraMainOffset = Camera.main.transform.rotation;
            m_invAttitudeSensorOffset = Quaternion.Inverse(AttitudeSensor.current.attitude.ReadValue() * Quaternion.Euler(0f, 0f, 180f));
        }

        //!
        //! Function that toggles the main camera rotation overwrite by attitude sensor.
        //!
        private void useAttitude()
        {
            if (m_cameraControl == CameraControl.ATTITUDE)
            {
                m_inputs.VPETMap.Look.performed -= updateCameraRotation;
                m_cameraControl = CameraControl.NONE;
            }
            else if (m_cameraControl == CameraControl.NONE)
            {
                setCameraAttitudeOffsets();
                m_inputs.VPETMap.Look.performed += updateCameraRotation;
                m_cameraControl = CameraControl.ATTITUDE;
            }
        }
    }

}
