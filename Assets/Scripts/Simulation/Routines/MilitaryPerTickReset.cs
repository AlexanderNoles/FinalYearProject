using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(200)]
public class MilitaryPerTickReset : RoutineBase
{
	public override void Run()
	{
		//Get all factions with a military
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.HasMilitary);

		foreach (Faction faction in factions)
		{
			if (faction.GetData(Faction.Tags.HasMilitary, out MilitaryData militaryData))
			{
				militaryData.markedTransfers.Clear();
				militaryData.totalDamageBuildup = 0.0f;
			}
		}
	}
}
