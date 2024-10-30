using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

//High priority so world runs before all other init ticks
[SimulationManager.ActiveSimulationRoutine(100, true)]
public class GameWorldInit : RoutineBase
{
    public override bool Check(Faction faction)
    {
        return faction.GetFactionData().HasTag(FactionData.Tags.GameWorld);
    }

    public override void Run(Faction faction)
    {
        GameWorld gameWorld = faction as GameWorld;
        GameWorldData data = gameWorld.GetFactionData() as GameWorldData;
    }
}
