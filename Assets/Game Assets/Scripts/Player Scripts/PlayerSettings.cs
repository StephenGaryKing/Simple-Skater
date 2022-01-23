using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Settings", menuName = "Player/Settings")]
public class PlayerSettings : ScriptableObject
{
	public float visualRotationDampening = 0.1f;
	public float jumpForce = -3f;
	public float pushForce = 5f;
	public float maxPushForce = 8f;
	public float turnSpeed = 0.1f;
}
