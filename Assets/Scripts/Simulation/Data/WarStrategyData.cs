using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarStrategyData : StrategyData
{
	//Increase based on the number of lost battles
	//A higher number means war has a larger negative effect on the country
	public float warExhaustion = 0;
	//This value should be made higher or lower based on how susceptible a faction is to war exhaustion
	//Certain factions might set this to zero (such as a fully robotic army).
	public float warExhaustionGrowthMultiplier = 0.05f;

	public List<int> atWarWith = new List<int>();

	public override List<int> GetTargets()
	{
		return atWarWith;
	}

	public override float GetDefensivePropensity()
	{
		//Increase defensive propensity with war exhaustion
		return base.GetDefensivePropensity() * warExhaustion;
	}
}
