using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class MovementState : MonoBehaviour
{
	public Rigidbody rb
	{
		get;
		protected set;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		GatherInput();
	}

	private void FixedUpdate()
	{
		Rotate();
		Move();
	}

	public abstract void Enter(Vector3 velocity);
	public abstract void Exit();
	public abstract Vector3 GetVelocity();
	public abstract void GatherInput();
	public abstract void Move();
	public abstract void Rotate();

	float wallColSize = 0.2f;
	float wallColLength = 0.7f;
	Ray wallColRay = new Ray();
	protected bool CheckForWallCollisions(Vector3 velocity, out Vector3 outputVelocity, out float similarity)
	{
		Vector3 up = rb.Up();
		wallColRay = new Ray(rb.position + up, -up);
	
		bool hitWall = Physics.CapsuleCast(wallColRay.origin, wallColRay.origin + wallColRay.direction * wallColLength, wallColSize, velocity, out RaycastHit hit, wallColSize, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp"));

		//Hit a wall
		if (hitWall)
			outputVelocity = Vector3.Reflect(rb.velocity, hit.normal);
		else
			outputVelocity = velocity;

		similarity = (Vector3.Dot(velocity.normalized, outputVelocity.normalized) + 1) / 2f;
		return hitWall;
	}
}
