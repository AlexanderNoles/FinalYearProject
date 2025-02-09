using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MilitaryData : DataModule
{
	//Can this military be controlled by other entites? i.e., can other entites force it to start battles (this is specifically towards forcing them to inititate a battle
	//NOT making them join a battle as defence). Currently (07/02/2025) this is only used by the player because we don't want the player's military randomly joining battles the player
	//doesn't know about. In the future this could also be used by a pacifist faction to avoid conflict for example (but still be able to act in defence)
	public bool selfControlled = false;

	public RealSpacePosition origin;
	public float totalDamageBuildup = 0;
	public int initalCount = 0;

	public float maxMilitaryCapacity;
	public int currentFleetCount;
	//Fleets that don't neccesarily have a defined position
	//A key example is the player's faction that keeps ships onboard it's moving vessel
	public List<ShipCollection> reserveFleets = new List<ShipCollection>();
	public Dictionary<RealSpacePosition, List<ShipCollection>> positionToFleets = new Dictionary<RealSpacePosition, List<ShipCollection>>();
	public Dictionary<RealSpacePosition, List<ShipCollection>> fromTransfer = new Dictionary<RealSpacePosition, List<ShipCollection>>();
	public Dictionary<RealSpacePosition, List<ShipCollection>> toTransfer = new Dictionary<RealSpacePosition, List<ShipCollection>>();

	public void MarkTransfer(RealSpacePosition from, RealSpacePosition to, ShipCollection target)
	{
		//From
		if (from != null)
		{
			if (!fromTransfer.ContainsKey(from))
			{
				fromTransfer.Add(from, new List<ShipCollection>());
			}

			fromTransfer[from].Add(target);
		}

		//To
		if (to != null)
		{
			if (!toTransfer.ContainsKey(to))
			{
				toTransfer.Add(to, new List<ShipCollection>());
			}

			toTransfer[to].Add(target);
		}
	}

	public virtual ShipCollection GetNewFleet()
	{
		return new Fleet();
	}

	public void AddFleetToReserves(ShipCollection fleet)
	{
		reserveFleets.Add(fleet);
		currentFleetCount++;
	}

	public bool RemoveFleetFromReserves(ShipCollection fleet)
	{
		if (reserveFleets.Remove(fleet))
		{
			currentFleetCount--;
			return true;
		}

		return false;
	}

	public void AddFleet(RealSpacePosition pos, ShipCollection fleet)
	{
		if (!positionToFleets.ContainsKey(pos))
		{
			positionToFleets.Add(pos, new List<ShipCollection>());
		}

		positionToFleets[pos].Add(fleet);

		//Mark a transfer
		MarkTransfer(null, pos, fleet);
		fleet.OnTransfer(pos);
		currentFleetCount++;
	}

	public ShipCollection RemoveFleet(RealSpacePosition pos, ShipCollection fleet)
	{
		ShipCollection toReturn = null;

		if (positionToFleets.ContainsKey(pos))
		{
			if (fleet == null)
			{
				//Should always be at least one in the cell
				//Otherwise the cell should have been removed
				toReturn = positionToFleets[pos][0] as Fleet;
				positionToFleets[pos].RemoveAt(0);
			}
			else
			{
				if (positionToFleets[pos].Remove(fleet))
				{
					toReturn = fleet;
				}
			}

			if (positionToFleets[pos].Count == 0)
			{
				//No remaing ships in this position
				positionToFleets.Remove(pos);
			}
		}

		if (toReturn != null)
		{
			//Mark a transfer
			MarkTransfer(pos, null, toReturn);
			currentFleetCount--;
		}

		return toReturn;
	}

	public ShipCollection RemoveFleet(RealSpacePosition pos)
	{
		return RemoveFleet(pos, null);
	}

	public int TransferFreeUnits(int budget, RealSpacePosition target, BattleData battleData, int budgetMinimum = 0, bool allowRetreat = false)
	{
		int fleetTransferredCount = 0;

		//Reduce budget if we already have ships there
		if (positionToFleets.ContainsKey(target))
		{
			budget -= positionToFleets[target].Count;
		}

		budget = Mathf.Max(budget, 0);

		//Move an amount of ships to this cell

		//We need to identify which ships are not currently engaged in a battle
		//So we scan through the current avliable military and check if their are already positioned at a battle, if they are not we move them to 
		//the new cell until they we meet our expected fleet amount (or we run out of fleets to send)
		List<(RealSpacePosition, int)> fromPositions = new List<(RealSpacePosition, int)>();

		foreach (KeyValuePair<RealSpacePosition, List<ShipCollection>> fleet in positionToFleets)
		{
			if (!battleData.positionToOngoingBattles.ContainsKey(fleet.Key) || allowRetreat)
			{
				//Not currently in a battle (or we are allowed to retreat)
				//Transfer ships out to new target cell

				//How many do we want to transfer
				//Don't want to exceed budget or amount of ships stored at this cell
				int transferLimit = Mathf.Min(budget, fleet.Value.Count);
				fromPositions.Add((fleet.Key, transferLimit));
				budget -= transferLimit;
			}

			if (budget <= 0)
			{
				break;
			}
		}

		foreach ((RealSpacePosition, int) entry in fromPositions)
		{
			for (int i = 0; i < entry.Item2; i++)
			{
				//Remove any fleet from previous cell
				ShipCollection transferredFleet = RemoveFleet(entry.Item1);

				//Add fleet to new cell
				AddFleet(target, transferredFleet);

				fleetTransferredCount++;
			}
		}

		return fleetTransferredCount;
	}

	public override string Read()
	{
		int shipCount = 0;

		foreach (List<ShipCollection> shipCollections in positionToFleets.Values)
		{
			foreach (ShipCollection collection in shipCollections)
			{
				shipCount += collection.GetShips().Count;
			}
		}

		return $"	Fleet Count: {currentFleetCount}\n" +
			$"	Ship Count: {shipCount}";
	}
}
