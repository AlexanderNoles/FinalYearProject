using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PirateCrew : Faction
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(EntityStateTags.Insignificant);
	}

	public override void InitData()
	{
		base.InitData();
		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Emblem, new EmblemData());
		TargetableLocationData targetableLocationData = new PirateCrewBaseLocation();
		TargetableLocationDesirabilityData desirabilityData = new TargetableLocationDesirabilityData();
		desirabilityData.target = targetableLocationData;

		AddData(DataTags.Desirability, desirabilityData);
		AddData(DataTags.TargetableLocation, targetableLocationData);

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.openlyHostile = true;

		AddData(DataTags.ContactPolicy, contactPolicyData);
	}

	public class PirateCrewBaseWeapon : StandardSimWeaponProfile
	{
		public override float GetDamageRaw()
		{
			return 0.5f;
		}

		public override float GetTimeBetweenAttacks()
		{
			return 0.25f;
		}
	}

	public class PirateCrewBaseLocation : TargetableLocationData
	{
		private List<StandardSimWeaponProfile> weapons = new List<StandardSimWeaponProfile>()
		{
			new PirateCrewBaseWeapon()
		};

		public override string GetTitle()
		{
			return "Pirate Base";
		}

		public override Color GetMapColour()
		{
			return Color.red;
		}

		public override float GetMaxHealth()
		{
			return 100.0f;
		}

		public override float GetKillReward()
		{
			return 250.0f;
		}

		public override List<StandardSimWeaponProfile> GetWeapons()
		{
			return weapons;
		}

		public override GeneratorManagement.Generation Draw(Transform parent)
		{
			GeneratorManagement.StructureGeneration generation = new GeneratorManagement.StructureGeneration();
			generation.parent = parent;

			generation.SpawnStructure(GeneratorManagement.POOL_INDEXES.PIRATEBASE, Vector3.zero);
			return generation;
		}
	}
}
