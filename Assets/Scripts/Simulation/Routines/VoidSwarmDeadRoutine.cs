using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-510)]
public class VoidSwarmDeadRoutine : RoutineBase
{
	public override void Run()
	{
		if (SimulationManagement.GetEntityCount(EntityTypeTags.Nation) > 0)
		{
			return;
		}

		//No returns, the swarms have accomplished their task
		//Remove them all
		List<SimulationEntity> voidSwarms = SimulationManagement.GetEntitiesViaTag(EntityTypeTags.VoidSwarm);

		foreach (SimulationEntity entity in voidSwarms)
		{
			entity.AddTag(EntityStateTags.Dead);
		}
	}
}
