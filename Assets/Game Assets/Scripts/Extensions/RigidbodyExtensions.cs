using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyExtensions
{
    public static Vector3 Right(this Rigidbody rb)
	{
		return (rb.rotation * Vector3.right).normalized;
	}

	public static Vector3 Up(this Rigidbody rb)
	{
		return (rb.rotation * Vector3.up).normalized;
	}

	public static Vector3 Forward(this Rigidbody rb)
	{
		return (rb.rotation * Vector3.forward).normalized;
	}
}
