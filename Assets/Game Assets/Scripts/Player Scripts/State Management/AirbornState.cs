using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirbornState : MovementState
{
	float currentRotateVelocity;

	[Header("Rotation")]
	public float rotationDampening;
	public float maxRotateSpeed;

	public override void Enter(Vector3 velocity)
	{
		rb.isKinematic = false;
		rb.velocity = velocity;
		rb.useGravity = true;
		currentRotateVelocity = 0;
	}

	public override void Exit()
	{
		//Do nothing
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void GatherInput()
	{
		var rotateInput = Input.GetAxis("Horizontal");
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);
	}

	public override void Move()
	{
		//Let physics move the player
	}

	public override void Rotate()
	{
		var normal = rb.Up();
		float dot = Vector3.Dot(normal, Vector3.up);
		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		float lerpSpeed = 1;

		if (mover.mostRecentRamp.Key == null || dot > 0.1f || dot < 0)
		{
			Ray localRay = new Ray(rb.position + rb.Up(), rb.velocity.normalized);
			float rayLength = rb.velocity.magnitude * Time.fixedDeltaTime * 20f;

			if (Physics.Raycast(localRay, out RaycastHit hit, rayLength,
				LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
			{
				normal = hit.normal;
				lerpSpeed = 0.1f;
			}
			else
			{
				normal = Vector3.Slerp(normal, Vector3.up, 0.05f);
			}
		}
		
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), normal);

		var newRotation = Quaternion.LookRotation(newForward, normal);
		newRotation = Quaternion.Lerp(rb.rotation, newRotation, lerpSpeed);

		rb.MoveRotation(newRotation * addedRotation);
	}
}
