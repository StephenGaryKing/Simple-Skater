using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrickManager : MonoBehaviour
{
	public TMPro.TMP_Text trickName;
	public TrickDictionary trickDict;
	public Transform xAxis;
	public Transform yAxis;
	public Transform zAxis;

	object tweenTarget = new object();
	bool boardFlipped = false;
	

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			StartCoroutine(Trick());

			IEnumerator Trick()
			{
				yield return new WaitForSeconds(0.1f);
				var trick = trickDict.GetRandom();
				DoTrick(trick);
			}
		}
	}

	void DoTrick(TrickScriptable trick)
	{
		xAxis.localRotation = Quaternion.identity;
		yAxis.localRotation = boardFlipped? Quaternion.Euler(Vector3.up * 180) : Quaternion.identity;
		zAxis.localRotation = Quaternion.identity;

		float boardFlipMod = boardFlipped ? -1 : 1;

		DOTween.Kill(tweenTarget);
		xAxis.DOLocalRotate(Vector3.right * trick.xMult, trick.duration, RotateMode.LocalAxisAdd).SetEase(trick.xAxis).SetTarget(tweenTarget);
		yAxis.DOLocalRotate(Vector3.up * trick.yMult, trick.duration, RotateMode.LocalAxisAdd).SetEase(trick.yAxis).SetTarget(tweenTarget);
		zAxis.DOLocalRotate(Vector3.forward * trick.zMult * boardFlipMod, trick.duration, RotateMode.LocalAxisAdd).SetEase(trick.zAxis).SetTarget(tweenTarget);

		if (trick.flipsBoardDirection)
			boardFlipped = !boardFlipped;

		trickName.SetText(trick.name);
	}
}
