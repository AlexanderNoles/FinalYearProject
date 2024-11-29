using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(29)]
public class NationSettlementManagementRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

        List<(int, SettlementData)> nationIndexAndSettleData = new List<(int, SettlementData)>();

        for (int i = 0; i < nations.Count; i++)
        {
            if (nations[i].GetData(Faction.Tags.Settlements, out SettlementData data))
            {
                nationIndexAndSettleData.Add((i, data));
            }
        }

        foreach ((int, SettlementData) currentNation in nationIndexAndSettleData)
        {
			//Get relationship data
			nations[currentNation.Item1].GetData(Faction.relationshipDataKey, out RelationshipData relationshipData);
			nations[currentNation.Item1].GetData(Faction.Tags.HasMilitary, out MilitaryData militaryData);
			nations[currentNation.Item1].GetData(Faction.Tags.Population, out PopulationData popData);

			float maxMilitaryCapacity = SimulationHelper.ValueTanhFalloff(popData.currentPopulationCount, 1000, 9000);

			//Iterate through each settlement
			foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> settlePair in currentNation.Item2.settlements)
            {
                SettlementData.Settlement currentSettlement = settlePair.Value;
				//Get inverted settlement index
				int invertedSettlementIndex = currentNation.Item2.settlements.Count - currentSettlement.setID;

				#region Trade Control
				//Trade control
				//Set current settlement trade fleet capacity

				//Calculate number of trade fleets this settlement should have 
				//based on inverted set id. This means older settlements will have more trade fleets

				currentSettlement.tradeFleetCapacity = Mathf.RoundToInt(Mathf.Max(Mathf.Log10(invertedSettlementIndex), 0));

				//Increase current trade fleets if not at capacity
				if (currentSettlement.tradeFleets.Count() < currentSettlement.tradeFleetCapacity)
				{
					currentSettlement.tradeFleets.Add(new SettlementData.Settlement.TradeFleet());
				}

				foreach (SettlementData.Settlement.TradeFleet tradeFleet in currentSettlement.tradeFleets)
				{
					//If not at trade fleet capacity...
					if (tradeFleet.tradeFleetCapacity > tradeFleet.ships.Count)
					{
						//...this tick add one new ship
						TradeShip newShip = new TradeShip();
						newShip.homeLocation = currentSettlement.location;

						tradeFleet.ships.Add(newShip);
					}

					foreach (TradeShip ship in tradeFleet.ships)
					{
						//Has completed journey
						if ((SimulationManagement.GetCurrentSimulationTime() - ship.startTime) > ship.currentJourneysLength)
						{
							//Apply effect of trade
							//This is a simplification from what should realistically happen
							//(trade is half completed when goods are delivered and fully completed when ship arrives back)

							//Signal we want a new trade
							ship.tradeTarget = null;
							ship.cargoAmount = 0;
						}

						int loopClamp = 0;

						//Needs new trade target
						while (loopClamp < 25 && ship.tradeTarget == null)
						{
							//Get random nation (could we expand this to all factions?)
							(int, SettlementData) targetNation = nationIndexAndSettleData[SimulationManagement.random.Next(0, nationIndexAndSettleData.Count)];
							int nationID = nations[targetNation.Item1].id;

							//Are we on good terms with that faction?
							if (relationshipData == null ||
								(relationshipData.idToRelationship.ContainsKey(nationID) && relationshipData.idToRelationship[nationID].favourability > 0.3f))
							{
								//If we are get a random settlement from that faction
								//! If we want we can make it calcualte distance to that settlement and all that
								if (targetNation.Item2.settlements.Count > 0)
								{
									SettlementData.Settlement targetSettlement = targetNation.Item2.settlements.ElementAt(SimulationManagement.random.Next(0, targetNation.Item2.settlements.Count)).Value;

									if (!targetSettlement.Equals(currentSettlement))
									{
										ship.StartNewJourney(targetSettlement.location, SimulationManagement.GetSimulationDaysAsTime(100), 10);
									}
								}
							}

							loopClamp++;
						}
					}
				}
				#endregion

				#region Military Generation Control
				float currentRemaingFleetCapacityNationWide = maxMilitaryCapacity - militaryData.currentFleetCount;

				if (currentRemaingFleetCapacityNationWide >= 1)
				{
					//Calculate how many units this can produce this tick
					//And then max units that can be stored for this settlement this tick

					//Then add new ships if there is capacity

					//Max units for this settlement using inverted settlement id, this means older sets will have a higher capacity
					float maxUnitStorageCapacity = SimulationHelper.ValueTanhFalloff(invertedSettlementIndex, 5);
					int fleetShipLimitForSettlement = Mathf.RoundToInt(SimulationHelper.ValueTanhFalloff(invertedSettlementIndex, 3));
					fleetShipLimitForSettlement = Mathf.Max(1, fleetShipLimitForSettlement);
					int fleetCountInCell = 0;
					RealSpacePostion cellCenter = settlePair.Key;
					if (militaryData.cellCenterToFleets.ContainsKey(cellCenter))
					{
						fleetCountInCell = militaryData.cellCenterToFleets[cellCenter].Count;

						//Add ships to fleet or repair ships if they have damage taken
						foreach (ShipCollection collection in militaryData.cellCenterToFleets[cellCenter])
						{
							Fleet current = collection as Fleet;

							if (current.ships.Count < fleetShipLimitForSettlement)
							{
								//Add new ship
								current.ships.Add(new FleetShip());
							}

							List<Ship> ships = current.GetShips();
							foreach (Ship ship in ships)
							{
								//Just restore 20% of health each tick
								//Currently (28/11/2024, 15:45) no routine to make ships retreat back to base after winning
								//Should be easy to add but I want to make a general retreat routine instead (so that includes fleeing from battle)
								ship.health += ship.GetMaxHealth() * 0.2f;
							}
						}
					}

					float maxAmountToAdd = Mathf.Min(currentRemaingFleetCapacityNationWide, maxUnitStorageCapacity - fleetCountInCell);

					if (maxAmountToAdd >= 1)
					{
						float productionSpeed = SimulationHelper.ValueTanhFalloff(invertedSettlementIndex, 1, 10);

						if (!nations[currentNation.Item1].HasTag(Faction.Tags.AtWar))
						{
							//Lower production outside war time
							productionSpeed *= 0.1f;
						}

						for (int i = 0; i < maxAmountToAdd; i++)
						{
							if (SimulationManagement.random.Next(0, 101) / 100.0f < productionSpeed)
							{
								militaryData.AddFleet(cellCenter, new Fleet());
							}
						}
					}
				}
				#endregion
			}
		}
    }
}
