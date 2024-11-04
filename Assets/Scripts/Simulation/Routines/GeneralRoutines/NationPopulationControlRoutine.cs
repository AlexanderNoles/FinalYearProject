using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(35)]
public class NationPopulationControlRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

        foreach (Faction faction in nations)
        {
            if (faction.GetData(Faction.Tags.Population, out PopulationData popData))
            {
                if (faction.GetData(Faction.Tags.Settlements, out SettlementData setData))
                {
                    popData.populationNaturalGrowthLimt = 0;

                    foreach (RealSpacePostion pos in setData.settlements.Keys)
                    {
                        popData.populationNaturalGrowthLimt += setData.settlements[pos].maxPop;
                    }

                    //Currently we just use some random gradients
                    popData.populationNaturalGrowthSpeed = Mathf.Log10(Mathf.Max(popData.currentPopulationCount, 1.1f)) / 10.0f;
                    popData.populationNaturalDeathSpeed = Mathf.Log10(Mathf.Max(popData.currentPopulationCount, 1.1f)) / 20.0f;

                    if (popData.currentPopulationCount > popData.populationNaturalGrowthLimt * 0.9f)
                    {
                        setData.rawSettlementCapacity += 1;
                    }
                    if (popData.currentPopulationCount < popData.populationNaturalGrowthLimt * 0.5f)
                    {
                        setData.rawSettlementCapacity -= 1;
                    }
                }
            }
        }
    }
}
