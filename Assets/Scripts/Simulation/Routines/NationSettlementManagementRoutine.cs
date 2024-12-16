using MonitorBreak.Bebug;
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

		foreach (Faction nation in nations)
		{
			//Get neccesary data
			nation.GetData(Faction.relationshipDataKey, out RelationshipData relationshipData);
			nation.GetData(Faction.Tags.HasMilitary, out MilitaryData militaryData);
			nation.GetData(Faction.Tags.Population, out PopulationData popData);
			nation.GetData(Faction.Tags.Settlements, out SettlementData settData);
			nation.GetData(Faction.Tags.CanFightWars, out WarData warData);
			nation.GetData(Faction.Tags.HasEconomy, out EconomyData economyData);
			//

			//Basic effect of population
			//More is gained from trade and 
			economyData.purchasingPower -= (popData.currentPopulationCount / 300.0f) * (SimulationManagement.random.Next(-10, 51 + Mathf.FloorToInt(warData.warExhaustion * 0.1f)) / 100.0f);

			float maxMilitaryCapacity = MathHelper.ValueTanhFalloff(popData.currentPopulationCount, 1000, 9000);
			//Iterate through each settlement
			foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> settlePair in settData.settlements)
			{
				SettlementData.Settlement currentSettlement = settlePair.Value;
				//Get inverted settlement index
				int invertedSettlementIndex = settData.settlements.Count - currentSettlement.setID;

				economyData.purchasingPower += (invertedSettlementIndex * 0.05f);

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
							economyData.purchasingPower += ship.cargoAmount * 10.0f;

							//Signal we want a new trade
							ship.tradeTarget = null;
							ship.cargoAmount = 0;
						}

						int loopClamp = 0;

						//Needs new trade target
						while (loopClamp < 25 && ship.tradeTarget == null)
						{
							//Get random nation (could we expand this to all factions?)
							Faction targetNation = nations[SimulationManagement.random.Next(0, nations.Count)];

							//Are we on good terms with that faction?
							if (relationshipData == null ||
								(relationshipData.idToRelationship.ContainsKey(targetNation.id) && relationshipData.idToRelationship[targetNation.id].favourability > 0.3f))
							{
								//If we are get a random settlement from that faction
								//! If we want we can make it calcualte distance to that settlement and all that
								targetNation.GetData(Faction.Tags.Settlements, out SettlementData targetSetData);

								if (targetSetData.settlements.Count > 0)
								{
									SettlementData.Settlement targetSettlement = targetSetData.settlements.ElementAt(SimulationManagement.random.Next(0, targetSetData.settlements.Count)).Value;

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
					float maxUnitStorageCapacity = MathHelper.ValueTanhFalloff(invertedSettlementIndex, 5);
					int fleetShipLimitForSettlement = Mathf.RoundToInt(MathHelper.ValueTanhFalloff(invertedSettlementIndex, 3));
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
								ship.health = Mathf.Clamp(ship.health + (ship.GetMaxHealth() * 0.2f), 0, ship.GetMaxHealth());
							}
						}
					}

					float maxAmountToAdd = Mathf.Min(currentRemaingFleetCapacityNationWide, maxUnitStorageCapacity - fleetCountInCell);

					if (maxAmountToAdd >= 1)
					{
						float productionSpeed = MathHelper.ValueTanhFalloff(invertedSettlementIndex, 1, 10);

						if (warData.atWarWith.Count > 0)
						{
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

			economyData.purchasingPower = MathHelper.ValueTanhFalloff(economyData.purchasingPower, 3000);
		}
    }
}
