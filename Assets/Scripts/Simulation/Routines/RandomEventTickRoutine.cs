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

			//Try to spawn new nations
			//By default have a low chance of each nation trying to spawn a new sub nation
			//(but limit to only one per tick)

			//Otherwise just spawn a brand new nation

			//Inital chance
			float newNationChance = 25 + (Mathf.Max(4 - SimulationManagement.GetEntityCount(EntityTypeTags.Nation), 0) * 26);

			if (SimulationManagement.random.Next(0, 101) < newNationChance)
			{
				Nation parent = null;
				int highestChance = int.MinValue;
				List<SimulationEntity> entities = SimulationManagement.GetEntitiesViaTag(EntityTypeTags.Nation);
					
				//Iterate through nations
				foreach (SimulationEntity entity in entities)
				{
					//Give a higher preference to nations that own more territory
					int modifier = 0;
					if (entity.GetData(DataTags.Territory, out TerritoryData territoryData))
					{
						modifier = territoryData.territoryCenters.Count;
					}
					//

					int chance = SimulationManagement.random.Next(0, 101) + (modifier / 5);
					if (chance > 110)
					{
						//Take the highest chance to not give preference to the first nations
						if (chance > highestChance)
						{
							highestChance = chance;
							parent = entity as Nation;
						}
					}
				}
				//

				//Create the new nation
				Nation newNation = new Nation();
				newNation.Simulate(); //Begin simulating

				//If a parent nation was found then copy political and emblem data
				if (parent != null)
				{
					//Emblem data
					newNation.ReplaceData(DataTags.Emblem, DataModule.ShallowCopy<EmblemData>(parent.GetDataDirect<EmblemData>(DataTags.Emblem)));

					EmblemData newEmblem = newNation.GetDataDirect<EmblemData>(DataTags.Emblem);
					newEmblem.SlightlyRandomize();
					//

					//Political data
					newNation.ReplaceData(DataTags.Political, DataModule.ShallowCopy<PoliticalData>(parent.GetDataDirect<PoliticalData>(DataTags.Political)));
					//

					//Give random feelings towards parent
					FeelingsData feelingsData = newNation.GetDataDirect<FeelingsData>(DataTags.Feelings);
					feelingsData.idToFeelings[parent.GetEntityID()] = new FeelingsData.Relationship(parent.GetDataDirect<FeelingsData>(DataTags.Feelings).baseFavourability);
					feelingsData.idToFeelings[parent.GetEntityID()].favourability += SimulationManagement.random.Next(-50, 51) / 50.0f;
					//
				}
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
					PirateCrew pirateCrew = new PirateCrew();
					pirateCrew.Simulate();

					//Get the new pirate crews strategy and make them target their original nation
					TargetEntityTypeStrategy targetEntityTypeStrategy = pirateCrew.GetDataDirect<TargetEntityTypeStrategy>(DataTags.Strategy);
					targetEntityTypeStrategy.targetEntityID = nation.id;
					targetEntityTypeStrategy.entityTypeTarget = EntityTypeTags.Nation;
				}
			}

			const int maxMineralCount = 75;
			if (SimulationManagement.GetEntityCount(EntityTypeTags.MineralDeposit) < maxMineralCount)
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
