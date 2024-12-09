using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostTickUpdate : MonoBehaviour
{
	protected int lastTickUpdatedOn;

	private void OnEnable()
	{
		lastTickUpdatedOn = -1;
	}

	private void Update()
	{
		if (SimulationManagement.currentTickID != lastTickUpdatedOn)
		{
			lastTickUpdatedOn = SimulationManagement.currentTickID;
			PostTick();
		}
	}

	protected virtual void PostTick()
	{

	}
}
