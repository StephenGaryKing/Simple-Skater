using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMover
{
	void Move(Vector3 direction);
}

public interface IInputManager
{
	Vector3 GetInput();
}
