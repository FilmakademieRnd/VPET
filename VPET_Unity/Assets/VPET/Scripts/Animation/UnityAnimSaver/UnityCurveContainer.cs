using UnityEngine;
using System.Collections;

public class UnityCurveContainer {

	public string propertyName = "";
	public AnimationCurve animCurve;

	public UnityCurveContainer( string _propertyName ) {
		animCurve = new AnimationCurve ();
		propertyName = _propertyName;
	}

	public void AddValue( float animTime, float animValue )
	{
		Keyframe key = new Keyframe (animTime, animValue, 0.0f, 0.0f);
		animCurve.AddKey (key);
	}
}
