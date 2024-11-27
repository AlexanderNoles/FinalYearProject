using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(-4000, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Debug)]
public class DebugRoutine : RoutineBase
{
    public override void Run()
	{
		List<Faction> populatedFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Population);

		Console.Log(">>>>>>>>>");

		foreach (Faction faction in populatedFactions)
		{
			if (faction.GetData(Faction.Tags.Population, out PopulationData data))
			{
				Console.Log(data.currentPopulationCount);
			}
		}
	}
}
