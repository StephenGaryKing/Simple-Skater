using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirbornState : MovementState
{
	public float currentRotateVelocity;
	protected Vector3 velocity;
	bool hittingWall;

	[Header("Rotation")]
	public float rotationDampening;
	public float maxRotateSpeed;

	public override void Enter(Vector3 velocity)
	{
		this.velocity = velocity;
		currentRotateVelocity = 0;
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
		var rotateInput = Input.GetAxis("Horizontal");
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);
	}

	public override void Move()
	{
		velocity += Physics.gravity * Time.fixedDeltaTime;
		Vector3 inVelocity = Vector3.Scale(velocity, new Vector3(1, 0, 1));
		CheckForWallCollisions(inVelocity, out var outVelocity, out var similarity);
		velocity = outVelocity + Vector3.Scale(velocity, Vector3.up);
		velocity *= similarity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}

	public override void Rotate()
	{
		var normal = Vector3.Lerp(rb.Up(), Vector3.up, 0.05f);

		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);
		var newForward = Vector3.Cross(rb.Right(), normal);
		rb.MoveRotation(Quaternion.LookRotation(newForward, normal) * addedRotation);
	}
}
