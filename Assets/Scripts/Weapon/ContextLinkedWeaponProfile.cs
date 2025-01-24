using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextLinkedWeaponProfile : WeaponProfile
{
	private Ship target;

	public ContextLinkedWeaponProfile SetTarget(Ship ship)
	{
		target = ship;
		return this;
	}

	public override float GetDamage()
	{
		return target.GetDamageWithVariance(false);
	}

	public override float GetTimeBetweenAttacks()
	{
		return target.GetTimeBetweenAttacks();
	}

	protected override bool AttackTimeVariance()
	{
		return true;
	}

	protected override bool SalvoEnabled()
	{
		return false;
	}
}
