using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : CelestialBody
{
	public Transform parent;

	public override Transform GetInWorldParent()
	{
		return parent;
	}
}
