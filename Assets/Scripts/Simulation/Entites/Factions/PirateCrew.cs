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
		TargetableLocationData targetableLocationData = new TargetableLocationData("Pirate Crew", "", Color.red,
			(parent) =>
			{
				GeneratorManagement.AsteroidGeneration generation = new GeneratorManagement.AsteroidGeneration();
				generation.parent = parent;

				generation.SpawnAsteroid(Vector3.zero);
				return generation;
			});

		targetableLocationData.maxHealth = 100;

		AddData(DataTags.TargetableLocation, targetableLocationData);
	}
}
