using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : VisitableLocation
{
	public RealSpacePosition cellCenter = null;
	public RealSpacePosition actualPosition;
	public GeneratorManagement.Generation generation;
	public int desirability = 1;
	public int lastTickTime;

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

		//Apply simulation context
		LinkToBehaviour(generation.targets[^1].Item2);

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
		SimulationManagement.RemoveEntityFromSimulation(parent.Get());
	}

	public override void Cleanup()
	{
		generation.AutoCleanup();
	}
}
