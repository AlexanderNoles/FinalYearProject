using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MilitaryData : DataBase
{
	public int currentFleetCount;
	public Dictionary<RealSpacePostion, List<ShipCollection>> cellCenterToFleets = new Dictionary<RealSpacePostion, List<ShipCollection>>();

	public void AddFleet(RealSpacePostion pos, Fleet fleet)
	{
		if (!cellCenterToFleets.ContainsKey(pos))
		{
			cellCenterToFleets.Add(pos, new List<ShipCollection>());
		}

		cellCenterToFleets[pos].Add(fleet);
		currentFleetCount++;
	}

	public Fleet RemoveFleet(RealSpacePostion pos, Fleet fleet)
	{
		if (cellCenterToFleets.ContainsKey(pos))
		{
			if (cellCenterToFleets[pos].Remove(fleet))
			{
				currentFleetCount--;
				return fleet;
			}
		}

		return null;
	}

	public Fleet RemoveFleet(RealSpacePostion pos)
	{
		if (cellCenterToFleets.ContainsKey(pos))
		{
			if (cellCenterToFleets[pos].Count > 0)
			{
				Fleet fleet = cellCenterToFleets[pos][0] as Fleet;
				cellCenterToFleets[pos].RemoveAt(0);

				return fleet;
			}
		}

		return null;
	}

	public void TransferFreeFleets(int budget, RealSpacePostion target, BattleData battleData, int budgetMinimum = 0)
	{
		//Reduce budget if we already have ships there
		if (cellCenterToFleets.ContainsKey(target))
		{
			budget -= cellCenterToFleets[target].Count;
		}

		budget = Mathf.Max(budget, 0);

		//Move an amount of ships to this cell

		//We need to identify which ships are not currently engaged in a battle
		//So we scan through the current avliable military and check if their are already positioned at a battle, if they are not we move them to 
		//the new cell until they we meet our expected fleet amount (or we run out of fleets to send)
		int controlledAreasCount = cellCenterToFleets.Count;

		for (int f = 0; f < controlledAreasCount && budget > 0; f++)
		{
			KeyValuePair<RealSpacePostion, List<ShipCollection>> fleet = cellCenterToFleets.ElementAt(f);

			if (!battleData.ongoingBattles.ContainsKey(fleet.Key))
			{
				//Not currently in a battle
				//Transfer ships out to new target cell

				//How many do we want to transfer
				//Don't want to exceed budget or amount of ships stored at this cell
				int transferLimit = Mathf.Min(budget, fleet.Value.Count);

				for (int i = 0; i < transferLimit; i++)
				{
					//Remove any fleet from previous cell
					Fleet transferredFleet = RemoveFleet(fleet.Key);

					//Add fleet to new cell
					AddFleet(target, transferredFleet);
					budget--;
				}
			}
		}
	}
}
