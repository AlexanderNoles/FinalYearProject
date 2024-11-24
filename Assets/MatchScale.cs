using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchScale : MonoBehaviour
{
	public Transform target;
	public float multiplier = 1;
	private new Transform transform;

	private void Awake()
	{
		transform = base.transform;
	}

	private void LateUpdate()
	{
		transform.localScale = target.localScale * multiplier;
	}
}
