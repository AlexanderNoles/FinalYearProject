using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(-50, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class WarEffectRoutine : RoutineBase
{
	public override void Run()
	{
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.HasMilitary);

		foreach (Faction faction in factions)
		{
			if (faction.GetData(Faction.Tags.HasMilitary, out MilitaryData milData) && faction.GetData(Faction.Tags.CanFightWars, out WarData warData))
			{
				warData.warExhaustion += milData.totalDamageBuildup * warData.warExhaustionGrowthMultiplier;
				milData.totalDamageBuildup = 0.0f;
			}
		}
	}
}
