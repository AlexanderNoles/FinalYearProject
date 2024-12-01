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
			if (SimulationManagement.random.Next(0, 101) > 25)
			{
				//Spawn random faction
				//Right now we just make a nation
				new Nation().Simulate();
			}
		}
	}
}
