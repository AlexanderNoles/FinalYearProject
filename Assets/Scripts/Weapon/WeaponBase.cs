using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase
{
	public virtual float GetDamageRaw()
	{
		return 0;
	}

	public virtual float GetTimeBetweenAttacks()
	{
		return 1.0f;
	}

	public virtual float GetDamageWithVariance(bool lazy = true)
	{
		if (lazy)
		{
			return GetDamageRaw() + ((SimulationManagement.random.Next(-100, 101) / 100.0f) * 0.05f);
		}
		else
		{
			return GetDamageRaw() * Random.Range(0.95f, 1.05f);
		}
	}
}
