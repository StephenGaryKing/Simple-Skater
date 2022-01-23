using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirbornState : MovementState
{
	Vector3 fallVelocity;

	public override void Enter()
	{
		rb.useGravity = true;
		rb.isKinematic = false;
	}

	public override void Exit()
	{
		rb.useGravity = false;
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void GatherInput()
	{
		fallVelocity += Physics.gravity * Time.deltaTime;
	}

	public override void Move()
	{
		//Movement is handled by the rigidbody's physics
	}

	public override void Rotate()
	{
		//Rotation is handled by the rigidbody's physics
	}
}
