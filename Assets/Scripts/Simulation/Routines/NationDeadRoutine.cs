using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.ActiveSimulationRoutine(-500, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class NationDeadRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all nations
		//If they have no territory they are dead
		List<SimulationEntity> nations = SimulationManagement.GetEntitiesViaTag(EntityTypeTags.Nation);

		for (int i = 0; i < nations.Count; i++)
		{
            SimulationEntity nation = nations[i];
			if (nation.GetData(DataTags.Territory, out TerritoryData territoryData))
			{
				if (territoryData.territoryCenters.Count == 0)
				{
					//Add death flag
					if (!nation.HasTag(EntityStateTags.Dead))
					{
						nation.AddTag(EntityStateTags.Dead);
					}
				}
			}
		}
	}
}
