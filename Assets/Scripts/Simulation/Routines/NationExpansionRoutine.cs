using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(20)]
public class NationExpansionRoutine : RoutineBase
{
    public override void Run()
    {
        //Get all nations
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);
        List<TerritoryData> territories = new List<TerritoryData>();
        List<int> nationIndexes = new List<int>();

        for (int i = 0; i < nations.Count; i++)
        {
            if (nations[i].GetData(Faction.Tags.Territory, out TerritoryData data))
            {
                //Get current controlled territory data
                territories.Add(data);
                nationIndexes.Add(i);
            }
        }

        for (int i = 0; i < territories.Count; i++)
        {
            TerritoryData current = territories[i];
            
            //For nations this should be based on population data
            //So we need to grab the population data
            //If it doesn't exists we can't perform the rest of the routine

            if (nations[nationIndexes[i]].GetData(Faction.Tags.Population, out PopulationData popData))
            {
                float sizeLimit = popData.currentPopulationCount / 10; 

                if (current.territoryCenters.Count > 0 && current.territoryCenters.Count < sizeLimit)
                {
                    RealSpacePostion currentBorder = current.borders.ElementAt(SimulationManagement.random.Next(0, current.borders.Count));

                    List<RealSpacePostion> neighbours = WorldManagement.GetNeighboursInGrid(currentBorder);

                    foreach (RealSpacePostion pos in neighbours)
                    {
                        //If not currently claimed by anyone else and within solar system
                        if (!AnyContains(territories, pos) && WorldManagement.WithinValidSolarSystem(pos))
                        {
                            //If not claimed by us
                            //! I actually don't think this check is neccesary because of AnyContains above
                            if (!current.territoryCenters.Contains(pos))
                            {
								//Need to perform "should be border" check on both this new territory and it's neighbours
								current.AddTerritory(pos);
                            }
                        }
                    }
                }
            }
        }
    }
}
