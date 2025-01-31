using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateOverride : MonoBehaviour
{
	public KeyCode inputKey = KeyCode.Escape;
	public UnityEvent onKeyPress = new UnityEvent();

	protected virtual void OnEnable()
	{
		UIManagement.AddStateOverride(this);
	}

	protected virtual void OnDisable()
	{
		UIManagement.RemoveStateOverride(this);
	}

	public void Process()
	{
		onKeyPress.Invoke();
	}
}
