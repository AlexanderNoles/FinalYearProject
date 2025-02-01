using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(30)]
public class SettlementRoutine : RoutineBase
{
    public override void Run()
    {
        List<DataModule> settlementDatas = SimulationManagement.GetDataViaTag(DataTags.Settlement);

        foreach (SettlementData settlementData in settlementDatas.Cast<SettlementData>())
        {
            int calculatedSettlementCapacity = Mathf.Max((int)(5 * Mathf.Log10(settlementData.rawSettlementCapacity)), 1);

            //Based on our current settlement cap create or destroy settlements
            if (settlementData.settlements.Count < calculatedSettlementCapacity)
            {
                int difference = calculatedSettlementCapacity - settlementData.settlements.Count;

                for (int i = 0; i < difference; i++)
                {
                    //Try to find appriopriate position
                    RealSpacePosition pos = null;

                    if (settlementData.TryGetLinkedData(DataTags.Territory, out TerritoryData territoryData))
                    {
                        int count = territoryData.territoryCenters.Count;

                        //Start at a random place in the list to give settelments more of a variance
                        int indexOffset = SimulationManagement.random.Next(count);

                        for (int t = 0; t < count; t++)
                        {
                            //This is broadly ineffiecent 
                            //Perhaps we should just take the first valid position
                            //so we don't have to run element at
                            //This would create more centralized settlements which might also place them better in the world
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

                    //If position has been found
                    if (pos != null && !settlementData.settlements.ContainsKey(pos))
                    {
                        SettlementData.Settlement newSettlement =
                            new SettlementData.Settlement(WorldManagement.RandomPositionInCell(pos, SimulationManagement.random), settlementData.parent);

                        settlementData.AddSettlement(pos, newSettlement);
                    }

                }
            }

            //Too many settlements!
            if (settlementData.settlements.Count > calculatedSettlementCapacity)
            {
                //Destroy
                //Right now we just destroy one at random 
                KeyValuePair<RealSpacePosition, SettlementData.Settlement> keyValuePair = settlementData.settlements.ElementAt(SimulationManagement.random.Next(0, settlementData.settlements.Count));
                settlementData.settlements.Remove(keyValuePair.Key);
            }
        }
    }
}
