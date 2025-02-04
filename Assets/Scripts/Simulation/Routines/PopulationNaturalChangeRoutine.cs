using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(25)]
public class PopulationNaturalChangeRoutine : RoutineBase
{
    public override void Run()
    {
        List<DataModule> populationDatas = SimulationManagement.GetDataViaTag(DataTags.Population);

        foreach (PopulationData populationData in populationDatas.Cast<PopulationData>())
        {
            //Change the population
            if (populationData.variablePopulation)
            {
                populationData.currentPopulationCount = 
                    Mathf.Clamp(
                        (populationData.currentPopulationCount + populationData.populationNaturalGrowthSpeed) - populationData.populationNaturalDeathSpeed, 
                        0,
                        populationData.populationNaturalGrowthLimt);
            }
        }
    }
}
