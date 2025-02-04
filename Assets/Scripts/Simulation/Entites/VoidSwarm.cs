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
		AddData(DataTags.Territory, new TerritoryData());
		//

		//Create pre coloured emblem data
		EmblemData emblem = new EmblemData();
		emblem.hasCreatedEmblem = true;
		emblem.mainColour = Color.magenta;
		emblem.SetColoursBasedOnMainColour();

		AddData(DataTags.Emblem, emblem);
		//

		//Setup some inital military count
		VoidMilitary milData = new VoidMilitary();
		milData.initalCount = SimulationManagement.random.Next(30, 40);
		AddData(DataTags.Military, milData);
		//

		AddData(DataTags.Refinery, new RefineryData());

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.openlyHostile = true;

		AddData(DataTags.ContactPolicy, contactPolicyData);
	}

	//Testing functions
	[MonitorBreak.Bebug.ConsoleCMD("SpawnVoid", "Spawn a VoidSwarm Entity")]
	public static void SpawnVoidCMD()
	{
		new VoidSwarm().Simulate();
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


