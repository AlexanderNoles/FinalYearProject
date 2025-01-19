using System.Collections;
using System.Collections.Generic;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-5000, SimulationManagement.SimulationRoutine.RoutineTypes.Normal)]
public class RandomEventTickRoutine : RoutineBase
{
	public override void Run()
	{
		int currentTickRandomValue = SimulationManagement.GetSimulationSeed() + SimulationManagement.currentTickID;

		if (currentTickRandomValue % SimulationManagement.YearsToTickNumberCount(5) == 0)
		{
			//Every 5 years

			float newEntityChance = 25 + (Mathf.Max(4 - SimulationManagement.GetEntityCount(), 0) * 26);
			if (SimulationManagement.random.Next(0, 101) < newEntityChance)
			{
				//Spawn random entity
				//Right now we just make a nation
				new Nation().Simulate();
			}
		}

		if (currentTickRandomValue % SimulationManagement.MonthToTickNumberCount(1) == 0)
		{
			//Every month
			List<SimulationEntity> nations = SimulationManagement.GetEntitiesViaTag(EntityTypeTags.Nation);

			//For every nation we have some chance to spawn offshoots
			foreach (SimulationEntity nation in nations)
			{
				//Could make this based on nation's internal political state
				if (SimulationManagement.random.Next(0, 101) < 1)
				{
					//Spawn some pirates
					new PirateCrew().Simulate();
				}
			}

			const int maxMineralCount = 75;
			if (MineralDeposit.totalMineralCount < maxMineralCount)
			{
				int mineralDepositCountPerMonth = 5;
				for (int i = 0; i < mineralDepositCountPerMonth; i++)
				{
					if (SimulationManagement.random.Next(0, 101) < 30)
					{
						//Spawn a new mineral deposit
						new MineralDeposit().Simulate();
					}
				}
			}
		}
	}
}
