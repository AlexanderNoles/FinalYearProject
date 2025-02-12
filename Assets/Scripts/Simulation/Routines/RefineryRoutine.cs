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
			if (refinery.putInReserves || militaryData.positionToFleets.ContainsKey(refinery.refineryPosition))
			{
				if (!refinery.putInReserves)
				{
					//If ships are being put in reserves we don't care if any ships are at the refinery
					amountOfFleetsAtRefinery = militaryData.positionToFleets[refinery.refineryPosition].Count;
				}

				if (refinery.productionActive)
				{
					List<ShipCollection> shipCollectionAtRefinery;

					if (refinery.putInReserves)
					{
						shipCollectionAtRefinery = militaryData.reserveFleets;
					}
					else
					{
						shipCollectionAtRefinery = militaryData.positionToFleets[refinery.refineryPosition];
					}

					foreach (ShipCollection collection in shipCollectionAtRefinery)
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
			if (overallRemainingMilitaryCapacity >= 1.0f)
			{
				//create new ships
				//Limit the amount of ships we can add based on the amount of ships already in the cell
				float maxAmountToAdd = Mathf.Min(Mathf.Floor(overallRemainingMilitaryCapacity), refinery.refineryCollectionStorageCapacity - amountOfFleetsAtRefinery);

				float chanceModifier = 1.0f;

				if (!refinery.productionAffectedBySimSpeed)
				{
					float simSpeed = SimulationManagement.GetSimulationSpeed();

					if (simSpeed > 0.0f)
					{
						//Can't divide by zero!
						chanceModifier = 1.0f / simSpeed;
					}
				}

				for (int i = 0; i < maxAmountToAdd; i++)
				{
					if (SimulationManagement.random.Next(0, 101) / 100.0f < refinery.productionSpeed * chanceModifier)
					{
						//Create new fleet
						ShipCollection collection = militaryData.GetNewFleet();

						if (refinery.putInReserves)
						{
							//Add to reserves, don't place anywhere on the map
							militaryData.AddFleetToReserves(collection);
						}
						else
						{
							//Place at refinery position on map
							militaryData.AddFleet(refinery.refineryPosition, collection);
						}

						if (refinery.autoFillFleets)
						{
							//Fill fleet
							for (int s = 0; s < collection.GetCapacity(); s++)
							{
								Ship newShip = collection.GetNewShip();
								newShip.SetParent(refinery.parent);
								collection.AddShip(newShip);
								collection.MarkCollectionUpdate(ShipCollection.UpdateType.Add, newShip); 
							}
						}
					}
				}
			}
		}
	}
}