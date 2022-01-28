using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideTransitionState : MovementState
{
	public float rotationDampening;
	public float maxRotateSpeed;
	[Header("Links")]
	public SkateboardMover mover;

	float verticalVelocity;
	float lateralVelocity;
	float currentRotateVelocity;
	bool InsideRampBounds => distance > 0.5f && distance < lastRamp.curveMath.GetDistance(lastRamp.curve.PointsCount - 1) - 0.5f;

	public override void Enter(Vector3 velocity)
	{
		//If the given ramp is no longer valid, invalidate this state
		if (Vector3.Distance(mover.mostRecentRamp.Value, rb.position) > 1f)
		{
			mover.mostRecentRamp = new KeyValuePair<Ramp, Vector3>(null, mover.mostRecentRamp.Value);
			return;
		}

		rampNormal = rb.Up();
		verticalVelocity = velocity.y * 1.1f;
		lateralVelocity = Vector3.Scale(velocity, new Vector3(1, 0, 1)).magnitude;

		CalculateRampInfo();
		if (InsideRampBounds)
		{
			Vector3 newPosition = rb.position;
			newPosition.x = rampLocation.x;
			newPosition.z = rampLocation.z;
			rb.position = newPosition;
		}

		float dot = Vector3.Dot(velocity, rampTangent);
		if (dot < 0)
			lateralVelocity = -lateralVelocity;
	}

	public override void Exit()
	{
		//Do nothing
	}

	public override void GatherInput()
	{
		var rotateInput = Input.GetAxis("Horizontal");
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void Move()
	{
		CalculateRampInfo();

		verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
		Vector3 positionOffset = Vector3.up * verticalVelocity * Time.fixedDeltaTime;

		positionOffset += rampTangent * lateralVelocity * Time.fixedDeltaTime;
		Vector3 newPosition = rb.position;

		if (lastRamp != null)
		{
			//Left Side
			if (InsideRampBounds)
			{
				newPosition.x = rampLocation.x;
				newPosition.z = rampLocation.z;
			}
		}

		rb.MovePosition(newPosition + positionOffset);
	}

	public override void Rotate()
	{
		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), rampNormal);
		rb.MoveRotation(Quaternion.LookRotation(newForward, rampNormal) * addedRotation);
	}

	Vector3 rampLocation;
	Vector3 rampTangent;
	Vector3 rampNormal;
	float distance;
	Ramp lastRamp;
	void CalculateRampInfo()
	{
		lastRamp = mover.mostRecentRamp.Key;
		if (lastRamp != null)
		{
			rampLocation = lastRamp.curveMath.CalcPositionByClosestPoint(rb.position, out float distance);
			rampTangent = lastRamp.curveMath.CalcTangentByDistance(distance);
			rampNormal = Vector3.Cross(Vector3.up, rampTangent);
		}
	}
}
