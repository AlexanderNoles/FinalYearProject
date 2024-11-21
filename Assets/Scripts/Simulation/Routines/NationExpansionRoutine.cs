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
                                current.territoryCenters.Add(pos);

                                List<RealSpacePostion> toCheck = WorldManagement.GetNeighboursInGrid(pos);
                                toCheck.Add(pos);

                                foreach (RealSpacePostion potentialBorder in toCheck)
                                {
                                    //Couple of cases to account for:

                                    //First do we even own this territory...
                                    //...If we don't then we don't care
                                    if (current.territoryCenters.Contains(potentialBorder))
                                    {
                                        //Then we need to check if all neighbours for this are owned
                                        //If they are then we need to remove this from the border list
                                        //If they aren't then we need to add this
                                        //Assuming it isn't already removed or added in the first place

                                        bool allClaimed = true;
                                        List<RealSpacePostion> neighboursOfPotentialBorder = WorldManagement.GetNeighboursInGrid(potentialBorder);

                                        foreach (RealSpacePostion neighbourOfPB in neighboursOfPotentialBorder)
                                        {
                                            if (!current.territoryCenters.Contains(neighbourOfPB))
                                            {
                                                allClaimed = false;
                                                break; //Break check early
                                            }
                                        }

                                        if (!allClaimed)
                                        {
                                            //Should be a border
                                            if (!current.borders.Contains(potentialBorder))
                                            {
                                                current.borders.Add(potentialBorder);
                                            }
                                        }
                                        else
                                        {
                                            //Should not be a border
                                            //All already claimed
                                            if (current.borders.Contains(potentialBorder))
                                            {
                                                current.borders.Remove(potentialBorder);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
