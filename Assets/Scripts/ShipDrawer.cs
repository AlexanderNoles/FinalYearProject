using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipDrawer : MonoBehaviour
{
	[HideInInspector]
	public new Transform transform;
	public Ship target = null;

	private void Awake()
	{
		transform = base.transform;
	}

	private void OnDisable()
	{
		target = null;
	}

	public void Link(Ship newTarget)
	{
		target = newTarget;
	}

	public void SetParent(Transform newParent)
	{
		transform.parent = newParent;
	}
}
