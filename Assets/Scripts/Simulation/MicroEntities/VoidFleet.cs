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
		float distancePercentage = 1.0f - (float)(origin.SubtractToClone(newPos).Magnitude() / WorldManagement.GetSolarSystemRadius());

		distancePercentage = Mathf.Clamp01(Mathf.Pow(distancePercentage, BalanceManagement.voidSwarmDamageFalloff));

		foreach (VoidShip ship in ships.Cast<VoidShip>())
		{
			ship.SetDamage(Mathf.Max(distancePercentage * 5.0f, 0.005f));
		}
	}
}
