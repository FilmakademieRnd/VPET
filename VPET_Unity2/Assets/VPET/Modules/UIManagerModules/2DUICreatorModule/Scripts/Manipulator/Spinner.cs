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

        #if UNITY_EDITOR
        [ReadOnly]
        #endif
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

        //TODO: Should function as a joystick
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
            Vector3 newlocalPos = localReferenceValue + axisDiff;//Vector3.Scale(localReferenceValue, axisDiff);
            
            return newlocalPos;
        }
    }
}