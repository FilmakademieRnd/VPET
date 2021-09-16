using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;


public class Spinner : MonoBehaviour
{
    private Vector3 _value;
    
    //!
    //! Definition of change function parameters.
    //!
    public class FEventArgs : EventArgs
    {
        public Vector3 value;
    }
    //!
    //! Event emitted when parameter changed.
    //!
    public event EventHandler<FEventArgs> hasChanged;

    private void Awake()
    {
        _value = Vector3.zero;

        Slider sliderX = GameObject.Find("SpinnerX").GetComponent<Slider>();
        Slider sliderY = GameObject.Find("SpinnerY").GetComponent<Slider>();
        Slider sliderZ = GameObject.Find("SpinnerZ").GetComponent<Slider>();

        sliderX.onValueChanged.AddListener(updateParameterX);
        sliderY.onValueChanged.AddListener(updateParameterY);
        sliderZ.onValueChanged.AddListener(updateParameterZ);
    }

    private void updateParameterX(float v)
    {
        _value.x = v;
        hasChanged?.Invoke(this, new FEventArgs { value = _value });
    }

    private void updateParameterY(float v)
    {
        _value.y = v;
        hasChanged?.Invoke(this, new FEventArgs { value = _value });
    }

    private void updateParameterZ(float v)
    {
        _value.z = v;
        hasChanged?.Invoke(this, new FEventArgs { value = _value });
    }


}
