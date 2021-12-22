using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SteveTween
{
	[System.Serializable]
	public enum Axis
	{
		X,
		Y,
		Z
	}

	class SteveTweenManager : MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}

	static GameObject tweenManager;
	static MonoBehaviour TweenManager
	{
		get
		{
			if (tweenManager == null)
			{
				tweenManager = new GameObject();
				tweenManager.AddComponent<SteveTweenManager>();
			}
			return tweenManager.GetComponent<SteveTweenManager>();
		}
	}

    public static Coroutine TweenRotate(this Transform trans, Axis axis, float distance, float duration)
	{
		return TweenManager.StartCoroutine(RotateCoroutine(trans, axis, distance, duration));
	}

	static IEnumerator RotateCoroutine(Transform trans, Axis axis, float distance, float duration)
	{



		//Calculate rotations per seccond
		float rot = distance / duration;
		float time = Time.time;
		float startTime = time;
		float previousUpdate = startTime;
		float delta = 0;

		while (true)
		{
			time = Time.time;
			if (time - startTime > duration)
			{
				delta = (startTime + duration) - previousUpdate;
				Rotate(trans, axis, rot * delta);
				break;
			}

			delta = time - previousUpdate;
			previousUpdate = time;
			Rotate(trans, axis, rot * delta);
			yield return new WaitForEndOfFrame();
		}
	}

	static void Rotate(Transform trans, Axis axis, float amount)
	{
		switch (axis)
		{
			case Axis.X:
				trans.Rotate(amount, 0, 0, Space.Self);
				break;
			case Axis.Y:
				trans.Rotate(0, amount, 0, Space.Self);
				break;
			case Axis.Z:
				trans.Rotate(0, 0, amount, Space.Self);
				break;
			default:
				break;
		}
	}
}
