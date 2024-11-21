using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(3000, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class FactionInit : InitRoutineBase
{
    private static int nextID = -1;

    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return true;
    }

    public override void Run()
    {
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);

        if (nextID == -1)
        {
            //This only occurs the first time an id needs to be assigned this game
            //If this is loaded from a save than nextID should be loaded as well so it won't be -1
            //if we did load and it's still -1 this will run anyway then as a fail safe

            foreach (Faction faction in factions)
            {
                if (faction.id > nextID)
                {
                    nextID = faction.id;
                }
            }

            nextID++;
        }

        foreach (Faction faction in factions)
        {
            if (faction.id == -1)
            {
                faction.id = nextID;
                nextID++;
            }
        }
    }
}
