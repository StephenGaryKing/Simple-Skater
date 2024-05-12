using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedFollowCam : MonoBehaviour
{
	public Rigidbody rb;

	public float MovementDampening = 0.1f;
	public float RotationDampening = 0.1f;
	public float distance = 5f;
	public Vector3 cameraPosOffset;
	public Vector3 cameraLookOffset;

	private void FixedUpdate()
	{
		Vector3 offset = -rb.velocity;
		offset.y = 0;
		if (offset == Vector3.zero)
			offset = -rb.Forward();
		offset.Normalize();
		offset *= distance;

		var startPos = rb.position + cameraPosOffset;
		var newPos = startPos + offset;

		var rayDir = newPos - startPos;
		Ray camRay = new Ray(startPos, rayDir);
		if (Physics.SphereCast(camRay, 0.1f, out var hit, distance, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
			newPos = startPos + (rayDir * (hit.distance / 5f));
		transform.position = Vector3.Lerp(transform.position, newPos, MovementDampening);

		var oldRot = transform.rotation;
		transform.LookAt(startPos + cameraLookOffset);
		var newRot = transform.rotation;
		transform.rotation = Quaternion.Lerp(oldRot, newRot, RotationDampening);
	}
}
