using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MilitaryData : DataBase
{
	public float totalDamageBuildup = 0;

	public float maxMilitaryCapacity;
	public int currentFleetCount;
	public Dictionary<RealSpacePostion, List<ShipCollection>> positionToFleets = new Dictionary<RealSpacePostion, List<ShipCollection>>();
	public List<(RealSpacePostion, RealSpacePostion)> markedTransfers = new List<(RealSpacePostion, RealSpacePostion)>();

	public void MarkTransfer(RealSpacePostion from, RealSpacePostion to)
	{
		markedTransfers.Add((from, to));
	}

	public void AddFleet(RealSpacePostion pos, Fleet fleet)
	{
		if (!positionToFleets.ContainsKey(pos))
		{
			positionToFleets.Add(pos, new List<ShipCollection>());
		}

		positionToFleets[pos].Add(fleet);
		currentFleetCount++;
	}

	public Fleet RemoveFleet(RealSpacePostion pos, Fleet fleet)
	{
		Fleet toReturn = null;

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
			currentFleetCount--;
		}

		return toReturn;
	}

	public Fleet RemoveFleet(RealSpacePostion pos)
	{
		return RemoveFleet(pos, null);
	}

	public int TransferFreeUnits(int budget, RealSpacePostion target, BattleData battleData, int budgetMinimum = 0, bool allowRetreat = false)
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
		List<(RealSpacePostion, int)> fromPositions = new List<(RealSpacePostion, int)>();

		foreach (KeyValuePair<RealSpacePostion, List<ShipCollection>> fleet in positionToFleets)
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

		foreach ((RealSpacePostion, int) entry in fromPositions)
		{
			for (int i = 0; i < entry.Item2; i++)
			{
				//Remove any fleet from previous cell
				Fleet transferredFleet = RemoveFleet(entry.Item1);

				//Add fleet to new cell
				AddFleet(target, transferredFleet);

				fleetTransferredCount++;
			}

			//Mark a transfer
			MarkTransfer(entry.Item1, target);
		}

		return fleetTransferredCount;
	}
}
