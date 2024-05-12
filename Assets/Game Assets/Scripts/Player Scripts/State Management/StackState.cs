using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackState : MovementState
{
	public override void Enter(Vector3 velocity)
	{
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.velocity = velocity;
	}

	public override void Exit()
	{
		
	}

	public override void GatherInput()
	{
		
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void Move()
	{

	}

	public override void Rotate()
	{
		var groundNormal = Vector3.up;

		if (mover.Hit.HasValue)
			groundNormal = mover.Hit.Value.normal;

		var forward = Vector3.Cross(rb.Right(), groundNormal);
		rb.MoveRotation(Quaternion.LookRotation(forward, groundNormal));
	}
}
