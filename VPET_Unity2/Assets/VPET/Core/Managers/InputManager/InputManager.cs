/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "InputManager.cs"
//! @brief Implementation of the VPET Input Manager, managing all user inupts and mapping.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 19.08.2021


using System;
using UnityEngine;
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
        //[REVIEW]
        //public delegate void ClickAction(InputEventArgs InputArg);
        //public event ClickAction OnClicked;

        //!
        //! The default input event.
        //!
        public event EventHandler<InputEventArgs> inputEvent;

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
            m_inputs.tonioMap.Tap.started += ctx => TapFunction(ctx);
            m_inputs.tonioMap.Tap.performed += ctx => TapFunction(ctx);
            m_inputs.tonioMap.Tap.canceled += ctx => TapFunction(ctx);

        }
        private void TapFunction(InputAction.CallbackContext c)
        {
            //Debug.Log(c);
            InputEventArgs e = new InputEventArgs();

            if (c.performed)
            {
                e.point = m_inputs.tonioMap.ScreenPosition.ReadValue<Vector2>();
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
    }
}
