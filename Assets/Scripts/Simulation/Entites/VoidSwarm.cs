using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Void swarms are unique in a couple ways

//They are intended to spawn at random from a rift
//If that rift is destroyed the entity is immediately destroyed
//They use a special type of ship that has it's power scale based on distance from the rift
//Meaning they quickly dominate around them but cannot spread across the whole solar system

//This rift should be modeled in a general way so it can be reused
//In a way this rift is similar to a pirate crew's base
//	- It acts as a central hub that destroys the entity if it is destroyed
//	- This central hub also acts as a production site for ships
//	- It sends out excursions of troops to attack places
//Only major differences are that void swarms hold territory (and consequently can't have the insignificant tag) and their unique ship power
//This model can be reused in tons of places, for example, an excursion team from outside the solar system sent by some larger power
//We can model ship control off ContactPolicy if we want
public class VoidSwarm : Faction
{
	public readonly EntitySpawnData antiVoidKnightSpawnData = new EntitySpawnData()
	{
		targets = new List<EntitySpawnData.EntityToSpawn>()
		{
			new EntitySpawnData.EntityToSpawn()
			{
				entityClassType = typeof(AntiVoidKnights),
				entityTag = EntityTypeTags.AntiVoidKnights,
				chance = 30,
				tendencyTowardsCap = 0.0f,
				totalMax = 2
			}
		}
	};

	public override void InitTags()
	{
		base.InitTags();

		AddTag(EntityTypeTags.VoidSwarm);
	}

	public override void InitData()
	{
		base.InitData();

		//Alter feelings data to make everyone hate this faction
		GetData(DataTags.Feelings, out FeelingsData feelingsData);
		feelingsData.baseFavourability = -2.0f;

		//Add rift
		VoidRift targetRift = new VoidRift();
		AddData(DataTags.TargetableLocation, targetRift);
		//

		//Give an initial static population
		PopulationData popData = new PopulationData();
		popData.variablePopulation = false;
		//Set to some arbitrarily high value as their population would be pulled from the void itself 
		popData.currentPopulationCount = int.MaxValue;

		AddData(DataTags.Population, popData);
		TerritoryData territoryData = new TerritoryData();
		territoryData.forceClaimInital = true;
		AddData(DataTags.Territory, territoryData);

		VoidDesirabilityData desirabilityData = new VoidDesirabilityData();
		desirabilityData.target = targetRift;
		desirabilityData.desirabilityBasis = territoryData;
		AddData(DataTags.Desirability, desirabilityData);
		//

		//Create pre coloured emblem data
		EmblemData emblem = new EmblemData();
		emblem.mainColour = Color.magenta;
		emblem.SetColoursBasedOnMainColour();

		AddData(DataTags.Emblem, emblem);
		//

		//Setup some inital military count
		VoidMilitary milData = new VoidMilitary();
		milData.initalCount = Mathf.RoundToInt(SimulationManagement.random.Next(90, 100) * BalanceManagement.voidSwarmStartTroopMultiplier);
		AddData(DataTags.Military, milData);

		AddData(DataTags.Strategy, new GenocidalStrategyData());
		//

		RefineryData refinery = new RefineryData();
		refinery.productionSpeed = 100.0f; //Very fast refinery
		refinery.autoFillFleets = true;
		AddData(DataTags.Refinery, refinery);

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.openlyHostile = true;

		AddData(DataTags.ContactPolicy, contactPolicyData);

		AddData(DataTags.EntitySpawner, antiVoidKnightSpawnData);
		AddData(DataTags.Timer, new TimerData(SimulationManagement.YearsToTickNumberCount(1)));
	}

	//Testing functions
	[MonitorBreak.Bebug.ConsoleCMD("SpawnVoid", "Spawn a VoidSwarm Entity")]
	public static void SpawnVoidCMD()
	{
		VoidSwarm swarm = new VoidSwarm();
		swarm.Simulate();

		MonitorBreak.Bebug.Console.Log($"Void Swarm with id {swarm.id} spawned!");
	}
}

public class VoidMilitary : MilitaryData
{
	public override ShipCollection GetNewFleet()
	{
		VoidFleet newFleet = new VoidFleet();
		newFleet.origin = origin;
		return newFleet;
	}
}

public class VoidDesirabilityData : TargetableLocationDesirabilityData
{
	public TerritoryData desirabilityBasis;

	public override void UpdateDesirability()
	{
		//If less then 20 cells held desirability is zero
		desirability = Mathf.Max(0, desirabilityBasis.territoryCenters.Count - 20);
	}
}

public class VoidRift : CentralBaseData
{
	public override Color GetMapColour()
	{
		return Color.magenta;
	}

	public override string GetTitle()
	{
		return "Void Rift";
	}

	public override Color GetFlashColour()
	{
		return Color.magenta;
	}

	public override bool FlashOnMap()
	{
		return true;
	}
}


