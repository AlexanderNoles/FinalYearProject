using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetEntityTypeStrategy : StrategyData
{
	public EntityTypeTags entityTypeTarget;
	public int targetEntityID;

	public override List<int> GetTargets()
	{
		if (!SimulationManagement.EntityExists(targetEntityID))
		{
			//Entity no longer exists
			//Pick a new random nation to target
			List<SimulationEntity> targets = SimulationManagement.GetEntitiesViaTag(entityTypeTarget);

			if (targets.Count > 0)
			{
				//Get random
				targetEntityID = targets[SimulationManagement.random.Next(0, targets.Count)].id;
			}
			else
			{
				//No targets if no entites of type
				return new List<int>();
			}
		}

		return new List<int>() { targetEntityID };
	}
}
