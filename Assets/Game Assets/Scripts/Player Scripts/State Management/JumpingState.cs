using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : AirbornState
{
	public float jumpAmount;

	public override void Enter(Vector3 velocity)
	{
		base.Enter(velocity);
		this.velocity += Vector3.up * jumpAmount;
	}
}
