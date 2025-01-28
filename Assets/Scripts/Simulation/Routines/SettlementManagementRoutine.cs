using EntityAndDataDescriptor;
using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(29)]
public class SettlementManagementRoutine : RoutineBase
{
    public override void Run()
    {
		List<SettlementData> settlementDatas = SimulationManagement.GetDataViaTag(DataTags.Settlement).Cast<SettlementData>().ToList();

		foreach (SettlementData settlementData in settlementDatas)
		{
			//Get data for this entity
			EconomyData economyData = null;
			bool hasEconomy = settlementData.TryGetLinkedData(DataTags.Economic, out economyData);

			FeelingsData feelingsData = null;
			bool hasFeelings = settlementData.TryGetLinkedData(DataTags.Feelings, out feelingsData);

			MilitaryData militaryData = null;
			bool hasMilitary = settlementData.TryGetLinkedData(DataTags.Military, out militaryData);

			BattleData battleData = null;
			bool hasBattleData = settlementData.TryGetLinkedData(DataTags.Battle, out battleData);

			WarData warData = null;
			bool hasWarData = settlementData.TryGetLinkedData(DataTags.War, out warData);
			//

            //Iterate through each settlement
            foreach (KeyValuePair<RealSpacePosition, SettlementData.Settlement> settlePair in settlementData.settlements)
            {
                SettlementData.Settlement currentSettlement = settlePair.Value;
                //Get inverted settlement index
				//This is used to estimate the relative level of this settlements
				//As older settlements (with lower ids) will have higher purchasing power etc.
                int invertedSettlementIndex = settlementData.settlements.Count - currentSettlement.setID;

				if (hasEconomy)
                {
                    economyData.purchasingPower += (invertedSettlementIndex * 0.05f);
                }

                #region Trade Control
                //Trade control
                //Set current settlement trade fleet capacity

                //Calculate number of trade fleets this settlement should have 
                //based on inverted set id.

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
                        //...this tick add one (1) new ship
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
							if (hasEconomy)
                            {
                                economyData.purchasingPower += ship.cargoAmount * 10.0f;
                            }

                            //Signal we want a new trade
                            ship.tradeTarget = null;
                            ship.cargoAmount = 0;
                        }

                        int loopClamp = 0;

                        //Needs new trade target
                        while (loopClamp < 25 && ship.tradeTarget == null)
                        {
                            //Get random target settlement data
                            SettlementData targetSettlementData = settlementDatas[SimulationManagement.random.Next(0, settlementDatas.Count)];
							int targetID = targetSettlementData.parent.Get().id;

                            //Are we on good terms with that entity?
                            if (!hasFeelings ||
                                (feelingsData.idToFeelings.ContainsKey(targetID) && feelingsData.idToFeelings[targetID].favourability > 0.3f))
                            {
                                //If we are get a random settlement from that entity
                                //! If we want we can make it calcualte distance to that settlement and all that

                                if (targetSettlementData.settlements.Count > 0)
                                {
									//Element at is not a very performant function!
                                    SettlementData.Settlement targetSettlement = 
										targetSettlementData.settlements.ElementAt(
											SimulationManagement.random.Next(0, targetSettlementData.settlements.Count)).Value;

									//Not this settlement
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
				if (hasMilitary)
				{
                    float currentRemaingFleetCapacityNationWide = militaryData.maxMilitaryCapacity - militaryData.currentFleetCount;

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
                        RealSpacePosition settlementPosition = settlePair.Value.actualSettlementPos;
                        if (militaryData.positionToFleets.ContainsKey(settlementPosition))
                        {
                            fleetCountInCell = militaryData.positionToFleets[settlementPosition].Count;

							//Don't repair or add new ships if ongoing battle in this settlement
							if (!(hasBattleData && battleData.positionToOngoingBattles.ContainsKey(settlementPosition)))
							{
								//Add ships to fleet or repair ships if they have damage taken
								foreach (ShipCollection collection in militaryData.positionToFleets[settlementPosition])
								{
									Fleet current = collection as Fleet;

									if (current.ships.Count < fleetShipLimitForSettlement)
									{
										//Add new ship
										FleetShip newShip = new FleetShip();
										//Set the parent so this ship can know things about its faction
										newShip.SetParent(settlementData.parent);
										current.ships.Add(newShip);
										//Mark the collection as having been updated
										//So we need to draw more ships
										current.MarkCollectionUpdate(ShipCollection.UpdateType.Add, newShip);
									}

									List<Ship> ships = current.GetShips();
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

                        float maxAmountToAdd = Mathf.Min(currentRemaingFleetCapacityNationWide, maxUnitStorageCapacity - fleetCountInCell);

                        if (maxAmountToAdd >= 1)
                        {
                            float productionSpeed = MathHelper.ValueTanhFalloff(invertedSettlementIndex, 1, 10);

                            if (!hasWarData || warData.atWarWith.Count == 0 || warData.globalStratergy == WarData.GlobalStratergy.Defensive)
                            {
								//Reduce military production speed outside of war time, or if stratergy is currently defensive
                                productionSpeed *= 0.1f;
                            }

                            for (int i = 0; i < maxAmountToAdd; i++)
                            {
                                if (SimulationManagement.random.Next(0, 101) / 100.0f < productionSpeed)
                                {
                                    militaryData.AddFleet(settlementPosition, new Fleet());
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
    }
}
