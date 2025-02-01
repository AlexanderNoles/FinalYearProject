using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : DataModule
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

		public override void InitDraw(Transform parent, PlayerLocationManagement.DrawnLocation drawnLocation)
		{
			generation = target.drawFunc.Invoke(parent);

			//Apply simulation context
			LinkToBehaviour(generation.targets[^1].Item2);

			//Finalize
			generation.FinalizeGeneration();
		}

		public override void OnDeath()
		{
			//Remove from simulation
			SimulationManagement.RemoveEntityFromSimulation(target.parent.Get());
		}

		public override void Cleanup()
		{
			generation.AutoCleanup();
		}

		public override float GetMaxHealth()
		{
			return target.maxHealth;
		}

		public override List<StandardSimWeaponProfile> GetWeapons()
		{
			return target.weapons;
		}

		public override int GetEntityID()
		{
			return target.parent.Get().id;
		}

		public override float GetKillReward()
		{
			return target.killReward;
		}
	}

	public ActualLocation location;
	public RealSpacePosition cellCenter = null;
	public RealSpacePosition actualPosition;
	public string name;
	public string description;
	public Color mapColour;

	public float maxHealth;
	public float killReward;
	public int desirability = 1;
	public int lastTickTime;
	public List<StandardSimWeaponProfile> weapons = new List<StandardSimWeaponProfile>();

	public Func<Transform, GeneratorManagement.Generation> drawFunc;

	public TargetableLocationData()
	{
		location = new ActualLocation(this);
	}
}
