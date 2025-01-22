using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipDrawer : MonoBehaviour
{
	[HideInInspector]
	public new Transform transform;
	public Ship target = null;
	private SimulationEntity parentEntity;

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

	public void Init(SimulationEntity entity)
	{
		parentEntity = entity;

		//Set position
		Vector3 newShipPosition = Random.onUnitSphere;
		newShipPosition.y = 0.0f;
		newShipPosition.Normalize();

		transform.position = newShipPosition * Random.Range(10.0f, 100.0f);
	}
}
