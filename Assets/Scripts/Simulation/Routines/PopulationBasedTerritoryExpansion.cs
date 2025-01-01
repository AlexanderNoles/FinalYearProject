using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.ActiveSimulationRoutine(20)]
public class PopulationBasedTerritoryExpansion : RoutineBase
{
    public override void Run()
    {
		//Get all territories 
		List<TerritoryData> territories = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();

		foreach (TerritoryData territory in territories)
		{
            //Check this territory has population
			PopulationData populationData = null;
			if (!territory.TryGetLinkedData(DataTags.Population, out populationData))
			{
				continue;
			}

            //Alter growth rate over time
            //Growth rate can be dramatiaclly reduced if it is appropriate
			bool reducedGrowth = false;
			if (territory.TryGetLinkedData(DataTags.War, out WarData warData))
            {
                //When at war growth rate is dramatically reduced
                //This prevents wars from continuing forever by just having nations expand as they are destroyed
                //Realistic too!
                //Updated to only be in effect if the global stratergy is aggresive
                reducedGrowth = warData.atWarWith.Count > 0 && warData.globalStratergy == WarData.GlobalStratergy.Aggresive;
			}

			if (reducedGrowth)
            {
                territory.growthRate = Mathf.Lerp(territory.growthRate, 0.1f, 0.5f);
            }
			else
            {
                territory.growthRate = Mathf.MoveTowards(territory.growthRate, MathHelper.ValueTanhFalloff(populationData.currentPopulationCount / 100.0f, 2, -1), 0.05f);
            }

            //Apply territory change effects
            territory.territoryClaimUpperLimit += territory.growthRate;

            territory.territoryClaimUpperLimit = MathHelper.ValueTanhFalloff(territory.territoryClaimUpperLimit, Mathf.Clamp(populationData.currentPopulationCount * 5.0f, 10, 900), -1);

            if (territory.territoryCenters.Count > 0 && territory.territoryCenters.Count < territory.territoryClaimUpperLimit)
            {
                //This is an absurdly unperformant function! (ElementAt is from IEnumerable so to get to an element it will enumerate through
                //every entry if it has too)
                RealSpacePostion currentBorder = territory.borders.ElementAt(SimulationManagement.random.Next(0, territory.borders.Count));

                List<RealSpacePostion> neighbours = WorldManagement.GetNeighboursInGrid(currentBorder);

                foreach (RealSpacePostion pos in neighbours)
                {
                    //If not currently claimed by anyone else and within solar system
                    if (!RoutineHelper.AnyContains(territories, pos) && WorldManagement.WithinValidSolarSystem(pos))
                    {
                        //If not claimed by us
                        //! I actually don't think this check is neccesary because of AnyContains above
                        if (!territory.territoryCenters.Contains(pos))
                        {
                            //Need to perform "should be border" check on both this new territory and it's neighbours
                            territory.AddTerritory(pos);
                        }
                    }
                }
            }
        }
    }
}
