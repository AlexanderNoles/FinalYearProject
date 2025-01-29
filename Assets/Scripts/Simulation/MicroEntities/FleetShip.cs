using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetShip : Ship
{
	public StandardSimWeaponProfile weapon = new FleetWeapon();

	public override List<StandardSimWeaponProfile> GetWeapons()
	{
		return new List<StandardSimWeaponProfile>
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

public class FleetWeapon : StandardSimWeaponProfile
{
	public override float GetDamageRaw()
	{
		//Static value for now
		return 3;
	}
}
