using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDistortion : MonoBehaviour
{
	[Range(0.01f, 1f)]
	public float timeScale = 1f;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKey(KeyCode.F))
			Time.timeScale = timeScale;
		else
			Time.timeScale = 1;
	}
}
