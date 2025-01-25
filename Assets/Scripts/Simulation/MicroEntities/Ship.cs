using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : WeaponBase
{
	public float health;
	public bool isWreck = false;

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
