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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "Spinner.cs"
//! @brief base class of spinner manipulators
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

using UnityEngine.UI;
using UnityEngine;
using System;


namespace vpet
{
    public abstract class Spinner : Manipulator
    {
        public enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        public Axis currentAxis;
        public ScrollSnap scrollSnap;
        [Range(0.1f, 2f)]
        public float sensitivity = 0.65f;

        protected Vector3 _value;

        //!
        //! Definition of change function parameters.
        //!
        public class FEventArgs : EventArgs
        {
            public Vector3 value;
        }

        //!
        //! function to initalize the spinner
        //!
        public void Init(Vector3 initialValue)
        {
            _value = initialValue;
        }

        public abstract void InvokeHasChanged();

        private void Start()
        {
            scrollSnap.onRelease.AddListener(AxisSnap);
            scrollSnap.onAxisDrag += AxisDrag;
        }

        //!
        //! function to perform axis drag
        //! @param axis axis to be used
        //! @param value drag value
        //!
        private void AxisDrag(Axis axis, float value)
        {
            //TODO: Shouldn't the UI send "Normalized" Data instead of Total Values?
            _value = AccelerationToLocalSpace(axis, value * sensitivity, _value);

            InvokeHasChanged();
        }

        private void AxisSnap(int index)
        {
            currentAxis = (Axis)index;
        }

        //!
        //! function converting acceleration to local space 
        //! @param axis axis to be used
        //! @param normalizedDiff normalized difference
        //! @param localReferenceValue local reference value
        //! @return acceleration in local space
        //!
        public Vector3 AccelerationToLocalSpace(Axis axis, float normalizedDiff, Vector3 localReferenceValue)
        {
            Vector3 direction = Vector3.zero;
            switch (axis)
            {
                case Axis.X:
                    direction = Vector3.right;
                    break;
                case Axis.Y:
                    direction = Vector3.up;
                    break;
                case Axis.Z:
                    direction = Vector3.forward;
                    break;
            }

            Vector3 axisDiff = direction * normalizedDiff;
            Vector3 newlocalPos = localReferenceValue + axisDiff;
            
            return newlocalPos;
        }
    }
}