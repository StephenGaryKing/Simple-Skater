using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : MovementState
{
	float currentMoveVelocity;
	float currentRotateVelocity;
	RaycastHit? hit;

	[Header("Rotation")]
	public float rotationDampening;
	public float maxRotateSpeed;
	[Header("Movement")]
	public float movementDampening;
	public float maxMoveSpeed;

	public override void Enter()
	{
		//Figure out if switch or regular

		//Transfer speed based on coherance with travel direction
	}

	public override void Exit()
	{
		//Do nothing on exit
	}

	public override Vector3 GetVelocity()
	{
		return rb.Forward() * currentMoveVelocity;
	}

	public override void GatherInput()
	{
		hit = SkateboardMover.GetDownHit(rb);

		currentMoveVelocity = Mathf.Lerp(currentMoveVelocity, maxMoveSpeed, movementDampening * Time.deltaTime);

		var rotateInput = Input.GetAxis("Horizontal");
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);
	}

	public override void Move()
	{
		var moveDir = rb.Forward() * currentMoveVelocity * Time.fixedDeltaTime;
		var currentPosition = hit.HasValue ? hit.Value.point : rb.position;

		rb.MovePosition(currentPosition + moveDir);
	}

	public override void Rotate()
	{
		var groundNormal = hit.HasValue? hit.Value.normal : Vector3.up;

		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), groundNormal);
		rb.MoveRotation(Quaternion.LookRotation(newForward, groundNormal) * addedRotation);
	}
}
