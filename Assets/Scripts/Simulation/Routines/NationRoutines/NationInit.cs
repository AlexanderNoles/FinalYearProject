using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManager.ActiveSimulationRoutine(0, true)]
public class NationInit : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return tags.Contains(Faction.Tags.Nation);
    }

    public override void Run()
    {
        List<Faction> nations = SimulationManager.GetAllFactionsWithTag(Faction.Tags.Nation);
    }
}
