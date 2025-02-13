using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class Nation : Faction
{
	private readonly static List<string> primaryWords = new List<string>() 
	{
		"Western",
		"Eastern",
		"Northen",
		"Southern",
		"United",
		"Kyrda",
		"Gronka",
		"Seyto",
		"Landngo",
		"New",
		"Old",
		"Waldan",
		"Central",
		"Kyrtoku",
		"Miri",
		"Eria",
		"Roonnew",
		"Maco",
		"Bralmo",
		"Honsouth",
		"Honeast",
		"Honwest",
		"Honnorth",
		"Lagui",
		"Nesland",
		"True",
		"Lunarian",
		"Ruby",
		"Emerald",
		"Diamond",
		"First",
		"Sixth",
		"Tenth",
		"Sovereign"
	};

	private readonly static List<string> secondaryWords = new List<string>()
	{
		"Kingdom",
		"Dominion",
		"Empire",
		"Conglomerate",
		"Militia",
		"Union",
		"Democracy",
		"Republic"
	};



	public string name;

	public override void Simulate()
	{
		base.Simulate();

		//Generate name

		//First primary word
		name = primaryWords[SimulationManagement.random.Next(0, primaryWords.Count)];

		//Should this have a grander feel?
		bool grander = SimulationManagement.random.Next(0, 101) > 70;
		
		if (grander)
		{
			name = "The " + name;
		}

		//Add a second primary word?
		bool secondWord = SimulationManagement.random.Next(0, 101) > 50;

		if (secondWord)
		{
			name += " ";
			name += primaryWords[SimulationManagement.random.Next(0, primaryWords.Count)];
		}

		//Then have a chance to give a secondary word
		bool hasSecondary = SimulationManagement.random.Next(0, 101) > 50;
		if (hasSecondary)
		{
			name += " ";
			name += secondaryWords[SimulationManagement.random.Next(0, secondaryWords.Count)];
		}
	}

	public override void InitTags()
    {
        base.InitTags();
        AddTag(EntityTypeTags.Nation);
    }

    public override void InitData()
    {
        base.InitData();
        AddData(DataTags.Territory, new TerritoryData());
        AddData(DataTags.Emblem, new EmblemData());
        AddData(DataTags.Settlements, new SettlementsData());
        AddData(DataTags.Population, new PopulationData());
		AddData(DataTags.Political, new PoliticalData());
		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Strategy, new WarStrategyData());
		AddData(DataTags.Economic, new EconomyData());
		AddData(DataTags.ContactPolicy, new ContactPolicyData());
    }
}
