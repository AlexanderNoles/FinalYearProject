using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(25)]
public class PopulationGrowthRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> populatedFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Population);

        foreach (Faction faction in populatedFactions)
        {
            if (faction.GetData(Faction.Tags.Population, out PopulationData data))
            {
                //Grow the population
                if (data.currentPopulationCount < data.populationNaturalGrowthLimt)
                {
                    data.currentPopulationCount = Mathf.Clamp((data.currentPopulationCount + data.populationNaturalGrowthSpeed) - data.populationNaturalDeathSpeed, 0, data.populationNaturalGrowthLimt);
                }
            }
        }
    }
}
