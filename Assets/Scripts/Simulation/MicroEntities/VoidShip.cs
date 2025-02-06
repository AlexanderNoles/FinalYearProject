using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidShip : Ship
{
	public float calcualtedDamage = 0;
	public VoidShipWeapon weapon = new VoidShipWeapon();

	public void SetDamage(float newDamage)
	{
		calcualtedDamage = MathHelper.ValueTanhFalloff(Mathf.Abs(newDamage), 10, -1);
	}

	public override float GetMaxHealth()
	{
		return 25;
	}

	public VoidShip()
	{
		weapon.target = this;
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
	public VoidShip target;

	public override float GetDamageRaw()
	{
		return target.calcualtedDamage;
	}
}
