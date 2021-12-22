using BansheeGz.BGSpline.Curve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ramp : MonoBehaviour
{
	public BGCurve curve;
	public BGCurveBaseMath curveMath;

	private void Start()
	{
		curveMath = new BGCurveBaseMath(curve, new BGCurveBaseMath.Config(BGCurveBaseMath.Fields.PositionAndTangent));
	}
}
