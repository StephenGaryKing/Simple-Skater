using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trick", menuName = "Skateboarding/Trick Dictionary")]
public class TrickDictionary : ScriptableObject
{
	public List<TrickScriptable> tricks;
	public TrickScriptable solo;

	public TrickScriptable GetRandom()
	{
		if (solo != null)
			return solo;

		var index = UnityEngine.Random.Range(0, tricks.Count);
		return tricks[index];
	}
}
