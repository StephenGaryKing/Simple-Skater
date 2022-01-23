using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : MovementState
{
	public float jumpAmount;

	public override void Enter()
	{
		rb.useGravity = true;
		rb.isKinematic = false;
		rb.AddForce(Vector3.up * jumpAmount, ForceMode.VelocityChange);
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
		//Do nothing
	}

	public override void Move()
	{
		//Do nothing
	}

	public override void Rotate()
	{
		//Do nothing
	}
}
