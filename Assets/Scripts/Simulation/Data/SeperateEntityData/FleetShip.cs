using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetShip : Ship
{
	//Get base damage
	public override float GetDamgeRaw()
	{
		//Static value for now
		return 30;
	}

	public override float GetMaxHealth()
	{
		//Static value for now
		return 50;
	}
}
