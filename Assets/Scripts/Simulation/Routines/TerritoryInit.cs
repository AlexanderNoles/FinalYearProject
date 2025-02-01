using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(95, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
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
		List<DataModule> territoriesToInit = SimulationManagement.GetToInitData(DataTags.Territory);

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
					int loopClamp = 1000;
                    do
                    {
                        territoryData.origin = WorldManagement.RandomCellCenterWithinSolarSystem();

						//If above function returned null it couldn't find any position
						//This means the solar system is likely full!
						if (loopClamp <= 0 || territoryData.origin == null)
						{
							break;
						}

                        if (!RoutineHelper.AnyContains(territories, territoryData.origin))
                        {
                            territoryData.AddTerritory(territoryData.origin);
                        }
                        else
                        {
                            territoryData.origin = null;
                        }

						loopClamp--;
                    }
                    while (territoryData.origin == null);
                }

				//If we can't find any position at all
				if (territoryData.origin == null)
				{
					territoryData.parent.Get().AddTag(EntityStateTags.Dead);
				}
            }
        }
    }
}
