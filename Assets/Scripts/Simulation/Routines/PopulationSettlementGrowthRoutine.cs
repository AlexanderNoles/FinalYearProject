using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(35)]
public class PopulationSettlementGrowthRoutine : RoutineBase
{
    public override void Run()
    {
        List<DataBase> populations = SimulationManagement.GetDataViaTag(DataTags.Population);

        foreach (PopulationData populationData in populations.Cast<PopulationData>())
        {
            if (populationData.TryGetLinkedData(DataTags.Settlement, out SettlementData settlementData))
            {
                populationData.populationNaturalGrowthLimt = 0;

                foreach (RealSpacePostion pos in settlementData.settlements.Keys)
                {
                    populationData.populationNaturalGrowthLimt += settlementData.settlements[pos].maxPop;
                }

                //Currently we just use some random gradients
                populationData.populationNaturalGrowthSpeed = Mathf.Log10(Mathf.Max(populationData.currentPopulationCount, 1.1f)) / 10.0f;

                float modifier;
                if (populationData.currentPopulationCount > populationData.populationNaturalGrowthLimt)
                {
                    modifier = 1;
                }
                else
                {
                    modifier = 20.0f;
                }

                populationData.populationNaturalDeathSpeed = Mathf.Log10(Mathf.Max(populationData.currentPopulationCount, 1.1f)) / modifier;

                if (populationData.currentPopulationCount > populationData.populationNaturalGrowthLimt * 0.9f)
                {
                    settlementData.rawSettlementCapacity += 1;
                }
                if (populationData.currentPopulationCount < populationData.populationNaturalGrowthLimt * 0.5f)
                {
                    settlementData.rawSettlementCapacity -= 1;
                }
            }
        }
    }
}
