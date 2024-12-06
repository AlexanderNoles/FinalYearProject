using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchRotation : MonoBehaviour
{
	public Transform target;

	[HideInInspector]
	public new Transform transform;

	private void Awake()
	{
		transform = base.transform;
	}

	void LateUpdate()
    {
        transform.rotation = target.rotation;
    }
}
