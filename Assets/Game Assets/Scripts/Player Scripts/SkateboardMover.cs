using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardMover : MonoBehaviour, IMover
{
	Rigidbody controller;
	Vector3 velocity = Vector3.zero;

	public Animator stateManager;
	public float skinWidth = 0.1f;

	public float jumpForce = 10f;
	public float pushForce = 10f;
	public float maxPushSpeed = 10f;
	public float turnSpeed = 10f;
	public float leavingGroundThreashold = 0.5f;
	public bool rollingSwitch = false;

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

	void Start()
	{
		controller = GetComponent<Rigidbody>();
	}

	void Update()
	{
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
	}

	private void FixedUpdate()
	{
		Vector3 m_EulerAngleVelocity = new Vector3(0, turnInput * (wasAirborn? 2000f : 1000f), 0);
		Quaternion q = controller.rotation * Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);

		controller.MoveRotation(q);
		controller.MovePosition(controller.position + (velocity * Time.fixedDeltaTime));
	}

	Vector3 rampLocation;
	Vector3 rampTangent;
	Vector3 rampNormal;
	Vector3 flattenScale = new Vector3(1, 0, 1);
	void MoveOnRamp()
	{
		rampLocation = lastRamp.curveMath.CalcPositionByClosestPoint(controller.position, out float distance);

		if (distance < 0.5f || distance > lastRamp.curveMath.GetDistance(lastRamp.curve.PointsCount - 1) - 0.5f || Vector3.Distance(Vector3.Scale(controller.position, flattenScale), Vector3.Scale(rampLocation, flattenScale)) > 1f)
		{
			inRamp = false;
			return;
		}

		rampTangent = lastRamp.curveMath.CalcTangentByDistance(distance);
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(rampLocation, 0.1f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(rampLocation, rampLocation + rampNormal);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(rampLocation, rampLocation + lastNormal);
		Gizmos.DrawLine(transform.position, transform.position + (lastForward * 1.5f));
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
			currentRollingSpeed -= pushForce * Time.deltaTime;
		else if (currentRollingSpeed < maxPushSpeed)
		{
			currentRollingSpeed = Mathf.Max(0, currentRollingSpeed);
			currentRollingSpeed += pushForce * Time.deltaTime;
		}
		currentRollingSpeed = Mathf.Clamp(currentRollingSpeed, -maxPushSpeed, maxPushSpeed);

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
		wasAirborn = false;

		currentFallingSpeed = 0f;

		if (shouldJump)
		{
			Jump();
		}

		MoveGeneric();
	}

	void Jump()
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
			currentFallingSpeed += -velocity.y * 1.5f;
			velocity.y = 0;
			currentRollingSpeed = velocity.magnitude;
		}
		else
		{
			currentFallingSpeed = jumpForce;
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

		Debug.DrawLine(transform.position, transform.position + up, Color.green);
		Debug.DrawLine(transform.position, transform.position + dir, Color.blue);

		controller.rotation = Quaternion.LookRotation(dir, up);
	}

	Vector3 GetTerrainNormal()
	{
		Vector3 up = inRamp ? Vector3.up : controller.Up();

		Ray ray = new Ray(controller.position + up, -up);
		float rayLength = 1f + skinWidth;
		Debug.DrawLine(ray.origin, ray.origin + (ray.direction * rayLength), Color.red);

		bool newIsGrounded = Physics.Raycast(ray, out RaycastHit hit, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp"));
		Debug.DrawLine(ray.origin, ray.origin + (ray.direction * rayLength), Color.red);

		//Just left the ground
		if (isGrounded && !newIsGrounded)
			Jump();

		//Land and continue with speed
		if (wasAirborn && !isGrounded && newIsGrounded)
		{
			ray = new Ray(controller.position + controller.Up(), -controller.Up());
			Physics.Raycast(ray, out RaycastHit rampHit, rayLength, LayerMask.GetMask("Ramp"));
			Debug.DrawLine(ray.origin, ray.origin + (ray.direction * rayLength), Color.red);
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

	public void Move(Vector3 direction)
	{
		turnInput = direction.x * turnSpeed;
		stopping = direction.z < 0f;
	}
}
