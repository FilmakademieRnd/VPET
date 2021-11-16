using UnityEngine.UI;
using UnityEngine;
using System;


public class Spinner : MonoBehaviour
{
    private Vector3 _value;

    private Slider _sliderX, _sliderY, _sliderZ;

    //!
    //! Definition of change function parameters.
    //!
    public class FEventArgs : EventArgs
    {
        public Vector3 value;
    }

    public delegate void spinnerEventHandler(Vector3 v);
    //!
    //! Event emitted when parameter changed.
    //!
    public event spinnerEventHandler hasChanged;

    private void Awake()
    {
        _value = Vector3.zero;

        _sliderX = transform.Find("SliderX").GetComponent<Slider>();
        _sliderY = transform.Find("SliderY").GetComponent<Slider>();
        _sliderZ = transform.Find("SliderZ").GetComponent<Slider>();

        _sliderX.onValueChanged.AddListener(updateParameterX);
        _sliderY.onValueChanged.AddListener(updateParameterY);
        _sliderZ.onValueChanged.AddListener(updateParameterZ);

    }

    private void updateParameterX(float v)
    {
        _value.x = v;
        hasChanged?.Invoke(_value);
    }

    private void updateParameterY(float v)
    {
        _value.y = v;
        hasChanged?.Invoke(_value);
    }

    private void updateParameterZ(float v)
    {
        _value.z = v;
        hasChanged?.Invoke(_value);
    }

    public void setValues (Vector3 v) 
    {
        // [REVIEW] 
        // this should be related to the scene scale

        _sliderX.maxValue = Mathf.Abs(v.x) * 2;
        _sliderX.minValue = Mathf.Abs(v.x) * -2;        
        
        _sliderY.maxValue = Mathf.Abs(v.y) * 2;
        _sliderY.minValue = Mathf.Abs(v.y) * -2;

        _sliderZ.maxValue = Mathf.Abs(v.z) * 2;
        _sliderZ.minValue = Mathf.Abs(v.z) * -2;

        _sliderX.value = v.x;
        _sliderY.value = v.y;
        _sliderZ.value = v.z;
    }

}
