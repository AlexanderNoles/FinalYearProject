using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : Faction
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(Tags.Territory);
        AddTag(Tags.Settlements);
        AddTag(Tags.Nation);
        AddTag(Tags.Emblem);
        AddTag(Tags.Population);
		AddTag(Tags.HasMilitary);
		AddTag(Tags.Politics);
        //This faction is visible to all other factions
        AddTag(Tags.OpenRelationship);
		AddTag(Tags.CanFightWars);
    }

    public override void InitData()
    {
        base.InitData();
        AddData(Tags.Territory, new TerritoryData());
        AddData(Tags.Emblem, new EmblemData());
        AddData(Tags.Settlements, new SettlementData());
        AddData(Tags.Population, new PopulationData());

		AddData(Tags.Politics, new PoliticalData());

		//Has war data cause it can go to war
		//but doesn't by defauly have the AtWar tag
		//Cause it isn't at war!
		AddData(Tags.HasMilitary, new MilitaryData());

		AddData(Tags.CanFightWars, new WarData());
    }
}
