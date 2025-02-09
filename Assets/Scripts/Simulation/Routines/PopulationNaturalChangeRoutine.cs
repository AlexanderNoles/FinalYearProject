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
			float speedOffsetModifier = 1.0f;

			if (!populationData.growthAffectedBySimSpeed)
			{
				float simSpeed = SimulationManagement.GetSimulationSpeed();

				if (simSpeed > 0.0f)
				{
					//Can't divide by zero!
					speedOffsetModifier = 1.0f / simSpeed;
				}
			}

            //Change the population
            if (populationData.variablePopulation)
            {
                populationData.currentPopulationCount = 
                    Mathf.Clamp(
                        (populationData.currentPopulationCount + (populationData.populationNaturalGrowthSpeed * speedOffsetModifier)) - (populationData.populationNaturalDeathSpeed * speedOffsetModifier), 
                        0,
                        populationData.populationNaturalGrowthLimt);
            }
        }
    }
}
