using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManager.ActiveSimulationRoutine(-1)]
public class TestRoutine : RoutineBase
{
    int testCounter = 0;

    public override void Run()
    {
        testCounter++;

        List<Faction> factions = SimulationManager.GetAllFactionsWithTag(Faction.Tags.Faction);
        Console.Log(factions.Count);
        List<Faction> nations = SimulationManager.GetAllFactionsWithTag(Faction.Tags.Nation);
        Console.Log(nations.Count);

        if (testCounter % 5 == 0)
        {
            Console.Log("Created New Nation");
            new Nation().Simulate();
        }
    }
}
