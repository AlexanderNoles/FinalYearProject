using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollection
{
	public int lastTickUpdateID = -1;
	public List<(UpdateType, Ship)> recordedUpdates = new List<(UpdateType, Ship)>();

	public enum UpdateType
	{
		Add,
		Remove
	}

	public void MarkCollectionUpdate(UpdateType type, Ship target)
	{
		if (lastTickUpdateID != SimulationManagement.currentTickID)
		{
			lastTickUpdateID = SimulationManagement.currentTickID;

			//Reset recorded updates
			recordedUpdates.Clear();
		}

		recordedUpdates.Add((type, target));
	}

	public virtual void OnTransfer(RealSpacePosition newPos)
	{

	}

	public virtual List<Ship> GetShips()
	{
		return null;
	}

	public virtual int GetCapacity()
	{
		return 3;
	}

	public virtual Ship GetNewShip()
	{
		return new Ship();
	}

	public virtual void AddShip(Ship newShip)
	{
		//Do nothing by default
	}

	public virtual bool TakeDamage(float damage)
	{
		List<Ship> ships = GetShips();

		bool allShipsDestroyed = true;
		foreach (Ship ship in ships)
		{
			//Don't take any damage if already destroyed
			if (!ship.isWreck)
			{
				ship.TakeDamage(damage);
			}

			//Check if it is destroyed after taking damage
			if (!ship.isWreck)
			{
				allShipsDestroyed = false;
			}
		}

		return allShipsDestroyed;
	}
}
