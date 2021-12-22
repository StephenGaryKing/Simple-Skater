using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrickManager : MonoBehaviour
{
	[Range(10, 120)]
	public int framerate = -1;
	public TrickDictionary trickDict;
	public Transform xAxis;
	public Transform yAxis;
	public Transform zAxis;

	Dictionary<TrickScriptable, Transform> trickTransforms = new Dictionary<TrickScriptable, Transform>();

	private void Start()
	{
		//foreach(var trick in trickDict.tricks)
		//{
		//	Transform trans = new GameObject(trick.name).transform;
		//	trans.SetParent(visualModel.parent);
		//	trans.localPosition = Vector3.zero;
		//	trans.localRotation =  Quaternion.identity;
		//	trans.localScale = Vector3.one;
		//	visualModel.SetParent(trans);
		//	trickTransforms.Add(trick, trans);
		//}
	}

	// Update is called once per frame
	void Update()
    {
		Application.targetFrameRate = framerate;

        foreach(var trick in trickDict.tricks)
		{
			if (Input.GetKeyDown(trick.actionKey))
			{
				for (int i = 0; i < trick.motions.Count; i++)
				{
					switch (trick.axis)
					{
						case SteveTween.Axis.X:
							trick.GetTween(xAxis, i);
							break;
						case SteveTween.Axis.Y:
							trick.GetTween(yAxis, i);
							break;
						case SteveTween.Axis.Z:
							trick.GetTween(zAxis, i);
							break;
					}				
				}
			}
		}
    }
}
