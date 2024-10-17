using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManager.ActiveSimulationRoutine(-1)]
public class TestRoutine : RoutineBase
{
    public override void Run(Faction faction)
    {
        Console.Log("Test Routine Run");
    }
}
