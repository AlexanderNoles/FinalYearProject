using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Static script to help centralize balance
public static class BalanceManagement
{
	public const float killWorthRatio = 1f;
	public const float mineralDepositWorthRatio = 0.5f;

	//This acts as a general modifier to attack damage and regen basically
	//Might include other things in the future
	//(28/01/2025)
	public const float overallBattlePace = 0.4f;

	//How much dealing damage to entites affects your reputation?
	//Realisitcally one attack should make them hate you but I want to give players a bit of leeway
	public const float damageReputationRatio = 0.05f;

	//Void Swarm balancing, controls how much effect they have on the solar system
	//If these values are overtuned all other factions could be wiped out too easily
	//If they are too low the void swarm has zero impact
	public const float voidSwarmStartTroopMultiplier = 0.25f;
	public const float voidSwarmDamageFalloff = 1;

	//Used by entites to determine when they hate each other
	public const float oppositionThreshold = -0.7f;
	public const float purchaseAllowedThreshold = -0.1f;

	//Player population balancing
	public const float playerPopulationChangePerTick = 2.243f; //Arbitrarly randomized number to unsync population count from exact ticks
}