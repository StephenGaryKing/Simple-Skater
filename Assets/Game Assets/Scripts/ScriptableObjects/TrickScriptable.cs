using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

[CreateAssetMenu(fileName = "Trick", menuName = "Skateboarding/Trick")]
public class TrickScriptable : ScriptableObject
{
	public bool flipsBoardDirection = false;
	public float duration;
	public AnimationCurve xAxis;
	public float xMult;
	public AnimationCurve yAxis;
	public float yMult;
	public AnimationCurve zAxis;
	public float zMult;
}
