using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The player faction represents the ship (sometimes reffered to as the monolith)
//The most important part of the ship is its population, most other player based systems are based around
//the population.
//For example, the population requires some amount of resources to survive so resources or money must be gained from somewhere.
//Or battles must be won so the population can survive so better weapons and technology must be sought out.
//Or alliances must be made so the population can survive.
//The majority of the game is based around keeping the population functioning.
public class PlayerFaction : Faction
{
	public override void InitTags()
	{
		base.InitTags();

		AddTag(Tags.Player);
		AddTag(Tags.Unkillable);
		AddTag(Tags.Population);
	}

	public override void InitData()
	{
		//No call to base method as this is a very atypical faction
		//Need to init data modules manually
		dataModules = new Dictionary<string, DataBase>();

		//Still add the faction data so fully general routines can access this faction
		//It still carries the unkillable tag so no data (as of 04/12/2024) within the faction data is currently used
		AddData(Tags.Faction, new FactionData());

		AddData(Tags.Population, new PopulationData());
	}
}
