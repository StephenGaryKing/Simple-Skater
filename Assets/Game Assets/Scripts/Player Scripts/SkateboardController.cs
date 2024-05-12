using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{
	[SerializeField] Rigidbody _rb;
	[SerializeField] float _pushForce;
	[SerializeField] float _rotationAmout;

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKey(KeyCode.W))
		{
			_rb.AddForce(transform.forward * _pushForce);
		}

		if (Input.GetKey(KeyCode.S))
		{
			_rb.AddForce(transform.forward * -_pushForce);
		}

		if (Input.GetKey(KeyCode.A))
		{
			_rb.MoveRotation(transform.rotation * Quaternion.Euler(transform.up * _rotationAmout));
		}

		if (Input.GetKey(KeyCode.D))
		{
			_rb.MoveRotation(transform.rotation * Quaternion.Euler(transform.up * -_rotationAmout));
		}
	}
}
