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

        //!
        //! Class defining pinch input event arguments.
        //!
        public class PinchEventArgs : EventArgs
        {
            public float distance;
        }
        //!
        //! The pinch input event.
        //!
        public event EventHandler<PinchEventArgs> pinchEvent;

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

            // inefficient?
            m_inputs.VPETMap.Position.performed += ctx => MovePoint(ctx);

        }

        ~InputManager()
        {
            m_inputs.VPETMap.Click.performed -= ctx => TapFunction(ctx);
        }

        private void TapFunction(InputAction.CallbackContext c)
        {
            Helpers.Log("Tapped");
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

        private void MovePoint(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            InputMove?.Invoke(this, e);
        }

        private void PressStart(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            if (!TappedUI(e.point))
                InputPressStart?.Invoke(this, e);
        }

        private void PressEnd(InputAction.CallbackContext c)
        {
            InputEventArgs e = new InputEventArgs();
            e.point = m_inputs.VPETMap.Position.ReadValue<Vector2>();
            InputPressEnd?.Invoke(this, e);
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
