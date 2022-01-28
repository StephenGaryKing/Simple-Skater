using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackState : MovementState
{
	RaycastHit? hit;
	protected Vector3 velocity;

	[Header("Links")]
	public SkateboardMover mover;

	public override void Enter(Vector3 velocity)
	{
		this.velocity = velocity;
		hit = null;
	}

	public override void Exit()
	{
		
	}

	public override void GatherInput()
	{
		hit = mover.GetDownHit(rb);
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void Move()
	{
		Vector3 inVelocity = Vector3.Scale(velocity, new Vector3(1, 0, 1));
		CheckForWallCollisions(inVelocity, out var outVelocity, out var similarity);
		velocity = outVelocity + Vector3.Scale(velocity, Vector3.up);
		velocity *= similarity;
		velocity = Vector3.Lerp(velocity, Vector3.zero, 0.02f);

		Vector3 currentPosition;
		if (!hit.HasValue)
		{
			velocity += Physics.gravity * Time.fixedDeltaTime;
			currentPosition = rb.position;
		}
		else
		{
			velocity = Vector3.Scale(velocity, new Vector3(1, 0, 1));
			currentPosition = hit.Value.point;
		}

		rb.MovePosition(currentPosition + (velocity * Time.fixedDeltaTime));
	}

	public override void Rotate()
	{
		if (!hit.HasValue)
			return;

		var groundNormal = hit.Value.normal;
		var forward = Vector3.Cross(rb.Right(), groundNormal);
		CheckForWallCollisions(forward, out var newForward, out var similarity);
		rb.MoveRotation(Quaternion.LookRotation(newForward, groundNormal));
	}
}
