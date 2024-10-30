using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManager.ActiveSimulationRoutine(0, true)]
public class NationInit : RoutineBase
{
    public override bool Check(Faction faction)
    {
        return faction.GetFactionData().HasTag(FactionData.Tags.Nation);
    }

    public override void Run(Faction faction)
    {

    }
}
