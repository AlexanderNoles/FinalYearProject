using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidShip : Ship
{
	public VoidShipWeapon weapon = new VoidShipWeapon();

	public override float GetMaxHealth()
	{
		return 25;
	}

	public override List<StandardSimWeaponProfile> GetWeapons()
	{
		return new List<StandardSimWeaponProfile>
		{
			weapon
		};
	}
}

public class VoidShipWeapon : StandardSimWeaponProfile
{
	public override float GetDamageRaw()
	{
		return 5;
	}
}
