using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(110, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class PoliticalInitRoutine : InitRoutineBase
{
	public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
	{
		return tags.Contains(Faction.Tags.Politics);
	}

	public override void Run()
	{
		//Get all politically involved factions
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Politics);

		foreach (Faction faction in factions)
		{
            if (faction.GetData(Faction.Tags.Politics, out PoliticalData data))
            {
				if (data.authorityAxis == 0 && data.economicAxis == 0)
				{
					data.economicAxis = SimulationManagement.random.Next(-100, 101) / 100.0f;
					data.authorityAxis = SimulationManagement.random.Next(-100, 101) / 100.0f;
				}
            }
        }
	}
}
