using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class Spinner : Slider
{
    public new SpinnerEvent onValueChanged { get; set; }

    public new Vector3 value;

    public class SpinnerEvent : UnityEvent<Vector3>
    {
        public SpinnerEvent() { }
    }
}
