using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(30)]
public class SettlementRoutine : RoutineBase
{
    public override void Run()
    {
        //Get all settlement factions
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Settlements);

        foreach (Faction faction in factions)
        {
            if (faction.GetData(Faction.Tags.Settlements, out SettlementData settlementData))
            {
                int calculatedSettlementCapacity = Mathf.Max((int)(5 * Mathf.Log10(settlementData.rawSettlementCapacity)), 1);

                //Based on our current settlement cap create or destroy settlements
                if (settlementData.settlements.Count < calculatedSettlementCapacity)
                {
                    int difference = calculatedSettlementCapacity - settlementData.settlements.Count;

                    for (int i = 0; i < difference; i++)
                    {
                        //Try to find appriopriate position
                        RealSpacePostion pos = null;

                        if (faction.GetData(Faction.Tags.Territory, out TerritoryData territoryData))
                        {
                            int count = territoryData.territoryCenters.Count;

                            //Start at a random place in the list to give settelments more of a variance
                            int indexOffset = SimulationManagement.random.Next(count);

                            for (int t = 0; t < count; t++)
                            {
                                //This really badly prioritizes early sectors
                                //We should instead be picking one at random
                                pos = territoryData.territoryCenters.ElementAt((t + indexOffset) % count);


                                if (settlementData.settlements.ContainsKey(pos))
                                {
                                    pos = null;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (pos != null && !settlementData.settlements.ContainsKey(pos))
                        {
                            SettlementData.Settlement newSettlement = new SettlementData.Settlement();
                            newSettlement.actualSettlementPos = WorldManagement.RandomPositionInChunk(pos, SimulationManagement.random);

                            settlementData.settlements.Add(pos, newSettlement);
                        }

                    }
                }
                if (settlementData.settlements.Count > calculatedSettlementCapacity)
                {
                    //Destroy
                    //Right now we just destroy one at random 
                    KeyValuePair<RealSpacePostion, SettlementData.Settlement> keyValuePair = settlementData.settlements.ElementAt(SimulationManagement.random.Next(0, settlementData.settlements.Count));
                    settlementData.settlements.Remove(keyValuePair.Key);
                }
            }
        }
    }
}
