using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertFollowCam : MonoBehaviour
{
	public Rigidbody rb;

	public float positionDampening = 0.1f;
	public float RotationDampening = 0.1f;
	public float distance = 5f;
	public float vOffset = 0;

	private void FixedUpdate()
	{
		Vector3 offset = Vector3.up;
		offset *= distance;

		var startPos = rb.position;
		var newPos = startPos + offset + (rb.Up() * vOffset);
		//var rayDir = newPos - startPos;
		//Ray camRay = new Ray(startPos, rayDir);
		//if (Physics.SphereCast(camRay, 0.1f, out var hit, distance, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
		//	newPos = startPos + (rayDir * (hit.distance / 5f));
		transform.position = Vector3.Lerp(transform.position, newPos, positionDampening);

		var oldRot = transform.rotation;
		transform.LookAt(startPos + (rb.Up() * -vOffset));
		var newRot = transform.rotation;
		transform.rotation = Quaternion.Lerp(oldRot, newRot, RotationDampening);
	}
}
