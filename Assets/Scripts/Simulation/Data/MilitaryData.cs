using EntityAndDataDescriptor;
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

	public List<ShipCollection> retreatBuffer = new List<ShipCollection>();

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

	public int FullyRepairedFleetsCount(List<ShipCollection> input)
	{
		//Dirty, wasteful but ensures number calculated always matches (even if we change the code)
		return GetFullyRepairedFleets(input).Count;
	}

	public List<ShipCollection> GetFullyRepairedFleets(List<ShipCollection> input)
	{
		List<ShipCollection> toReturn = new List<ShipCollection>();

		foreach (ShipCollection fleet in input)
		{
			List<Ship> ships = fleet.GetShips();

			bool validFleet = true;
			foreach (Ship sh in ships)
			{
				if (sh.health < sh.GetMaxHealth())
				{
					validFleet = false;
					break;
				}
			}

			if (validFleet)
			{
				toReturn.Add(fleet);
			}
		}

		return toReturn;
	}

	public void AddFleetToReserves(ShipCollection fleet)
	{
		reserveFleets.Add(fleet);
		currentFleetCount++;
	}

	public ShipCollection RemoveFleetFromReserves()
	{
		if (reserveFleets.Count > 0)
		{
			ShipCollection toReturn = reserveFleets[0];
			reserveFleets.RemoveAt(0);
			currentFleetCount--;

			return toReturn;
		}

		return null;
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

	public void AddFleetToRetreatBuffer(ShipCollection fleet)
	{
		retreatBuffer.Add(fleet);
		currentFleetCount++;
	}

	public ShipCollection RemoveNextFleetFromRetreatBuffer()
	{
		if (retreatBuffer.Count > 0)
		{
			ShipCollection toReturn = retreatBuffer[0];
			retreatBuffer.RemoveAt(0);
			currentFleetCount--;

			return toReturn;
		}

		return null;
	}

	public int TransferFreeUnits(int budget, RealSpacePosition target)
	{
		//Reduce budget if we already have ships there
		if (positionToFleets.ContainsKey(target))
		{
			budget -= positionToFleets[target].Count;
		}

		budget = Mathf.Max(budget, 0);

		//Move an amount of ships to this cell

		//Find troops that are currently avaliable
		//This means ships at refineries that are fully repaired
		//If we have direct refinery we can just pull from there
		//Otherwise we should scan through settlements as those each have a refinery attached

		List<RealSpacePosition> fromPositions = GetSafetyPositions();

		//These are all the fleets we want to transfer
		List<ShipCollection> foundFleets = new List<ShipCollection>();

		foreach (RealSpacePosition pos in fromPositions)
		{
			//I think this can happen when 
			if (pos == null)
			{
				continue;
			}

			if (positionToFleets.ContainsKey(pos))
			{
				//Iterate over all fleets, finding fully repaired ones up to budget cap
				List<ShipCollection> ships = positionToFleets[pos];

				int shipsRemoved = 0;
				for (int i = 0; i < ships.Count && budget > 0;)
				{
					if (ships[i].FullyRepaired())
					{
						foundFleets.Add(ships[i]);
						ships.RemoveAt(i);

						shipsRemoved++;
						budget--;
					}
					else
					{
						i++;
					}
				}

				//Because we don't directly use the remove function we must adjust some things here
				currentFleetCount -= shipsRemoved;

				//no ships left at position
				if (ships.Count == 0)
				{
					positionToFleets.Remove(pos);
				}
			}

			if (budget <= 0)
			{
				break;
			}
		}

		//Add all the found ships to the desired position
		int fleetTransferredCount = foundFleets.Count;
		for (int i = 0; i < fleetTransferredCount; i++)
		{
			AddFleet(target, foundFleets[i]);
		}

		foundFleets.Clear();

		return fleetTransferredCount;
	}

	public override string Read()
	{
		int shipCount = 0;
		int wreckCount = 0;
		string healthReadouts = "";
		healthReadouts += "		Normal:\n";

		foreach (List<ShipCollection> shipCollections in positionToFleets.Values)
		{
			foreach (ShipCollection collection in shipCollections)
			{
				shipCount += collection.GetShips().Count;

				foreach (Ship ship in collection.GetShips())
				{
					if (ship.isWreck)
					{
						wreckCount++;
					}
					else
					{
						healthReadouts += $"		Health: {ship.health}\n";
					}
				}
			}
		}

		healthReadouts += "		Reserve:\n";
		foreach (ShipCollection reserve in reserveFleets)
		{
			shipCount += reserve.GetShips().Count;

			foreach (Ship ship in reserve.GetShips())
			{
				if (ship.isWreck)
				{
					wreckCount++;
				}
				else
				{
					healthReadouts += $"		Health: {ship.health}\n";
				}
			}
		}

		return
			$"	Max Military Capacity: {maxMilitaryCapacity}\n" +
			$"	Total Damage Buildup: {totalDamageBuildup}\n" +
			$"	Fleet Count: {currentFleetCount}\n" +
			$"	In Reserves Count: {reserveFleets.Count}\n" +
			$"	Ship Count: {shipCount} | Wreck Count: {wreckCount}\n";// +
			//ealthReadouts;
	}
}
