using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lowest priority routine
[SimulationManagement.ActiveSimulationRoutine(-3000)]
public class MetaRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);

        for(int i = 0; i < factions.Count;)
        {
            Faction faction = factions[i];

            if (faction.GetData(Faction.Tags.Faction, out FactionData factionData))
            {
                if (factionData.deathFlag)
                {
                    //Remove this faction from the simulation
                    SimulationManagement.RemoveFactionFully(faction);
                    continue; //Perform nothing else for this faction as it is now dead
                }
            }

            i++;
        }
    }
}
