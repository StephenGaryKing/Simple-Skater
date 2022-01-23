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

	public abstract void Enter();
	public abstract void Exit();
	public abstract Vector3 GetVelocity();
	public abstract void GatherInput();
	public abstract void Move();
	public abstract void Rotate();
}
