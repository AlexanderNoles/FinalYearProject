using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// (Copied from private discord)
/// 
/// Mineral deposits as a case study of the game's design:
/// - Mineral deposits give gold
/// - They are obvious points on the map
/// - Other simulation entites interact with this one
/// This results in a couple major things:
/// 1 - Mineral deposits are not worth mining post early game (I think this is good, cause they would be fairly boring to grind out)
/// 2 - Their position on the map make them obvious to players, this means they could have some negative repurcussions. Tightly lowering the amount of gold they give could better indicate they are not super worth doing?
/// 3 - Middle to late game, they can be used as ambush points for other factions trying to take them allowing you to kill enemies for a much larger reward (Without the pressure of Settlements)
///  
/// Point 3 is kind of an ideal interaction, this is what we want the game to be about.
/// Also that click moment of realizing the mineral deposits true purpose could be pretty impactful. Though that comes with the cost of a player just dismissing them out of hand for the entire playtime cause the actual mining is not super worth doing
/// Indicating the battle state with a glow and not just on hover over could be helpful
/// </summary>

public class MineralDeposit : SimulationEntity
{
	public static int totalMineralCount;

	public override void Simulate()
	{
		base.Simulate();

		totalMineralCount++;
	}

	public override void OnDeath()
	{
		base.OnDeath();

		totalMineralCount--;
	}

	public override void InitTags()
	{
		base.InitTags();
		AddTag(EntityStateTags.Insignificant);
	}

	public override void InitData()
	{
		base.InitData();
		//Allows the mineral depoist to pick fights
		//It has no military so it cannot fight back
		AddData(DataTags.Battle, new BattleData());
		TargetableLocationData targetableLocationData = new MineralDepositLocation();
		//give this location a random desirability
		float t = SimulationManagement.random.Next(0, 101) / 100.0f;
		targetableLocationData.desirability = Mathf.CeilToInt(Mathf.Lerp(1, 31, Mathf.Pow(t, 3)));

		AddData(DataTags.TargetableLocation, targetableLocationData);

		ContactPolicyData contactPolicyData = new ContactPolicyData();
		contactPolicyData.openlyHostile = true;

		AddData(DataTags.ContactPolicy, contactPolicyData);
	}
}

public class MineralDepositLocation : TargetableLocationData
{
	public override string GetTitle()
	{
		return "Ore Deposit";
	}

	public override Color GetMapColour()
	{
		return Color.green;
	}

	public override GeneratorManagement.Generation Draw(Transform parent)
	{
		GeneratorManagement.AsteroidGeneration generation = new GeneratorManagement.AsteroidGeneration();
		generation.parent = parent;
		generation.SpawnAsteroid(Vector3.zero);

		return generation;
	}

	public override float GetMaxHealth()
	{
		return 50.0f * desirability;
	}
}
