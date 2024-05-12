using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetState : MovementState
{
	Vector3 startingPos;

	private void Start()
	{
		startingPos = rb.position;
	}

	public override void Enter(Vector3 velocity)
	{
		rb.MovePosition(startingPos);
	}

	public override void Exit()
	{
	}

	public override void GatherInput()
	{
	}

	public override Vector3 GetVelocity()
	{
		return Vector3.zero;
	}

	public override void Move()
	{
	}

	public override void Rotate()
	{
	}
}
