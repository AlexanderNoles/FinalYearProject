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

        List<int> idsOfRemovedFactions = new List<int>();

        for(int i = 0; i < factions.Count;)
        {
            Faction faction = factions[i];

            if (faction.GetData(Faction.Tags.Faction, out FactionData factionData))
            {
                if (factionData.deathFlag)
                {
                    //Remove this faction from the simulation
                    SimulationManagement.RemoveFactionFully(faction);
                    idsOfRemovedFactions.Add(faction.id);
                    continue; //Perform nothing else for this faction as it is now dead (including incrementing the index)
                }
            }

            i++;
        }

        if (idsOfRemovedFactions.Count > 0)
        {
            //Do cleanup

            //Factions are now removed from the factions list
            //So we can just iterate through that
            //They are removed automatically because GetAllFactionsWithTag really just returns a pointer to the actual list
            //Which RemoveFactionFully takes them out of
            foreach (Faction faction in factions)
            {
                if (faction.GetData(Faction.relationshipDataKey, out RelationshipData relationshipData))
                {
                    relationshipData.idToRelationship.Remove(faction.id);
                }
            }
        }
    }
}
