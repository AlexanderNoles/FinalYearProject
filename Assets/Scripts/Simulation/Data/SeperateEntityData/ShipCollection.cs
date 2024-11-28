using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollection
{
	public virtual List<Ship> GetShips()
	{
		return null;
	}

	public virtual bool TakeDamage(float damage)
	{
		List<Ship> ships = GetShips();

		bool allShipsDestroyed = true;
		foreach (Ship ship in ships)
		{
			if (!ship.destroyed)
			{
				ship.TakeDamage(damage);
			}

			if (!ship.destroyed)
			{
				allShipsDestroyed = false;
			}
		}

		return allShipsDestroyed;
	}
}
