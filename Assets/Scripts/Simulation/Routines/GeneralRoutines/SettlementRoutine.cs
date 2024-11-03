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
            if (faction.GetData(Faction.Tags.Settlements, out DataBase data))
            {
                SettlementData settlementData = data as SettlementData;

                //Based on our current settlement cap create or destroy settlements
                if (settlementData.settlements.Count < settlementData.settlementCapacity)
                {
                    int difference = settlementData.settlementCapacity - settlementData.settlements.Count;

                    for (int i = 0; i < difference; i++)
                    {
                        //Try to find appriopriate position
                        RealSpacePostion pos = null;

                        if (faction.GetData(Faction.Tags.Territory, out DataBase terrData))
                        {
                            TerritoryData territoryData = terrData as TerritoryData;

                            int count = territoryData.territoryCenters.Count;
                            for (int t = 0; t < count; t++)
                            {
                                pos = territoryData.territoryCenters.ElementAt(t);
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
                            settlementData.settlements.Add(pos, new SettlementData.Settlement());
                        }

                    }
                }
                if (settlementData.settlements.Count > settlementData.settlementCapacity)
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
