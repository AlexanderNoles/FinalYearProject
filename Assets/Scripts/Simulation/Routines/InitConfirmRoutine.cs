using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManager.ActiveSimulationRoutine(-100, true)]
public class InitConfirmRoutine : RoutineBase
{
    //Runs after all other init routines to tell factions they are done initlizing

    public override bool Check(Faction faction)
    {
        return true;
    }

    public override void Run(Faction faction)
    {
        faction.hasRunInit = true;
    }
}
