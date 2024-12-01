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
			Faction nation = nations[nationIndexes[i]];

			TerritoryData current = territories[i];
			nation.GetData(Faction.Tags.Population, out PopulationData popData);
			nation.GetData(Faction.Tags.CanFightWars, out WarData warData);

			if (warData.atWarWith.Count > 0)
			{
				//When at war growth rate is dramatically reduced
				//This prevents wars from continuing forever by just having nations expand as they are destroyed
				//Realistic too!
				current.growthRate = Mathf.Lerp(current.growthRate, 0.1f, 0.5f);
			}
			else
			{
				current.growthRate = Mathf.MoveTowards(current.growthRate, SimulationHelper.ValueTanhFalloff(popData.currentPopulationCount / 100.0f, 2, -1), 0.05f);
			}

			current.territoryClaimUpperLimit += current.growthRate;

			current.territoryClaimUpperLimit = SimulationHelper.ValueTanhFalloff(current.territoryClaimUpperLimit, Mathf.Clamp(popData.currentPopulationCount * 5.0f, 10, 900), -1);

			if (current.territoryCenters.Count > 0 && current.territoryCenters.Count < current.territoryClaimUpperLimit)
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
