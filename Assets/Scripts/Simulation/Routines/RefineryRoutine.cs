using EntityAndDataDescriptor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(28)]
public class RefineryRoutine : RoutineBase
{
	public override void Run()
	{
		List<DataModule> refineries = SimulationManagement.GetDataViaTag(DataTags.Refinery);

		foreach (RefineryData refinery in refineries.Cast<RefineryData>())
		{
			//Produce some amount of things per tick based on context of refinery
			refinery.TryGetLinkedData(DataTags.Military, out MilitaryData milData);
			ProcessRefinery(refinery, milData);
		}
	}

	public static void ProcessRefinery(RefineryData refinery, MilitaryData militaryData)
	{
		//Military production
		if (militaryData != null)
		{
			if (!refinery.productionActive)
			{
				return;
			}

			float overallRemainingMilitaryCapacity = militaryData.maxMilitaryCapacity - militaryData.currentFleetCount;
			float amountOfFleetsAtRefinery = 0;

			//Repair any ships in this chunk
			//Or add any ships to a collection with empty space in this chunk
			if (militaryData.positionToFleets.ContainsKey(refinery.refineryPosition))
			{
				amountOfFleetsAtRefinery = militaryData.positionToFleets[refinery.refineryPosition].Count;

				if (refinery.productionActive)
				{
					foreach (ShipCollection collection in militaryData.positionToFleets[refinery.refineryPosition])
					{
						if (collection.GetShips().Count < collection.GetCapacity())
						{
							//Add new ship
							Ship newShip = collection.GetNewShip();

							//Set the parent so this ship can know things about its faction
							newShip.SetParent(refinery.parent);
							collection.AddShip(newShip);

							//Mark the collection as having been updated
							//So we need to draw more ships
							collection.MarkCollectionUpdate(ShipCollection.UpdateType.Add, newShip);
						}

						List<Ship> ships = collection.GetShips();

						foreach (Ship ship in ships)
						{
							//Just restore 20% of health each tick
							ship.health = Mathf.Clamp(ship.health + (ship.GetMaxHealth() * 0.2f), 0, ship.GetMaxHealth());

							//Undestroy a ship
							if (ship.isWreck)
							{
								ship.isWreck = false;
							}
						}
					}
				}
			}

			//Do we have any ship capacity left at all
			if (overallRemainingMilitaryCapacity >= 1)
			{
				//create new ships
				//Limit the amount of ships we can add based on the amount of ships already in the cell
				float maxAmountToAdd = Mathf.Min(overallRemainingMilitaryCapacity, refinery.refineryCollectionStorageCapacity - amountOfFleetsAtRefinery);

				for (int i = 0; i < maxAmountToAdd; i++)
				{
					if (SimulationManagement.random.Next(0, 101) / 100.0f < refinery.productionSpeed)
					{
						militaryData.AddFleet(refinery.refineryPosition, new Fleet());
					}
				}
			}
		}
	}
}