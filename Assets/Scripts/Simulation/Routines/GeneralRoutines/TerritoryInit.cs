using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;
using System;

//[SimulationManagement.ActiveSimulationRoutine(90, true)]
public class TerritoryInit : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return tags.Contains(Faction.Tags.Territory);
    }

    public override void Run()
    {
        //Give a territory some area if it has none
        //Death routines should be run last on the previous tick so don't worry about accidently adding back territory to a faction that is meant to be dead
        List<Faction> territories = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);

        List<(int, TerritoryData)> territoriesData = new List<(int, TerritoryData)>();

        int count = territories.Count;
        for (int i = 0; i < count; i++)
        {
            //Get this factions territory data if it has it
            //If it doesn't throw a warning
            if (territories[i].GetData(Faction.Tags.Territory, out DataBase data))
            {
                //Get territory data
                territoriesData.Add((i, data as TerritoryData));
            }
            else
            {
                MonitorBreak.Bebug.Console.Log("No territory data found by territory init routine! Nowhere to put territory information!");
            }
        }

        int validTerritoriesCount = territoriesData.Count;
        int solarSystemRadius = (int)WorldManagement.GetSolarSystemRadius();
        Func<RealSpacePostion, bool> falloffPositionGenerationValidPositionCheck = delegate(RealSpacePostion pos) 
        {
            return WorldManagement.WithinSolarSystem(pos);
        };
        for (int i = 0; i < validTerritoriesCount; i++)
        {
            //Actually construct the data
            TerritoryData territoryData = territoriesData[i].Item2;

            if (territoryData.territoryCenters.Count == 0)
            {
                //This is one of the new territories
                //We need to pick an appropriate place for the territory start point
                //This means we need to query the map information (WorldManagement)

                //WE NEED TO REMOVE A LARGE PART OF THIS, THIS BELONGS IN A NATION INIT ROUTINE INSTEAD IF WE ARE BEING HONEST
                //OTHER FACTION TYPES MIGHT NEED TO CONTROL TERRITORY DATA BUT THIS GENERALIZED SOLUTION CREATES DEPENDICES WHICH DIRECTLY
                //BREAK THE FLEXIBILITY OF THE SYSTEM
                //ROUTINES SHOULD NOT KNOW ABOUT OTHER ROUTINES!

                int loopCheck = 0;
                //If no position has been set by a previous routine
                while (territoryData.origin == null && loopCheck < 1000)
                {
                    //Get potential position
                    double range = SimulationManagement.random.Next(0, solarSystemRadius);
                    Vector3 pos = GenerationUtility.GetCirclePositionBasedOnPercentage(SimulationManagement.random.Next(0, 100) / 100.0f, 1.0f);

                    territoryData.origin = new RealSpacePostion(pos.x * range, 0, pos.z * range);

                    //Clamp position to grid
                    territoryData.origin = WorldManagement.ClampPositionToGrid(territoryData.origin);

                    //if (AnyContains(territoriesData, territoryData.origin))
                    //{
                    //    //Set to null if position already occupied
                    //    territoryData.origin = null;
                    //}

                    loopCheck++;
                }

                if (territoryData.origin != null)
                {
                    int budget = 100;
                    //if (territoryData.hasClaimOpinions)
                    //{
                    //    //Do a random falloff spread across the game map
                    //    territoryData.territoryCenters = GenerationUtility.GenerateFalloffPositionSpread(
                    //        validStartPosition, //Start pos
                    //        new Vector2(0.01f, 0.1f), //Min max falloff per step
                    //        budget, //Max scale
                    //        falloffPositionGenerationValidPositionCheck,
                    //        SimulationManagement.random);
                    //}
                    //else
                    //{

                    //}
                }
                
                if (territoryData.origin == null || territoryData.territoryCenters.Count == 0)
                {
                    //Something major has gone wrong! We've filled up the board!
                    //Need to get the territories death data and tell it it needs to die!
                    if (territories[territoriesData[i].Item1].GetData(Faction.Tags.Faction, out DataBase data))
                    {
                        (data as FactionData).ForceDeath();
                        
                    }
                    else
                    {
                        MonitorBreak.Bebug.Console.Log("Big Error! No Faction data!");
                    }
                }
            }
        }
    }
}
