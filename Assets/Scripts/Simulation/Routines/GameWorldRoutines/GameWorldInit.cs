using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

//High priority so world runs before all other init ticks
[SimulationManagement.ActiveSimulationRoutine(100, true)]
public class GameWorldInit : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return tags.Contains(Faction.Tags.GameWorld);
    }

    public override void Run()
    {

    }
}
