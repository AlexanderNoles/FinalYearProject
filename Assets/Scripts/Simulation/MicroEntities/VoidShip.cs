using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidShip : FleetShip
{
	public float calcualtedDamage = 0;

	public void SetDamage(float newDamage)
	{
		calcualtedDamage = MathHelper.ValueTanhFalloff(Mathf.Abs(newDamage), 3, -1);
	}
}
