using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmediateTargetSpawnSourceData : SpawnSourceData
{
	public override void OnSpawn(int source)
	{
		base.OnSpawn(source);

		if (TryGetLinkedData(DataTags.Strategy, out StrategyData stratData) && stratData is TargetEntityTypeStrategy)
		{
			TargetEntityTypeStrategy targetEntityTypeStrategy = (TargetEntityTypeStrategy)stratData;
			targetEntityTypeStrategy.targetEntityID = source;
			targetEntityTypeStrategy.entityTypeTarget = GetSourceType();
		}
	}
}
