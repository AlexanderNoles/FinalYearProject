using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(80)]
public class WarProcessRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> atWarFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.AtWar);

        //Need to get relationship data for each faction
        //And military data for each faction

        //We then calculate battles based on ships in each "battleground"
    }
}
