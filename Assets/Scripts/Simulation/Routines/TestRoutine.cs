using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

//[SimulationManager.ActiveSimulationRoutine(-1)]
public class TestRoutine : RoutineBase
{
    int testCounter = 0;

    public override void Run()
    {
        testCounter++;

        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);
        Console.Log(factions.Count);
        List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);
        Console.Log(nations.Count);

        if (testCounter % 5 == 0)
        {
            //Every 5 tickls create a new nation
            Console.Log("Created New Nation");
            new Nation().Simulate();
        }
    }
}
