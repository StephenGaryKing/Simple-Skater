using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardMover : MonoBehaviour, IMover
{
	#region OLD
	Rigidbody controller;
	Vector3 velocity = Vector3.zero;

	public Animator stateManager;
	public float skinWidth = 0.1f;
	public float leavingGroundThreashold = 0.5f;
	public bool rollingSwitch = false;
	public PlayerSettings playerSettings;

	float currentRollingSpeed = 0;
	float currentFallingSpeed = 0;
	Vector3 lastForward = Vector3.forward;
	Vector3 lastNormal = Vector3.zero;

	bool shouldJump = false;
	float turnInput = 0;
	bool stopping = false;
	bool wasAirborn = false;
	float leavingGroundDuration = 0;
	bool isGrounded = false;
	bool inRamp = false;
	GameObject lastObjectTouched = null;
	Ramp lastRamp = null;
	#endregion

	public Animator stateHandler;

	[System.Serializable]
	public class State
	{
		public string stateName;
		public MovementState state;
	}
	private State currentState;
	[SerializeField]
	public List<State> states;

	Rigidbody Rb => currentState.state.rb;

	public static RaycastHit? GetDownHit(Rigidbody rb)
	{
		Ray ray = new Ray(rb.position + rb.Up(), -rb.Up());
		float rayLength = 1.1f;
		if (Physics.Raycast(ray, out RaycastHit hit, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
			return hit;
		return null;
	}

	public bool Grounded { get => isGrounded; }

	void Start()
	{
		foreach(var state in states)
			state.state.enabled = false;

		//currentState.state.Enter(Vector3.zero);
		//controller = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
			stateHandler.SetBool("NoClip", !stateHandler.GetBool("NoClip"));

		if (Input.GetKeyDown(KeyCode.Space))
			stateHandler.SetBool("Jumping", true);

		foreach (var state in states)
		{
			//When entering the state in the Animator, switch to that state in the internal states
			if (stateHandler.GetCurrentAnimatorStateInfo(0).IsName(state.stateName))
			{
				SwitchStates(state);
			}
		}

		//if (Input.GetKeyUp(KeyCode.Space))
		//{
		//	SwitchStates(GetState("Airborn"), Vector3.up * 5f);
		//}

		//Debug.Log(Rb.velocity);

		/*
		if (Input.GetKeyUp(KeyCode.Space))
			shouldJump = true;

		//Fix Rotation
		FixRotation();

		if (!isGrounded)
		{
			if (inRamp)
				MoveOnRamp();
			if (!inRamp)
				MoveAirborn();
		}
		else
		{
			MoveGrounded();
		}

		stateManager.SetBool("InRamp", inRamp);
		stateManager.SetBool("TouchingGround", isGrounded);
		shouldJump = false;
		*/
	}

	private void FixedUpdate()
	{
		if (currentState == null)
			return;

		stateHandler.SetBool("Grounded", GetDownHit(Rb) != null);
		stateHandler.SetFloat("XVel", Rb.velocity.x);
		stateHandler.SetFloat("YVel", Rb.velocity.y);
		stateHandler.SetFloat("ZVel", Rb.velocity.z);

		/*
		Vector3 m_EulerAngleVelocity = new Vector3(0, turnInput * (wasAirborn? 2000f : 1000f), 0);
		Quaternion q = controller.rotation * Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);

		controller.MoveRotation(q);
		controller.MovePosition(controller.position + (velocity * Time.fixedDeltaTime));

		CheckForWallCollisions();
		*/
	}

	State GetState(string stateName)
	{
		foreach (var state in states)
			if (state.stateName == stateName)
				return state;
		return null;
	}

	void SwitchStates(State newState)
	{
		if (newState == currentState)
			return;

		Debug.Log($"State Change: {newState.stateName}");

		if (currentState != null)
		{
			currentState.state.Exit();
			currentState.state.enabled = false;
		}
		newState.state.enabled = true;
		newState.state.Enter();
		currentState = newState;
	}

	Vector3 rampLocation;
	Vector3 rampTangent;
	Vector3 rampNormal;
	Vector3 flattenScale = new Vector3(1, 0, 1);
	void MoveOnRamp()
	{
		rampLocation = lastRamp.curveMath.CalcPositionByClosestPoint(controller.position, out float distance);
		rampTangent = lastRamp.curveMath.CalcTangentByDistance(distance);

		//front or back
		if (Vector3.Distance(Vector3.Scale(controller.position, flattenScale), Vector3.Scale(rampLocation, flattenScale)) > 1f)
		{
			inRamp = false;
			lastNormal = Vector3.up;
			return;
		}

		//left side
		if (distance < 0.5f)
		{
			inRamp = false;
			lastNormal = Vector3.up;
			lastForward = -rampTangent;
			controller.rotation = Quaternion.LookRotation(lastForward, lastNormal);
			return;
		}

		//right side
		if (distance > lastRamp.curveMath.GetDistance(lastRamp.curve.PointsCount - 1) - 0.5f)
		{ 
			inRamp = false;
			lastNormal = Vector3.up;
			lastForward = rampTangent;
			controller.rotation = Quaternion.LookRotation(lastForward, lastNormal);
			return;
		}

		rampNormal = Vector3.Cross(Vector3.up, rampTangent);
		SetUp(rampNormal);
		ApplyGravity();

		Vector3 pos = rampLocation;
		pos.y = controller.position.y;
		controller.position = pos;

		float angle = Vector3.SignedAngle(lastNormal, rampNormal, Vector3.up);
		Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
		lastForward = rot * lastForward;

		lastNormal = rampNormal;
		MoveGeneric();
	}

	void ApplyGravity()
	{
		//Apply Gravity
		currentFallingSpeed += Physics.gravity.magnitude * Time.deltaTime;
		leavingGroundDuration += Time.deltaTime;

		if (leavingGroundDuration >= leavingGroundThreashold)
			wasAirborn = true;
	}

	private void MoveAirborn()
	{
		ApplyGravity();
		MoveGeneric();
	}

	void MoveGrounded()
	{
		//Add push force
		if (stopping)
			currentRollingSpeed -= playerSettings.pushForce * Time.deltaTime;
		else if (currentRollingSpeed < playerSettings.maxPushForce)
		{
			currentRollingSpeed = Mathf.Max(0, currentRollingSpeed);
			currentRollingSpeed += playerSettings.pushForce * Time.deltaTime;
		}
		currentRollingSpeed = Mathf.Clamp(currentRollingSpeed, -playerSettings.maxPushForce, playerSettings.maxPushForce);

		leavingGroundDuration = 0;
		if (wasAirborn)
		{
			//Check dot of velocity and forward to chang to switch
			float dot = Vector3.Dot(controller.velocity, rollingSwitch ? -controller.transform.forward : controller.transform.forward);
			if (dot < 0)
			{
				rollingSwitch = !rollingSwitch;
			}
		}

		currentFallingSpeed = 0f;

		if (shouldJump)
		{
			Jump(playerSettings.jumpForce);
		}
		shouldJump = false;

		MoveGeneric();
	}

	void Jump(float force = 0f)
	{
		isGrounded = false;
		shouldJump = false;

		float angle = Vector3.Angle(controller.Up(), Vector3.up);
		if (angle > 45)
		{
			inRamp = true;
			if (lastObjectTouched.layer == LayerMask.NameToLayer("Ramp"))
			{
				//Get the spline connected to the ramp and get the point along the spline the player is closest to
				lastRamp = lastObjectTouched.GetComponent<Ramp>();
				lastForward = controller.Forward();
				lastNormal = controller.Up();
			}

			//Flatten out vertical component of the velocity in world space
			currentFallingSpeed += (-velocity.y * 1.5f) + force;
			velocity.y = 0;
			currentRollingSpeed = velocity.magnitude;
		}
		else
		{
			currentFallingSpeed = force;
			//Push out of the ground
			controller.position = controller.position + Vector3.up * (skinWidth * 2f);
		}
	}

	void MoveGeneric()
	{
		velocity = Vector3.zero;

		velocity += Vector3.forward * currentRollingSpeed;
		velocity.z = Mathf.Max(0, velocity.z);

		if (rollingSwitch)
			velocity.z = -velocity.z;

		Quaternion rot = Quaternion.FromToRotation(Vector3.forward, lastForward);
		velocity = rot * velocity;

		velocity += Vector3.down * currentFallingSpeed;
	}

	void FixRotation()
	{
		Vector3 normal = GetTerrainNormal();
		SetUp(normal);	
	}

	void SetUp(Vector3 up)
	{
		Vector3 dir = Vector3.Cross(controller.Right(), up);
		controller.rotation = Quaternion.LookRotation(dir, up);
	}

	Vector3 GetTerrainNormal()
	{
		Vector3 up = inRamp ? Vector3.up : controller.Up();

		Ray ray = new Ray(controller.position + up, -up);
		float rayLength = 1f + skinWidth;
		bool newIsGrounded = Physics.Raycast(ray, out RaycastHit hit, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp"));

		//Just left the ground
		if (isGrounded && !newIsGrounded)
			Jump();

		//Land and continue with speed
		if (wasAirborn && !isGrounded && newIsGrounded)
		{
			ray = new Ray(controller.position + controller.Up(), -controller.Up());
			Physics.Raycast(ray, out RaycastHit rampHit, rayLength, LayerMask.GetMask("Ramp"));
			currentRollingSpeed += currentFallingSpeed * Vector3.Dot(controller.Up(), rampHit.normal);
		}

		isGrounded = newIsGrounded;
		if (isGrounded)
		{
			lastForward = controller.Forward();

			inRamp = false;
			controller.position = hit.point;

			if (hit.collider.gameObject != lastObjectTouched)
			{
				lastObjectTouched = hit.collider.gameObject;
				var ramp = lastObjectTouched.GetComponent<Ramp>();
				if (ramp != null)
					lastRamp = ramp;
			}

			float angle = Vector3.Angle(lastNormal, hit.normal);
			if (!wasAirborn && angle > 45)
				Jump();
			else
			{
				lastNormal = hit.normal;
				return hit.normal;
			}
		}

		if (inRamp)
		{
			Vector3 newUp = controller.Up();
			controller.MovePosition(controller.position + newUp * (skinWidth * 2));
			newUp.y = 0;
			newUp.Normalize();
			return newUp;
		}
		else
		{
			return Vector3.up;
		}
	}

	float wallColSize = 0.2f;
	float wallColLength = 0.7f;
	Ray wallColRay = new Ray();
	void CheckForWallCollisions()
	{
		Vector3 up = controller.Up();
		wallColRay = new Ray(controller.position + up, -up);
		Vector3 forward = rollingSwitch ? -lastForward : lastForward;

		bool hitWall = Physics.CapsuleCast(wallColRay.origin, wallColRay.origin + wallColRay.direction * wallColLength, wallColSize, forward, out RaycastHit hit, wallColSize, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp"));

		//Hit a wall
		if (hitWall)
		{
			//bounce the velocity
			Vector3 newForward = Vector3.Reflect(controller.Forward(), hit.normal);
			lastForward = newForward;
			currentRollingSpeed *= 1 - Mathf.Abs(Vector3.Dot(hit.normal, controller.Forward()));
			controller.rotation = Quaternion.LookRotation(newForward, controller.Up());
		}
	}

	public void Move(Vector3 direction)
	{
		turnInput = direction.x * playerSettings.turnSpeed;
		stopping = direction.z < 0f;
	}

	private void OnDrawGizmos()
	{
		/*
		if (Application.isPlaying)
		{
			if (inRamp)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(rampLocation, 0.1f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(rampLocation, rampLocation + rampNormal);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(rampLocation, rampLocation + lastNormal);
			}
			Gizmos.DrawLine(controller.position, controller.position + (lastForward * 1.5f));

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(wallColRay.origin, wallColSize);
			Gizmos.DrawRay(wallColRay.origin, wallColRay.direction * wallColLength);
			Gizmos.DrawWireSphere(wallColRay.origin + wallColRay.direction * wallColLength, wallColSize);
		}
		*/
	}
}
