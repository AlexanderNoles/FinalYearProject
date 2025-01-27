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
		TargetableLocationData targetableLocationData = new TargetableLocationData();

		targetableLocationData.name = "Pirate Crew";
		targetableLocationData.description = "";
		targetableLocationData.mapColour = Color.red;
		targetableLocationData.maxHealth = 100;
		targetableLocationData.weapons = new List<WeaponBase> { new PirateCrewBaseWeapon() };

		targetableLocationData.drawFunc = (parent) =>
		{
			GeneratorManagement.StructureGeneration generation = new GeneratorManagement.StructureGeneration();
			generation.parent = parent;

			generation.SpawnStructure(GeneratorManagement.POOL_INDEXES.PIRATEBASE, Vector3.zero);
			return generation;
		};

		AddData(DataTags.TargetableLocation, targetableLocationData);
	}

	public class PirateCrewBaseWeapon : WeaponBase
	{
		public override float GetDamageRaw()
		{
			return 0.5f;
		}
	}
}
