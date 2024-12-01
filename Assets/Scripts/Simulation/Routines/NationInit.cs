using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(95, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class NationInit : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return tags.Contains(Faction.Tags.Nation);
    }

    public override void Run()
    {
        //Get all nations and set their territory information accurately.
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

        //Get all territory factions
        List<Faction> territories = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);
        List<TerritoryData> territoryDatas = new List<TerritoryData>();

        foreach (Faction territory in territories)
        {
            if (territory.GetData(Faction.Tags.Territory, out TerritoryData data))
            {
                territoryDatas.Add(data);
            }
        }

        foreach (Faction faction in nations)
        {
            if (faction.GetData(Faction.Tags.Territory, out TerritoryData territory))
            {
                //If this faction doesn't have an origin, give it one.
                if (territory.origin == null)
                {
					//Nations should try to grab a planet
					//If all are claimed then just get a random position
					for (int i = 0; i < Planet.availablePlanetPositions.Count && territory.origin == null; i++)
					{
						territory.origin = Planet.availablePlanetPositions[i];

						if (!AnyContains(territoryDatas, territory.origin))
						{
							//No one owns this planet
							territory.AddTerritory(territory.origin);
						}
						else
						{
							territory.origin = null;
						}
					}

					//Fallback
					//Pick random position
					if (territory.origin == null)
					{
						do
						{
							territory.origin = WorldManagement.RandomCellCenterWithinSolarSystem();

							if (!AnyContains(territoryDatas, territory.origin))
							{
								territory.AddTerritory(territory.origin);
							}
							else
							{
								territory.origin = null;
							}
						}
						while (territory.origin == null);
					}
                }
            }
        }
    }
}
