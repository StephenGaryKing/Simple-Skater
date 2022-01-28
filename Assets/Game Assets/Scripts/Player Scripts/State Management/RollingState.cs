using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : MovementState
{
	float currentMoveVelocity;
	float currentRotateVelocity;
	RaycastHit? hit;
	bool rollingSwitch = false;

	[Header("Rotation")]
	public float rotationDampening;
	public float maxRotateSpeed;
	[Header("Movement")]
	public float movementDampening;
	public float maxMoveSpeed;
	[Header("Links")]
	public SkateboardMover mover;

	Vector3 Velocity => rollingSwitch ? -rb.Forward() : rb.Forward();
	bool OverRotated => Vector3.Dot(oldNormal, newNormal) < 0.8f;
	public Vector3 oldNormal = Vector3.up;
	public Vector3 newNormal = Vector3.up;

	public override void Enter(Vector3 velocity)
	{
		rb.MovePosition(rb.position + velocity);
		currentMoveVelocity = 0;
		currentRotateVelocity = 0;
		hit = mover.GetDownHit(rb);
		RotateToNormal();
		if (hit.HasValue)
		{
			newNormal = hit.Value.normal;
			oldNormal = newNormal;
		}

		//Figure out if switch or regular
		float dot = Vector3.Dot(velocity.normalized, Velocity);
		if (dot < 0)
		{
			rollingSwitch = !rollingSwitch;
			Debug.Log($"Changed Direction: {(rollingSwitch? "Switch" : "Regular")}");
		}

		if (velocity.magnitude > 0.5f && Mathf.Abs(dot) < 0.2f)
			mover.SetStacked();
		//Transfer speed based on coherance with travel direction
		else
			velocity *= Mathf.Abs(dot);

		currentMoveVelocity = velocity.magnitude;
	}

	public override void Exit()
	{
		//Do nothing on exit
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void GatherInput()
	{
		hit = mover.GetDownHit(rb);

		var rotateInput = Input.GetAxis("Horizontal");
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);

		if (Input.GetKey(KeyCode.S))
			currentMoveVelocity = Mathf.Lerp(currentMoveVelocity, 0, movementDampening * 2 * Time.deltaTime);
		else
			currentMoveVelocity = Mathf.Lerp(currentMoveVelocity, maxMoveSpeed, movementDampening * Time.deltaTime);

	}

	public override void Move()
	{
		var moveDir = Velocity * currentMoveVelocity * Time.fixedDeltaTime;
		var currentPosition = hit.HasValue && !OverRotated ? hit.Value.point : rb.position;
		rb.position = currentPosition;
		rb.MovePosition(rb.position + moveDir);
	}

	public override void Rotate()
	{
		if (!hit.HasValue)
			return;

		var groundNormal = hit.Value.normal;
		oldNormal = Vector3.Lerp(oldNormal, newNormal, 5 * Time.fixedDeltaTime);
		oldNormal.Normalize();
		newNormal = groundNormal;

		Debug.DrawLine(transform.position, transform.position + (oldNormal / 2f), !OverRotated ? Color.green : Color.red);

		if (OverRotated)
			return;

		RotateToNormal();
	}

	void RotateToNormal()
	{
		if (!hit.HasValue)
			return;

		var groundNormal = hit.Value.normal;
		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var forward = Vector3.Cross(rb.Right(), groundNormal);
		CheckForWallCollisions(rollingSwitch? -forward : forward, out var newForward, out var similarity);
		currentMoveVelocity *= similarity;
		rb.MoveRotation(Quaternion.LookRotation(rollingSwitch? -newForward : newForward, groundNormal) * addedRotation);
	}
}
