using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
	public Transform target;
	public float dampening = 10;
	public float distance = 5f;
	public Vector3 cameraLocalOffset;
	public Vector3 cameraLookOffset;

	private void FixedUpdate()
	{
		Vector3 positionOffset = target.position + (transform.position - target.position).normalized * distance + cameraLocalOffset;
		Vector3 lookOffset = (target.position + cameraLookOffset) - transform.position;
		
		//Look towards the target
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookOffset, Vector3.up), dampening * Time.fixedDeltaTime);
		//Move towards/away from the target
		transform.position = Vector3.Lerp(transform.position, positionOffset, dampening * Time.fixedDeltaTime);
	}
}
