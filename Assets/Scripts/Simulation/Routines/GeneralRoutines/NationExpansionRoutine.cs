using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(20)]
public class NationExpansionRoutine : RoutineBase
{
    public override void Run()
    {
        //Get all nations
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);
        List<TerritoryData> territories = new List<TerritoryData>();
        List<int> nationIndex = new List<int>();

        for (int i = 0; i < nations.Count; i++)
        {
            if (nations[i].GetData(Faction.Tags.Territory, out DataBase data))
            {
                //Get current controlled territory data
                territories.Add(data as TerritoryData);
                nationIndex.Add(i);
            }
        }

        for (int i = 0; i < territories.Count; i++)
        {
            TerritoryData current = territories[i];
            int sizeLimit = 100; //Currently static

            if (current.territoryCenters.Count > 0 && current.territoryCenters.Count < sizeLimit)
            {
                RealSpacePostion start = current.territoryCenters.ElementAt(SimulationManagement.random.Next(0, current.territoryCenters.Count));

                List<RealSpacePostion> neighbours = WorldManagement.GetNeighboursInGrid(start);

                foreach (RealSpacePostion pos in neighbours)
                {
                    if (!AnyContains(territories, pos) && !current.territoryCenters.Contains(pos) && WorldManagement.WithinSolarSystem(pos))
                    {
                        current.territoryCenters.Add(pos);
                    }
                }
            }
        }
    }
}
