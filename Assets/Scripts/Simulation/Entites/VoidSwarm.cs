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
public class VoidSwarm : SimulationEntity
{
	public override void InitData()
	{
		base.InitData();

		//
	}
}
