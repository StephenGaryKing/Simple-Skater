using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : AirbornState
{
	public float jumpAmount = 5f;
	public float modifyJumpAfterAngle = 90f;

	public override void Enter(Vector3 velocity)
	{
		rb.isKinematic = false;
		rb.useGravity = true;
		
		mover.mostRecentRamp = new KeyValuePair<Ramp, Vector3>(null, mover.mostRecentRamp.Value);
		base.Enter(velocity);

		Vector3 up = Vector3.up;
		if (Vector3.Angle(rb.Up(), Vector3.up) > modifyJumpAfterAngle)
			up = rb.Up();
		rb.AddForce(up * jumpAmount, ForceMode.Impulse);
	}
}
