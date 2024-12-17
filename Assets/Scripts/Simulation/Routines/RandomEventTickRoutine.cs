using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(-5000, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class RandomEventTickRoutine : RoutineBase
{
	public override void Run()
	{
		int currentTickRandomValue = SimulationManagement.GetSimulationSeed() + SimulationManagement.currentTickID;

		if (currentTickRandomValue % SimulationManagement.YearsToTickNumberCount(5) == 0)
		{
			//Every 5 years

			float newFactionChance = 25 + (Mathf.Max(4 - SimulationManagement.GetFactionCount(), 0) * 26);
			if (SimulationManagement.random.Next(0, 101) < newFactionChance)
			{
				//Spawn random faction
				//Right now we just make a nation
				new Nation().Simulate();
			}
		}

		if (currentTickRandomValue % SimulationManagement.MonthToTickNumberCount(1) == 0)
		{
			//Every month
			List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

			//For every nation we have some chance to spawn offshoots
			foreach (Faction nation in nations)
			{
				if (SimulationManagement.random.Next(0, 101) < 1)
				{
					//Spawn some pirates
					new PirateCrew().Simulate();
				}
			}
		}
	}
}
