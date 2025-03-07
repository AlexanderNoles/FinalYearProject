using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PirateCrew : Faction
{
	public override void InitEntityTags()
	{
		base.InitEntityTags();
		AddTag(EntityStateTags.Insignificant);
		AddTag(EntityTypeTags.PirateCrew);
	}

	public override void InitData()
	{
		base.InitData();

		AddData(DataTags.SpawnSource, new ImmediateTargetSpawnSourceData());

		//Give pirate crew population so they can have a military (i.e., have people to put into the military)
		PopulationData populationData = new PopulationData();
		populationData.variablePopulation = false; //Don't allow population change
		populationData.currentPopulationCount = SimulationManagement.random.Next(20, 51);

		AddData(DataTags.Population, populationData);
		//

		AddData(DataTags.Military, new MilitaryData());
		TargetEntityTypeStrategy strats = new TargetEntityTypeStrategy();
		strats.removeTerritory = false; //Pirate crews don't take territory
		strats.attackPositionUncertainty = 0.5f;
		AddData(DataTags.Strategy, strats);
		AddData(DataTags.Refinery, new RefineryData());
		TargetableLocationData targetableLocationData = new PirateCrewBaseLocation();
		TargetableLocationDesirabilityData desirabilityData = new TargetableLocationDesirabilityData();
		desirabilityData.target = targetableLocationData;

		AddData(DataTags.Desirability, desirabilityData);
		AddData(DataTags.TargetableLocation, targetableLocationData);

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.visibleToAll = false;
		contactPolicyData.openlyHostile = true;

		AddData(DataTags.ContactPolicy, contactPolicyData);

		//Set predetermined emblem colour
		EmblemData emblem = new EmblemData();
		emblem.mainColour = Color.red * 0.9f;
		emblem.SetColoursBasedOnMainColour();

		AddData(DataTags.Emblem, emblem);
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

	public class PirateCrewBaseLocation : CentralBaseData
	{
		private readonly List<StandardSimWeaponProfile> weapons = new List<StandardSimWeaponProfile>()
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
