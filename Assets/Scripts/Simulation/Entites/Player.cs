using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

//The player's corresponding simulation entity, stores a lot of data about the player
public class Player : SimulationEntity
{
	public override void InitTags()
	{
		base.InitTags();

		AddTag(EntityTypeTags.Player);
		AddTag(EntityStateTags.Insignificant);
		AddTag(EntityStateTags.Unkillable);
	}

	public override void InitData()
	{
		base.InitData();

		AddData(DataTags.Emblem, new EmblemData());

		PlayerStats stats = new PlayerStats();
		stats.Init();
		AddData(DataTags.Stats, stats);

		PlayerInventory inventory = new PlayerInventory();
		inventory.SetStatsTarget(stats);
		AddData(DataTags.Inventory, inventory);

		PlayerInteractions interactions = new PlayerInteractions();
		//Add basic interactions that all players have
		//The order of interactions matters as the smart interaction will
		//iterate through them in that order
		interactions.playersInteractions.Add(new TravelInteraction().Init());
		interactions.playersInteractions.Add(new ShopInteraction().Init());
		interactions.playersInteractions.Add(new AttackInteraction().Init());
		//
		AddData(DataTags.Interactions, interactions);
		
		FeelingsData feelings = new FeelingsData();
		feelings.baseFavourability = 0.0f;
		AddData(DataTags.Feelings, feelings);
		AddData(DataTags.ContactPolicy, new ContactPolicyData());

		AddData(DataTags.Policies, new PoliciesData().Init());

		//Give the player population
		//This is a very important balancing statistic
		PopulationData population = new PopulationData();
		population.currentPopulationCount = BalanceManagement.playerPopulationStartValue;
		population.populationNaturalGrowthLimt = BalanceManagement.intialPlayerPopulationMax;
		population.populationNaturalGrowthSpeed = BalanceManagement.playerPopulationChangePerTick;
		//Because the player will be traveling in the warp their population growth shouldn't be affected by the increased speed
		population.growthAffectedBySimSpeed = false;
		AddData(DataTags.Population, population);

		//Give the player a military
		MilitaryData militaryData = new MilitaryData();
		militaryData.initalCount = BalanceManagement.initalMilitaryCount;
		militaryData.selfControlled = true;
		AddData(DataTags.Military, militaryData);

		//Give the player a refinery
		RefineryData refineryData = new RefineryData();
		refineryData.putInReserves = true;
		refineryData.productionAffectedBySimSpeed = false;
		refineryData.productionSpeed = 0.5f; //The player has a super fast refinery!
		refineryData.autoFillFleets = true;
		AddData(DataTags.Refinery, refineryData);
	}
}
