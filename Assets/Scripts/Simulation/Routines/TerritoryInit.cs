using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.ActiveSimulationRoutine(95, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class TerritoryInit : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
        return tags.Contains(DataTags.Territory);
    }

    public override void Run()
    {
		//Get all territory data for the contains check
		List<TerritoryData> territories = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();
		//Get territories that need to be initalized
		List<DataBase> territoriesToInit = SimulationManagement.GetToInitData(DataTags.Territory);

		foreach (TerritoryData territoryData in territoriesToInit.Cast<TerritoryData>())
		{
            //If this territoryData doesn't have an origin, give it one.
            if (territoryData.origin == null)
            {
                //Nations should try to grab a planet
                //If all are claimed then just get a random position
                for (int i = 0; i < Planet.availablePlanetPositions.Count && territoryData.origin == null; i++)
                {
                    territoryData.origin = Planet.availablePlanetPositions[i];

                    if (!RoutineHelper.AnyContains(territories, territoryData.origin))
                    {
                        //No one owns this planet
                        territoryData.AddTerritory(territoryData.origin);
                    }
                    else
                    {
                        territoryData.origin = null;
                    }
                }

                //Fallback
                //Pick random position
                if (territoryData.origin == null)
                {
                    do
                    {
                        territoryData.origin = WorldManagement.RandomCellCenterWithinSolarSystem();

                        if (!RoutineHelper.AnyContains(territories, territoryData.origin))
                        {
                            territoryData.AddTerritory(territoryData.origin);
                        }
                        else
                        {
                            territoryData.origin = null;
                        }
                    }
                    while (territoryData.origin == null);
                }
            }
        }
    }
}
