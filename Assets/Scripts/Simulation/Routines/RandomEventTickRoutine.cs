using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		//
		//The above is not handle using that data module as I wanted it to work apart from specific data modules
		//(a.k.a it would be annoying to make it generalized and wouldn't give us much in return in my opnion)
		//(for example, one of the major things is wanting to spawn new nations regardless of a parent nation, with data being tied to entities the answer would be to tie it to the game world I guess?)
		//(but then we would have to specify for the gw to not copy political and emblem data) (we would also have to specify for nations that that's the data we want to copy) 
		//

		//Process entity spawn datas
		List<DataModule> entitySpawners = SimulationManagement.GetDataViaTag(DataTags.EntitySpawner);

		foreach (EntitySpawnData spawner in entitySpawners.Cast<EntitySpawnData>())
		{
			foreach (EntitySpawnData.EntityToSpawn entity in spawner.targets)
			{
				//Is on the month interval
				if (currentTickRandomValue % SimulationManagement.MonthToTickNumberCount(entity.monthFrequency) == 0)
				{
					for (int i = 0; i < entity.countPer; i++)
					{
						//No max or space left
						if (entity.totalMax < 0 || SimulationManagement.GetEntityCount(entity.entityTag) < entity.totalMax)
						{
							float modifier = 0;
							if (entity.totalMax > 0)
							{
								//Amount of entites left to spawn to reach cap
								modifier = entity.totalMax - SimulationManagement.GetEntityCount(entity.entityTag);
								modifier *= entity.tendencyTowardsCap;
								modifier = Mathf.Max(modifier, 0);
							}

							if (SimulationManagement.random.Next(0, 101) < entity.chance + modifier)
							{
								//Spawn entity
								SimulationEntity newEntity = Activator.CreateInstance(entity.entityClassType) as SimulationEntity;
								//Begin simulating entity
								newEntity.Simulate();

								if (newEntity.GetData(DataTags.SpawnSource, out SpawnSourceData spawnSourceData))
								{
									//Tell a spawn source data module that it has been spawned
									spawnSourceData.OnSpawn(spawner.parent.Get().id);
								}
							}
						}
					}
				}
			}
		}
	}
}
