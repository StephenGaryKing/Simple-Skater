using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideTransitionState : MovementState
{
	public float rotationDampening;
	public float maxRotateSpeed;
	public float fixRotationThreshold = 20;
	//public float cancelFixThreshold = 20;

	protected float verticalVelocity;
	float lateralVelocity;
	float currentRotateVelocity;
	bool InsideRampBounds => mover.mostRecentRamp.Key != null &&
		rb.position.y >= mover.rampLocation.y && 
		(mover.distance > 0.5f && 
		mover.distance < mover.mostRecentRamp.Key.curveMath.GetDistance(mover.mostRecentRamp.Key.curve.PointsCount - 1) - 0.5f);

	bool ridingSwitch;
	float angleLastUpdate;
	bool fixingRotation = false;

	public override void Enter(Vector3 velocity)
	{
		fixingRotation = true;
		rb.isKinematic = true;
		mover.CalculateRampInfo();

		mover.rampNormal = rb.Up();
		verticalVelocity = velocity.y;
		Vector3 flatVelocity = Vector3.Scale(velocity, new Vector3(1, 0, 1));
		lateralVelocity = flatVelocity.magnitude;

		if (InsideRampBounds)
		{
			Vector3 newPosition = rb.position;
			newPosition.x = mover.rampLocation.x;
			newPosition.z = mover.rampLocation.z;
			newPosition += mover.rampNormal * 0.25f;
			rb.position = newPosition;
		}

		float dot = Vector3.Dot(flatVelocity.normalized, mover.rampTangent);
		lateralVelocity *= dot;
		ridingSwitch = Vector3.Dot(velocity, rb.Forward()) < 0;

		Vector3 forward = ridingSwitch ? -rb.Forward() : rb.Forward();
		var angle = Vector3.Angle(forward, Vector3.up);
		angleLastUpdate = angle;
	}

	public override void Exit()
	{
		//Do nothing
	}

	public override void GatherInput()
	{
		var rotateInput = Input.GetAxis("Horizontal");

		if (rotateInput == 0 && fixingRotation)
			rotateInput = GetFixRotationInput();

		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);
	}

	float GetFixRotationInput()
	{
		if (!fixingRotation)
			return 0;

		Vector3 forward = ridingSwitch ? -rb.Forward() : rb.Forward();
		var angle = Vector3.Angle(forward, Vector3.up);
		if (angle <= fixRotationThreshold || angle < (angleLastUpdate - fixRotationThreshold))
		{
			fixingRotation = false;
			return 0;
		}
		angleLastUpdate = angle;

		angle = Vector3.Angle(-forward, Vector3.up);
		if (angle <= fixRotationThreshold * 2f)
		{
			fixingRotation = false;
			return 0;
		}

		float dot = Vector3.Dot(rb.Right(), Vector3.up);
		float ret = (ridingSwitch ? 1 : -1) / 2f;
		if (dot < 0)
			return -ret;
		else
			return ret;
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void Move()
	{
		mover.CalculateRampInfo();

		verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
		Vector3 positionOffset = Vector3.up * verticalVelocity * Time.fixedDeltaTime;

		positionOffset += mover.rampTangent * lateralVelocity * Time.fixedDeltaTime;
		Vector3 newPosition = rb.position;

		if (mover.mostRecentRamp.Key != null)
		{
			//Left Side
			if (InsideRampBounds)
			{
				newPosition.x = mover.rampLocation.x;
				newPosition.z = mover.rampLocation.z;
				newPosition += mover.rampNormal * mover.collider.radius;
			}
		}

		Ray localRay = new Ray(rb.position, rb.velocity.normalized);
		if (Physics.Raycast(localRay, out RaycastHit hit, (rb.velocity.magnitude * Time.fixedDeltaTime) + 0.1f, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
		{
			if (Vector3.Angle(hit.normal, rb.Up()) > 45)
			{
				mover.SetStacked();
				return;
			}
		}

		rb.MovePosition(newPosition + positionOffset);
	}

	public override void Rotate()
	{
		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), mover.rampNormal);
		Quaternion newRotation = rb.rotation;
		if (InsideRampBounds)
			newRotation = Quaternion.LookRotation(newForward, mover.rampNormal);
		else if (mover.mostRecentRamp.Key != null && rb.velocity.y <= 0)
		{
			Ray localRay = new Ray(rb.position, Vector3.down);
			float rayLength = Mathf.Abs(rb.velocity.y) * Time.fixedDeltaTime * 20f;

			if (Physics.Raycast(localRay, out RaycastHit hit, rayLength,
				LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
			{
				Ramp ramp = hit.collider.GetComponent<Ramp>();
				if (ramp == null)
				{
					//mover.mostRecentRamp = new KeyValuePair<Ramp, Vector3>(null, Vector3.zero);
				}
			}
		}

		rb.MoveRotation(newRotation * addedRotation);
	}
}
