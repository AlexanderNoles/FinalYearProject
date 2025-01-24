using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetShip : Ship
{
	//Get base damage
	public override float GetDamgeRaw()
	{
		//Static value for now
		return 3;
	}

	public override float GetTimeBetweenAttacks()
	{
		return base.GetTimeBetweenAttacks();
	}

	public override float GetMaxHealth()
	{
		//Static value for now
		return 25;
	}
}
