using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : MovementState
{
	float currentRotateVelocity;
	bool rollingSwitch = false;

	[Header("Rotation")]
	public float rotationDampening;
	public float maxRotateSpeed;
	[Header("Movement")]
	public float movementDampening;
	public float regularMaxMoveSpeed;
	public float fastMaxMoveSpeed;
	public float gripSpeed;

	float slopeTheashold = 0.9f;
	Vector3 Velocity => rollingSwitch ? -rb.Forward() : rb.Forward();
	bool OverRotated => Vector3.Dot(oldNormal, newNormal) < slopeTheashold && Vector3.Dot(rb.velocity, Vector3.up) > slopeTheashold && Vector3.Dot(newNormal, Vector3.up) > slopeTheashold;
	public Vector3 oldNormal = Vector3.up;
	public Vector3 newNormal = Vector3.up;
	RaycastHit? hit = null;

	public override void Enter(Vector3 velocity)
	{
		currentRotateVelocity = 0;
		if (mover.Hit.HasValue)
		{
			hit = mover.Hit;
			ApplyRotation();
			newNormal = mover.Hit.Value.normal;
			oldNormal = newNormal;
		}

		//Figure out if switch or regular
		float dot = Vector3.Dot(velocity.normalized, Velocity);
		if (dot < 0)
		{
			rollingSwitch = !rollingSwitch;
			Debug.Log($"Changed Direction: {(rollingSwitch? "Switch" : "Regular")}");
		}
			
		velocity *= Mathf.Abs(dot);
		rb.velocity = velocity;
		rb.useGravity = false;
		rb.isKinematic = false;
	}

	public override void Exit()
	{
		//Do nothing on exit
	}

	public override Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public override void GatherInput()
	{
		//float maxSpeed = Input.GetKey(KeyCode.Space) ? fastMaxMoveSpeed : regularMaxMoveSpeed;
		var moveVec = InputManagement.Move;
		var rotateInput = moveVec.x;
		currentRotateVelocity = Mathf.Lerp(currentRotateVelocity, maxRotateSpeed * rotateInput, rotationDampening * Time.deltaTime);

		if (moveVec.y < 0)
			rb.velocity *= 0.99f;

		if (moveVec.y > 0)
			rb.AddForce(Velocity.normalized * 100);
	}

	public override void Move()
	{
		var moveDir = Velocity;
		FixPosition();
		rb.velocity = moveDir * rb.velocity.magnitude;
	}

	void FixPosition()
	{
		Ray localRay = new Ray(rb.position, -rb.Up());
		float rayLength = mover.collider.radius + 0.05f;

		if (Physics.Raycast(localRay, out RaycastHit hitLocal, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
		{
			rb.MovePosition(hitLocal.point + (rb.Up() * mover.collider.radius));
			hit = hitLocal;
			return;
		}
		hit = null;
	}

	public override void Rotate()
	{
		if (!hit.HasValue)
			return;

		var groundNormal = hit.Value.normal;
		oldNormal = Vector3.Lerp(oldNormal, newNormal, 5 * Time.fixedDeltaTime);
		oldNormal.Normalize();
		newNormal = groundNormal;

		Debug.DrawLine(transform.position, transform.position + (oldNormal / 2f), !OverRotated ? Color.green : Color.red);

		if (OverRotated)
			return;

		ApplyRotation();
	}

	void ApplyRotation()
	{
		bool useNormal = true;

		if (!hit.HasValue)
			return;

		var groundNormal = hit.Value.normal;
		
		float rotSpeed = currentRotateVelocity * Time.fixedDeltaTime;
		Quaternion addedRotation = Quaternion.Euler(0, rotSpeed, 0);

		if (rb.velocity.magnitude >= gripSpeed)
			if (Vector3.Dot(groundNormal, rollingSwitch ? -rb.Forward() : rb.Forward()) >= 0)
				useNormal = false;

		if (!useNormal)
		{
			Debug.Log("Not using normal");
			groundNormal = rb.Up();
		}

		var forward = Vector3.Cross(rb.Right(), groundNormal);
		rb.MoveRotation(Quaternion.LookRotation(forward, groundNormal) * addedRotation);
	}
}
