using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagement : MonoBehaviour
{
	public static PlayerControls Input;

	//Helpers
	public static Vector2 Move => Input.Player.Move.ReadValue<Vector2>();
	public static bool Crouch => Input.Player.Crouch.ReadValue<float>() > 0;
	public static Action Jump;

	void Awake()
    {
		Input = new PlayerControls();
		Input.Enable();

		Input.Player.Jump.performed += context => Jump?.Invoke();
    }
}
