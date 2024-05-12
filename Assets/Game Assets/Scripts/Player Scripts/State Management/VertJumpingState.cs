using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertJumpingState : InsideTransitionState
{
	public float jumpAmount = 1f;

	public override void Enter(Vector3 velocity)
	{
		if (velocity.y <= 0)
			base.Enter(velocity);
		else
			base.Enter(velocity + Vector3.up * jumpAmount);
		//velocity = Vector3.Scale(velocity, new Vector3(1, 0, 1));

	}

	public override void Exit()
	{
		base.Exit();
	}
}
