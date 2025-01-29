using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardSimWeaponProfile : WeaponProfile
{
	public override float GetTimeBetweenAttacks()
	{
		return 1.0f;
	}

	public float GetDamage(bool lazy)
	{
		if (lazy)
		{
			return GetDamageLazy();
		}

		return GetDamage();
	}

	public override float GetDamage()
	{
		return GetDamageRaw() * Random.Range(0.95f, 1.05f); ;
	}

	public float GetDamageLazy()
	{
		return GetDamageRaw() + ((SimulationManagement.random.Next(-100, 101) / 100.0f) * 0.05f);
	}

	public virtual float GetDamageRaw()
	{
		return 1.0f;
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
