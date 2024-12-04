using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : CelestialBody
{
	private Transform parent;

	private void Start()
	{
		//Set parent
		parent = transform.parent;

		//remove parent
		transform.parent = SurroundingsRenderingManagement.mainTransform;
	}

	public override Transform GetInWorldParent()
	{
		return parent;
	}
}
