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

        List<(int, SettlementData)> allSettlementData = new List<(int, SettlementData)>();

        for (int i = 0; i < nations.Count; i++)
        {
            if (nations[i].GetData(Faction.Tags.Settlements, out SettlementData data))
            {
                allSettlementData.Add((i, data));
            }
        }

        foreach ((int, SettlementData) settlementData in allSettlementData)
        {
            foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> settlePair in settlementData.Item2.settlements)
            {
                SettlementData.Settlement currentSettlement = settlePair.Value;

                //Trade control
                if (settlementData.Item2.settlements.Count > 0)
                {
                    nations[settlementData.Item1].GetData(Faction.relationshipDataKey, out RelationshipData relationshipData);

                    //Set current settlement trade fleet capacity
                    //Get inverted settlement index
                    int invertedSettlementIndex = settlementData.Item2.settlements.Count - currentSettlement.setID;

                    //Calculate number of trade fleets this settlement should have 
                    //based on inverted set id. This means older settlements will have more trade fleets

                    currentSettlement.tradeFleetCapacity = Mathf.RoundToInt(Mathf.Max( Mathf.Log10(invertedSettlementIndex), 0));

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
                                (int, SettlementData) targetNation = allSettlementData[SimulationManagement.random.Next(0, allSettlementData.Count)];
                                int nationID = nations[targetNation.Item1].id;

                                //Are we on good terms with that faction?
                                if (relationshipData == null || 
                                    (relationshipData.idToRelationship.ContainsKey(nationID) && relationshipData.idToRelationship[nationID].favourability > 0.3f))
                                {
                                    //If we are get a random settlement from that faction
                                    //! If we want we can make it calcualte distance to that settlement and all that
                                    SettlementData.Settlement targetSettlement = targetNation.Item2.settlements.ElementAt(SimulationManagement.random.Next(0, targetNation.Item2.settlements.Count)).Value;

                                    if (!targetSettlement.Equals(currentSettlement))
                                    {
                                        ship.StartNewJourney(targetSettlement.location, SimulationManagement.GetSimulationDaysAsTime(100), 10);
                                    }
                                }

                                loopClamp++;
                            }
                        }
                    }
                }
            }
        }
    }
}
