using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardMover : MonoBehaviour
{
	public Animator stateHandler;
	public Animator animations;
	public SphereCollider collider;

	[System.Serializable]
	public class State
	{
		public string stateName;
		public MovementState state;
	}
	private State currentState;
	[SerializeField]
	float stackForce;
	[SerializeField]
	public List<State> states;

	Transform currentCamera = null;
	Rigidbody Rb => currentState.state.rb;

	public RaycastHit? GetDownHit(Rigidbody rb)
	{
		Ray localRay = new Ray(rb.position + rb.Up(), -rb.Up());
		float radius = collider.radius + 0.05f;
		float rayLength = 1f;

		var endPoint = localRay.origin + (localRay.direction * rayLength);
		Debug.DrawLine(localRay.origin, endPoint, Color.blue);
		Debug.DrawLine(localRay.origin, localRay.origin + (rb.Up() * radius), Color.red);
		Debug.DrawLine(endPoint, endPoint + (-rb.Up() * radius), Color.red);

		if (Physics.SphereCast(localRay, radius, out RaycastHit hitLocal, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
		{
			Ramp ramp = hitLocal.collider.GetComponent<Ramp>();
			if (ramp != null)
			{
				CalculateRampInfo();
				mostRecentRamp = new KeyValuePair<Ramp, Vector3>(ramp, rampLocation);
			}
			Hit = hitLocal;
			return hitLocal;
		}
		else
			return null;
	}
	public KeyValuePair<Ramp, Vector3> mostRecentRamp;
	public bool TouchingGround => Hit.HasValue;
	bool jumping = false;

	public Vector3 rampLocation;
	public Vector3 rampTangent;
	public Vector3 rampNormal;
	public float distance;
	public void CalculateRampInfo()
	{
		Ramp lastRamp = mostRecentRamp.Key;
		if (lastRamp != null)
		{
			rampLocation = lastRamp.curveMath.CalcPositionByClosestPoint(Rb.position, out distance);
			rampTangent = lastRamp.curveMath.CalcTangentByDistance(distance);
			rampNormal = Vector3.Cross(Vector3.up, rampTangent);
		}
	}

	void Start()
	{
		foreach (var state in states)
			state.state.enabled = false;

		InputManagement.Jump += () => jumping = true;
	}

	void Update()
	{
		var moveVector = InputManagement.Move;

		//consume the jump input on this frame
		stateHandler.SetBool("Jumping", jumping);
		jumping = false;

		if (Input.GetKeyDown(KeyCode.Escape))
			stateHandler.SetBool("NoClip", !stateHandler.GetBool("NoClip"));

		foreach (var state in states)
		{
			//When entering the state in the Animator, switch to that state in the internal states
			if (stateHandler.GetCurrentAnimatorStateInfo(0).IsName(state.stateName))
			{
				SwitchStates(state);
			}
		}

		float pushPower = TouchingGround? Mathf.Clamp(Rb.velocity.magnitude / 4, 0, 1) : 0;

		var leanAmount = moveVector.x;

		animations.SetFloat("Lean", leanAmount);
		animations.SetFloat("Power", pushPower);

		if (moveVector.y > 0)
			animations.SetBool("Push", true);
		else
			animations.SetBool("Push", false);

		animations.SetBool("TouchingGround", TouchingGround);

		stateHandler.SetBool("InVert", currentState.stateName == "Transition" || currentState.stateName == "VertJumping");

		currentState.state.GatherInput();
	}

	public RaycastHit? Hit;

	private void FixedUpdate()
	{
		Rb.angularVelocity = Vector3.zero;
		if (currentState == null)
			return;

		Hit = GetDownHit(Rb);

		//Debug.DrawLine(rb.position, rb.position + (Rb.velocity * 10 * Time.fixedDeltaTime));

		stateHandler.SetBool("HasRamp", mostRecentRamp.Key != null);
		stateHandler.SetBool("CloseToRamp", mostRecentRamp.Key != null && Vector3.Distance(mostRecentRamp.Value, Rb.position) < 1f);
		stateHandler.SetBool("Grounded", TouchingGround);
		stateHandler.SetFloat("XVel", Rb.velocity.x);
		stateHandler.SetFloat("YVel", Rb.velocity.y);
		stateHandler.SetFloat("ZVel", Rb.velocity.z);
		stateHandler.SetFloat("Tilt", Vector3.Dot(Rb.Up(), Vector3.up));

		currentState.state.Rotate();
		currentState.state.Move();
	}

	public void SetStacked()
	{
		animations.SetBool("Stacked", true);
		stateHandler.SetBool("Stacked", true);
		StartCoroutine(GetBackUp());
	}

	IEnumerator GetBackUp()
	{
		yield return new WaitForSeconds(3f);
		stateHandler.SetBool("Stacked", false);
		animations.SetBool("Stacked", false);
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

		if (newState.state.cameraLogic != null)
		{
			Vector3 startPos = transform.position;
			Quaternion startRot = transform.rotation;
			if (currentCamera != null)
			{
				startPos = currentCamera.position;
				startRot = currentCamera.rotation;
				currentCamera.gameObject.SetActive(false);
			}
			currentCamera = newState.state.cameraLogic;
			currentCamera.gameObject.SetActive(true);
			currentCamera.position = startPos;
			currentCamera.rotation = startRot;
		}

		Debug.Log($"State Change: {newState.stateName}");
		Vector3 currentVelocity = Vector3.zero;

		if (currentState?.state != null)
		{
			currentVelocity = currentState.state.GetVelocity();
			currentState.state.Exit();
			currentState.state.enabled = false;
		}
		newState.state.enabled = true;
		newState.state.Enter(currentVelocity);
		currentState = newState;
	}
}
