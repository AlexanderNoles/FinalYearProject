using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : VisitableLocation
{
	public static Dictionary<RealSpacePosition, List<TargetableLocationData>> targetableLocationLookup = new Dictionary<RealSpacePosition, List<TargetableLocationData>>();

	public RealSpacePosition cellCenter = null;
	public RealSpacePosition actualPosition;
	public GeneratorManagement.Generation generation;

	public void OnPositionSet()
	{
		//Add to lookup
		if (cellCenter == null)
		{
			return;
		}

		if (!targetableLocationLookup.ContainsKey(cellCenter))
		{
			targetableLocationLookup[cellCenter] = new List<TargetableLocationData>();
		}

		targetableLocationLookup[cellCenter].Add(this);
	}

	public override void OnRemove()
	{
		//Remove from lookup
		if (targetableLocationLookup.ContainsKey(cellCenter))
		{
			targetableLocationLookup[cellCenter].Remove(this);

			if (targetableLocationLookup[cellCenter].Count == 0)
			{
				targetableLocationLookup.Remove(cellCenter);
			}
		}
	}

	public override RealSpacePosition GetPosition()
	{
		return actualPosition;
	}

	public override float GetEntryOffset()
	{
		return 100.0f;
	}

	public override void InitDraw(Transform parent, PlayerLocationManagement.DrawnLocation drawnLocation)
	{
		//Call draw function
		generation = Draw(parent);

		if (generation.targets.Count > 0)
		{
			//Apply simulation context
			LinkToBehaviour(generation.targets[^1].Item2);
		}

		//Finalize
		generation.FinalizeGeneration();
	}

	public virtual GeneratorManagement.Generation Draw(Transform parent)
	{
		//Draw nothing by default
		return new GeneratorManagement.Generation();
	}

	public override void OnDeath()
	{
		//Remove from simulation

		//This causes death to happen with no delay but causes an issue where cleanup is not done correctly!
		//SimulationManagement.RemoveEntityFromSimulation(parent.Get());
		parent.Get().AddTag(EntityStateTags.Dead);
	}

	public override void Cleanup()
	{
		generation.AutoCleanup();
	}
}
