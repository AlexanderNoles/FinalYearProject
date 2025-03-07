using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiVoidKnights : Faction
{
	public override void InitEntityTags()
	{
		base.InitEntityTags();
		AddTag(EntityStateTags.Insignificant);
		AddTag(EntityTypeTags.AntiVoidKnights);
	}

	public override void InitData()
	{
		base.InitData();

		//These entity is very similar to pirate crews except they have different targets

		//Spawned by void swarms and so they will immediately begin to fight them
		AddData(DataTags.SpawnSource, new ImmediateTargetSpawnSourceData());

		PopulationData populationData = new PopulationData();
		populationData.variablePopulation = false;
		populationData.currentPopulationCount = SimulationManagement.random.Next(75, 150);

		AddData(DataTags.Population, populationData);
		//

		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Strategy, new TargetEntityTypeStrategy());
		AddData(DataTags.Refinery, new RefineryData());

		TerritoryData territoryData = new TerritoryData();
		territoryData.hardTerritoryCountLimit = 15;
		AddData(DataTags.Territory, territoryData);
		AddData(DataTags.TargetableLocation, new AntiVoidKnightBase());

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.visibleToAll = false;
		AddData(DataTags.ContactPolicy, contactPolicyData);
		AddData(DataTags.Political, new PoliticalData());

		//Set predetermined emblem color
		EmblemData emblem = new EmblemData();
		emblem.mainColour = Color.white * 0.7f;
		emblem.SlightlyRandomize(0.1f);
		emblem.SetColoursBasedOnMainColour();

		AddData(DataTags.Emblem, emblem);
		AddData(DataTags.Name, new AntiVoidKnightNameData());
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

public class AntiVoidKnightNameData : NameData
{
	string[] primaryNames = new string[]
	{
		"Moon",
		"Feather",
		"Fate",
		"Eagle",
		"Shield",
		"Blade",
		"Fate",
		"Dawn",
		"Star",
		"Iron",
		"Gold",
		"Bronze",
		"Diamond",
		"Sunrise",
		"Sunset",
		"Noon",
		"Lancer",
		"Mother",
		"Father",
		"Hunter"
	};

	public override void Generate()
	{
		baseName = 
			"The " + 
			primaryNames[SimulationManagement.random.Next(0, primaryNames.Length)] + 
			" " +
			primaryNames[SimulationManagement.random.Next(0, primaryNames.Length)] +
			" Order";
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
