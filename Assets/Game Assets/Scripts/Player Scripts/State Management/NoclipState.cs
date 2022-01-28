using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoclipState : MovementState
{
	Vector3 inputVelocity;
	float rotationVelocity;
	public float moveSpeedMultiplier = 2;
	public float rotateSpeedMultiplier = 1;

	public override void Enter(Vector3 velocity)
	{
		//Do nothing with the previous velocity
	}
	public override void Exit()
	{
		//Do nothing on exit
	}

	public override void GatherInput()
	{
		inputVelocity.x = Input.GetAxisRaw("Horizontal");
		inputVelocity.z = Input.GetAxisRaw("Vertical");
		inputVelocity.y = Input.GetKey(KeyCode.Space)? 1 : Input.GetKey(KeyCode.LeftShift)? -1 : 0;
		rotationVelocity = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;
	}

	public override void Move()
	{
		var moveSpeed = inputVelocity * moveSpeedMultiplier * Time.fixedDeltaTime;
		var moveDir = rb.rotation * moveSpeed;
		rb.MovePosition(rb.position + moveDir);
	}

	public override void Rotate()
	{
		float rotSpeed = rotationVelocity * rotateSpeedMultiplier * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), Vector3.up);
		rb.MoveRotation(Quaternion.LookRotation(newForward, Vector3.up) * addedRotation);
	}

	public override Vector3 GetVelocity()
	{
		return Vector3.zero;
	}
}
