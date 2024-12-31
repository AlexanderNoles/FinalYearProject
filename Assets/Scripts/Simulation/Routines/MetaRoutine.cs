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
        List<Faction> deadFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Dead);

        List<int> idsOfRemovedFactions = new List<int>();

        for(int i = 0; i < deadFactions.Count;)
        {
            Faction faction = deadFactions[i];

            if (faction.HasTag(Faction.Tags.Dead) && !faction.HasTag(Faction.Tags.Unkillable))
            {
                //Remove this faction from the simulation
                SimulationManagement.RemoveFactionFully(faction);
                idsOfRemovedFactions.Add(faction.id);
                //Perform nothing else for this faction as it is now dead (including incrementing the index)
                continue; 
            }

            i++;
        }

        if (idsOfRemovedFactions.Count > 0)
        {
            //Do cleanup
			//This cleanup is only done for generic things that all factions should have
			//Specific systems implement evaluation functions that typically do other things alongside removal

            //Factions are now removed from the factions list
            //So we can just iterate through that
            //They are removed automatically because GetAllFactionsWithTag really just returns a pointer to the actual list
            //Which RemoveFactionFully takes them out of
            foreach (Faction faction in factions)
            {
                if (faction.GetData(Faction.relationshipDataKey, out FeelingsData relationshipData))
                {
					foreach (int id in idsOfRemovedFactions)
					{
						relationshipData.idToFeelings.Remove(id);
					}
                }
            }

			//Check current battle data and remove them from that
			//Get global data
			SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0].GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

			foreach (KeyValuePair<RealSpacePostion, GlobalBattleData.Battle> battleEntry in globalBattleData.battles)
			{
				foreach (int id in idsOfRemovedFactions)
				{
					battleEntry.Value.RemoveInvolvedFaction(id);
				}
			}
		}
    }
}
