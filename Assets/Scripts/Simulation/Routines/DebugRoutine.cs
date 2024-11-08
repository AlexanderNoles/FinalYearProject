using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(-4000, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Debug)]
public class DebugRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);

        factions[0].GetData(Faction.relationshipDataKey, out RelationshipData data);
        Console.Log(data.idToRelationship.Count);
    }
}
