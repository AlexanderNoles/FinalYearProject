using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(20)]
public class PopulationBasedTerritoryExpansion : RoutineBase
{
    public override void Run()
    {
		//Get all territories 
		List<TerritoryData> territories = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();

		foreach (TerritoryData territory in territories)
		{
			//Check this territory has population
			if (!territory.TryGetLinkedData(DataTags.Population, out PopulationData populationData))
			{
				continue;
			}

			//Alter growth rate over time
			//Growth rate can be dramatiaclly reduced if it is appropriate
			bool reducedGrowth = false;
			if (territory.TryGetLinkedData(DataTags.Strategy, out StrategyData stratData))
            {
				//When at war growth rate is dramatically reduced
				//This prevents wars from continuing forever by just having nations expand as they are destroyed
				//Realistic too!
				reducedGrowth = stratData.globalStrategy == StrategyData.GlobalStrategy.Aggresive && stratData.GetTargets().Count > 0;
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
			territory.territoryClaimUpperLimit = Mathf.Min(territory.territoryClaimUpperLimit, territory.hardTerritoryCountLimit);

            if (territory.territoryCenters.Count < territory.territoryClaimUpperLimit)
            {
				if (territory.territoryCenters.Count > 0)
				{
					//This is an absurdly unperformant function! (ElementAt is from IEnumerable so to get to an element it will enumerate through
					//every entry if it has too)
					RealSpacePosition currentBorder = territory.borders.ElementAt(SimulationManagement.random.Next(0, territory.borders.Count));

					List<RealSpacePosition> neighbours = WorldManagement.GetNeighboursInGrid(currentBorder);

					foreach (RealSpacePosition pos in neighbours)
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
				else
				{
					//Currently have no territory!
					//For some entites this should've killed them last tick (so they shouldn't be running this code)
					//But for others they want to instead try to keep reclaiming their origin
					if (!RoutineHelper.AnyContains(territories, territory.origin)) 
					{
						territory.AddTerritory(territory.origin);
					}
				}
            }
        }
    }
}
