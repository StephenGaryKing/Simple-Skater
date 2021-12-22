using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trick", menuName = "Skateboarding/Trick Dictionary")]
public class TrickDictionary : ScriptableObject
{
	public List<TrickScriptable> tricks;
}
