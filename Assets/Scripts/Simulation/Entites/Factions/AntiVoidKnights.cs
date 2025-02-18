using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiVoidKnights : Faction
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(EntityStateTags.Insignificant);
		AddTag(EntityTypeTags.AntiVoidKnights);
	}

	public override void InitData()
	{
		base.InitData();

		//These entity is very similar to pirate crews except they have different targets

		//Spawned by void swarms and so they will immediately begin to fight them
		AddData(DataTags.SpawnSource, new ImmediateTargetSourceSpawnSourceData());

		PopulationData populationData = new PopulationData();
		populationData.variablePopulation = false;
		populationData.currentPopulationCount = SimulationManagement.random.Next(75, 150);

		AddData(DataTags.Population, populationData);
		//

		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Strategy, new TargetEntityTypeStrategy());
		AddData(DataTags.Refinery, new RefineryData());

		AddData(DataTags.Territory, new TerritoryData());
		AddData(DataTags.TargetableLocation, new AntiVoidKnightBase());

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.visibleToAll = false;
		AddData(DataTags.ContactPolicy, contactPolicyData);

		//Set predetermined emblem color
		EmblemData emblem = new EmblemData();
		emblem.mainColour = Color.white * 0.7f;
		emblem.SlightlyRandomize(0.1f);
		emblem.SetColoursBasedOnMainColour();

		AddData(DataTags.Emblem, emblem);
	}
}

public class AntiVoidKnightBaseWeapon : StandardSimWeaponProfile
{
	public override float GetDamageRaw()
	{
		return 1;
	}

	public override float GetTimeBetweenAttacks()
	{
		return 0.25f;
	}
}

public class AntiVoidKnightBase : CentralBaseData
{
	private readonly List<StandardSimWeaponProfile> weapons = new List<StandardSimWeaponProfile>()
	{
		new AntiVoidKnightBaseWeapon()
	};


	//Battle
	public override float GetMaxHealth()
	{
		return 250.0f;
	}

	public override float GetKillReward()
	{
		//Riches!
		return 350.0f;
	}

	public override List<StandardSimWeaponProfile> GetWeapons()
	{
		return weapons;
	}

	//Display
	public override string GetTitle()
	{
		return "Anti Void";
	}

	public override Color GetMapColour()
	{
		return Color.grey;
	}

	//Draw
	//(DEBUG: Just reusing the pirate crew base draw function for now)
	public override GeneratorManagement.Generation Draw(Transform parent)
	{
		GeneratorManagement.StructureGeneration generation = new GeneratorManagement.StructureGeneration();
		generation.parent = parent;

		generation.SpawnStructure(GeneratorManagement.POOL_INDEXES.PIRATEBASE, Vector3.zero);
		return generation;
	}
}
