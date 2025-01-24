using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship
{
	public float health;
	public bool isWreck = false;

	public virtual float GetDamgeRaw()
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
			return GetDamgeRaw() + ((SimulationManagement.random.Next(-100, 101) / 100.0f) * 0.05f);
		}
		else
		{
			return GetDamgeRaw() * Random.Range(0.95f, 1.05f);
		}
	}

	public virtual void TakeDamage(float damage)
	{
		health -= damage;

		if (health <= 0)
		{
			isWreck = true;
		}
	}

	public virtual float GetMaxHealth()
	{
		return 10;
	}
}
