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
		List<SettlementsData> settlementDatas = SimulationManagement.GetDataViaTag(DataTags.Settlements).Cast<SettlementsData>().ToList();

		foreach (SettlementsData settlementData in settlementDatas)
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

			StrategyData stratData = null;
			bool hasStratData = settlementData.TryGetLinkedData(DataTags.Strategy, out stratData);
			//

			//Iterate through each settlement
			foreach (KeyValuePair<RealSpacePosition, SettlementsData.Settlement> settlePair in settlementData.settlements)
            {
                SettlementsData.Settlement currentSettlement = settlePair.Value;
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
                    currentSettlement.tradeFleets.Add(new SettlementsData.Settlement.TradeFleet());
                }

                foreach (SettlementsData.Settlement.TradeFleet tradeFleet in currentSettlement.tradeFleets)
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
                            SettlementsData targetSettlementData = settlementDatas[SimulationManagement.random.Next(0, settlementDatas.Count)];
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
                                    SettlementsData.Settlement targetSettlement = 
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

				#region Refinery
				//Setup refinery data to be processed
				RefineryData refinery = settlePair.Value.settlementRefinery;

				//Produciton Speed
				refinery.productionSpeed = MathHelper.ValueTanhFalloff(invertedSettlementIndex, 1, 10);

				if (hasStratData && (stratData.GetTargets().Count == 0 || stratData.globalStrategy == StrategyData.GlobalStrategy.Defensive))
				{
					//Reduce military production speed outside of war time, or if stratergy is currently defensive
					refinery.productionSpeed *= 0.1f;
				}
				//

				//Storage Capacity
				//Max units for this settlement using inverted settlement id, this means older sets will have a higher capacity
				refinery.refineryCollectionStorageCapacity = MathHelper.ValueTanhFalloff(invertedSettlementIndex, 5);
				//

				//Disable production if ongoing battle
				refinery.productionActive = !(hasBattleData && battleData.positionToOngoingBattles.ContainsKey(refinery.refineryPosition));
				//

				//Manually process refinery as lookup for main refinery routine ignores nested data
				RefineryRoutine.ProcessRefinery(refinery, militaryData);
				#endregion
            }
        }
    }
}
