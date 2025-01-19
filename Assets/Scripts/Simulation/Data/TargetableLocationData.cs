using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : DataBase
{
	public class ActualLocation : VisitableLocation
	{
		public TargetableLocationData target;
		public GeneratorManagement.Generation generation;

		public override RealSpacePosition GetPosition()
		{
			return target.actualPosition;
		}

		public ActualLocation(TargetableLocationData target)
		{
			this.target = target;
		}

		public override string GetTitle()
		{
			return target.name;
		}

		public override string GetDescription()
		{
			return target.description;
		}

		public override Color GetMapColour()
		{
			return target.mapColour;
		}

		public override float GetEntryOffset()
		{
			return 100.0f;
		}

		public override void InitDraw(Transform parent)
		{
			generation = target.drawFunc.Invoke(parent);

			//Apply simulation context
			ApplyContext(generation.targets[^1].Item2);

			//Finalize
			generation.FinalizeGeneration();
		}

		public override void OnDeath()
		{
			//Remove from simulation
			target.parent.Get().AddTag(EntityStateTags.Dead);
		}

		public override void Cleanup()
		{
			generation.AutoCleanup();
		}

		public override float GetMaxHealth()
		{
			return target.maxHealth;
		}
	}

	public ActualLocation location;
	public RealSpacePosition cellCenter = null;
	public RealSpacePosition actualPosition;
	private string name;
	private string description;
	private Color mapColour;

	public float maxHealth;
	public int desirability = 1;
	public int lastTickTime;

	private Func<Transform, GeneratorManagement.Generation> drawFunc;

	public TargetableLocationData(string name, string description, Color mapColour, Func<Transform, GeneratorManagement.Generation> drawFunc)
	{
		this.name = name;
		this.description = description;
		this.mapColour = mapColour;

		this.drawFunc = drawFunc;
		location = new ActualLocation(this);
	}
}
