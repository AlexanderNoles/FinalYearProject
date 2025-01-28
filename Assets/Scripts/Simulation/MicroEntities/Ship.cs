using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : SimObject
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

	public override float GetMaxHealth()
	{
		return 10;
	}
}
