using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(95, true)]
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

        Func<RealSpacePostion, bool> falloffPositionGenerationValidPositionCheck = delegate (RealSpacePostion pos)
        {
            return !AnyContains(territoryDatas, pos) && WorldManagement.WithinSolarSystem(pos);
        };

        foreach (Faction faction in nations)
        {
            if (faction.GetData(Faction.Tags.Territory, out TerritoryData territory))
            {
                //If this faction doesn't have an origin, give it one.
                if (territory.origin == null)
                {
                    //Nations should try to grab a planet
                    if (Planet.availablePlanetPositions.Count > 0)
                    {
                        territory.origin = Planet.availablePlanetPositions[0];
                        Planet.availablePlanetPositions.RemoveAt(0);

                        //Do a random falloff spread across the game map
                        //Disabled because we want to instead start the nation in a place and then run the simulation for 100 years (1 tick is one day)
                        //to get more dynamic and interesting results
                        //territory.territoryCenters = GenerationUtility.GenerateFalloffPositionSpread(
                        //    territory.origin, //Start pos
                        //    new Vector2(0.02f, 0.5f), //Min max falloff per step
                        //    100, //Max scale
                        //    falloffPositionGenerationValidPositionCheck,
                        //    SimulationManagement.random);

                        territory.territoryCenters.Add(territory.origin);
                    }
                    else if (faction.GetData(Faction.Tags.Faction, out FactionData factionData))
                    {
                        //If no planets avaliable for now just kill this faction
                        factionData.ForceDeath();
                    }
                }
            }
        }
    }
}
