using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : MonoBehaviour, IInputManager
{
	IMover[] movers;

	private void Start()
	{
		movers = GetComponentsInChildren<IMover>();
	}

	void Update()
    {
		foreach (var mover in movers)
			mover.Move(GetInput());
	}

	public Vector3 GetInput()
	{
		return (Input.GetAxisRaw("Horizontal") * Vector3.right + Input.GetAxisRaw("Vertical") * Vector3.forward);
	}
}
