using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoidFleet : Fleet
{
	public RealSpacePosition origin;

	public override int GetCapacity()
	{
		return 4;
	}

	public override Ship GetNewShip()
	{
		return new VoidShip();
	}
}
