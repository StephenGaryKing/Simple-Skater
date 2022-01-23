using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardVisuals : MonoBehaviour
{
	public SkateboardMover mover;
	public Transform visualObject;
	public PlayerSettings playerSettings;

	Quaternion cachedRotation = Quaternion.identity;
	float cachedDampening = 1;

	private void FixedUpdate()
	{
		if (mover.Grounded)
			cachedDampening = 0.9f;
		else
			cachedDampening = playerSettings.visualRotationDampening;
		
		cachedRotation = Quaternion.Lerp(cachedRotation, mover.transform.rotation, cachedDampening);
	}

	private void Update()
	{
		visualObject.rotation = cachedRotation;
	}
}
