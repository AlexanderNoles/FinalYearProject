using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetShip : Ship
{
	public WeaponBase weapon = new FleetWeapon();

	public override List<WeaponBase> GetWeapons()
	{
		return new List<WeaponBase>
		{
			weapon
		};
	}

	public override float GetMaxHealth()
	{
		//Static value for now
		return 25;
	}
}

public class FleetWeapon : WeaponBase
{
	public override float GetDamageRaw()
	{
		//Static value for now
		return 3;
	}
}
