using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardMover : MonoBehaviour
{
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

	public RaycastHit? GetDownHit(Rigidbody rb)
	{
		Ray localRay = new Ray(rb.position + rb.Up(), -rb.Up());
		Ray globalRay = new Ray(rb.position + Vector3.up, -Vector3.up);
		float rayLength = 1.1f;
		if (Physics.Raycast(localRay, out RaycastHit hitLocal, rayLength, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Ramp")))
		{
			Ramp ramp = hitLocal.collider.GetComponent<Ramp>();
			if (ramp != null)
				mostRecentRamp = new KeyValuePair<Ramp, Vector3>(ramp, hitLocal.point);
			return hitLocal;
		}
		if (Physics.Raycast(globalRay, out RaycastHit hitGlobal, rayLength, LayerMask.GetMask("Terrain")))
		{
			return hitGlobal;
		}
		else
		return null;
	}
	public KeyValuePair<Ramp, Vector3> mostRecentRamp;
	public bool TouchingGround => hit.HasValue;

	void Start()
	{
		foreach(var state in states)
			state.state.enabled = false;
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
	}

	RaycastHit? hit;

	private void FixedUpdate()
	{
		if (currentState == null)
			return;

		hit = GetDownHit(Rb);

		Debug.DrawLine(transform.position, transform.position + (Rb.velocity * 10 * Time.fixedDeltaTime));

		stateHandler.SetBool("HasRamp", mostRecentRamp.Key != null);
		stateHandler.SetBool("Grounded", TouchingGround);
		stateHandler.SetFloat("XVel", Rb.velocity.x);
		stateHandler.SetFloat("YVel", Rb.velocity.y);
		stateHandler.SetFloat("ZVel", Rb.velocity.z);
		stateHandler.SetFloat("NormalTilt", Vector3.Dot(Rb.Up(), Vector3.up));
		stateHandler.SetFloat("Tilt", Vector3.Dot(Rb.Up(), Vector3.up));
	}

	public void SetStacked()
	{
		stateHandler.SetBool("Stacked", true);
		StartCoroutine(GetBackUp());
	}

	IEnumerator GetBackUp()
	{
		yield return new WaitForSeconds(3f);
		stateHandler.SetBool("Stacked", false);
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
		Vector3 currentVelocity = Vector3.zero;

		if (currentState != null)
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
