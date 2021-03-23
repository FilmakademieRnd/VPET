// -------------------------------------------------------------------------------
// VPET - Virtual Production Editing Tools
// vpet.research.animationsinstitut.de
// https://github.com/FilmakademieRnd/VPET
// 
// Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
// 
// This project has been initiated in the scope of the EU funded project 
// Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
// 
// Post Dreamspace the project has been further developed on behalf of the 
// research and development activities of Animationsinstitut.
// 
// In 2018 some features (Character Animation Interface and USD support) were
// addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
// under grant agreement no 780470, 2018-2020
// 
// VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
// Syncronisation Server. They are licensed under the following terms:
// -------------------------------------------------------------------------------


//! @file "Parameter.cs"
//! @authors JTvisual <jntrottnow@gmail.com>Simon Spielmann <theseim@gmx.de>
//! @version 0
//! @date 2021-03-23 11:43:53
﻿
using System;
using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    //!
    //! Abstract parameter class defining the fundamental functionality and interface
    //!
    public abstract class AbstractParameter
    {
        //!
        //! Event emitted when parameter changed
        //!
        public event EventHandler hasChanged;

        //!
        //! Abstract definition of change function parameters
        //!
        public abstract class changeEventArgs : EventArgs{}

        //!
        //! Wrapper function to emit value changed event
        //!
        protected void callValueChanged()
        {
            if (hasChanged != null)
                hasChanged(this, null);
        }

        //!
        //! Function connecting an external event to change the parameters value
        //! @param   ev     The event to be connected
        //!
        public void connectToChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev += changeValue;
        }

        //!
        //! Function disconnecting an external event from the parameters change function
        //! @param   ev     The event to be disconnected
        //!
        public void disconnectChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev -= changeValue;
        }

        //!
        //! Abstract definition of the function called to change a parameters value
        //! @param   sender     Object calling the change function
        //! @param   a          Values to be passed to the change function
        //!
        protected abstract void changeValue(object sender, changeEventArgs a);
    }

    //!
    //! Specific implementation for a float value
    //!
    public class FloatParameter : AbstractParameter
    {
        //!
        //! parameters float value
        //!
        private float value;

        //!
        //! definition of change function parameters
        //!
        public class floatEventArgs : changeEventArgs
        {
            public float value;
        }

        //!
        //! definition of the function called to change a float parameter value
        //! @param   sender     Object calling the change function
        //! @param   a          float value to be passed to the change function
        //!
        protected override void changeValue(object sender, changeEventArgs a)
        {
            value = ((floatEventArgs)a).value;
            callValueChanged();
        }
    }


    /*
    public class Parameter<T>
    {
        private string name;
        private T value;
        public event EventHandler hasChanged;
        public class changeEventArgs : EventArgs
        {
            public T value;
        }

        public void connectToChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev += changeValue;
        }

        public void removeFromChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev -= changeValue;
        }

        private void changeValue(object sender, changeEventArgs a)
        {
            value = a.value;
            if (hasChanged != null)
                hasChanged(this, null);
        }
    }

    */
}
