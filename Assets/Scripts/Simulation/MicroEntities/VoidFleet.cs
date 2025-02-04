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

	public override void OnTransfer(RealSpacePosition newPos)
	{
		float distance = 1.0f - (float)(origin.SubtractToClone(newPos).Magnitude() / WorldManagement.GetSolarSystemRadius());

		foreach (VoidShip ship in ships.Cast<VoidShip>())
		{
			ship.SetDamage(Mathf.Max(distance * 3.0f, 0.05f));
		}
	}
}
