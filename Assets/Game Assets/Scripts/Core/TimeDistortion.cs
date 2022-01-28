using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDistortion : MonoBehaviour
{
	public float timeScale = 1f;

    // Update is called once per frame
    void Update()
    {
		Time.timeScale = timeScale;
    }
}
