using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMover : MonoBehaviour
{
	[SerializeField] Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
		int v = 0;
		int h = 0;

		if (Input.GetKeyDown(KeyCode.W))
			v += 1;

		if (Input.GetKeyDown(KeyCode.S))
			v -= 1;

		if (Input.GetKeyDown(KeyCode.A))
			h -= 1;

		if (Input.GetKeyDown(KeyCode.D))
			h += 1;

		Vector3 dir = new Vector3(h, 0, v);

		rb.AddForce(dir * 5, ForceMode.VelocityChange);
    }
}
