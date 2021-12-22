using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static SteveTween;

[CreateAssetMenu(fileName = "Trick", menuName = "Skateboarding/Trick")]
public class TrickScriptable : ScriptableObject
{
	[System.Serializable]
	public struct Motion
	{
		public float duration;
		public float rotation;
	}

	public KeyCode actionKey;
	public Axis axis;
	public List<Motion> motions;

	public Coroutine GetTween(Transform trans, int index)
	{
		Motion motion = motions[index];
		return trans.TweenRotate(axis, motion.rotation, motion.duration);
	}
}
