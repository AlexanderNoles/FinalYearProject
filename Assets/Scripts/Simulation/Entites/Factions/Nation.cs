using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using MonitorBreak.Bebug;

public class Nation : Faction
{
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
		AddData(DataTags.Name, new NationNameData());
        AddData(DataTags.Settlements, new SettlementsData());
        AddData(DataTags.Population, new PopulationData());
		AddData(DataTags.Political, new PoliticalData());
		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Strategy, new WarStrategyData());
		AddData(DataTags.Economic, new EconomyData());
		AddData(DataTags.ContactPolicy, new ContactPolicyData());
    }
}

public class NationNameData : NameData
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

	private class PoliticalTitle
	{
		public string actualWord;
		public Vector2 politicalAxesPosition;

		public PoliticalTitle(string text, Vector2 pos)
		{
			actualWord = text;
			politicalAxesPosition = pos;
		}
	}

	private readonly static List<PoliticalTitle> politicalTitles = new List<PoliticalTitle>()
	{
		//No clue if most of these are accurate but they seem close enough based on the 5 minutes of work I've done
		new PoliticalTitle("Kingdom", new Vector2(0.3f, 0.9f)),
		new PoliticalTitle("Dominion", new Vector2(0.1f, 1)),
		new PoliticalTitle("Empire", new Vector2(0.75f, 0.9f)),
		new PoliticalTitle("Conglomerate", new Vector2(1f, 0.25f)),
		new PoliticalTitle("Militia", new Vector2(-0.5f, 0.8f)),
		new PoliticalTitle("Union", new Vector2(0.75f, -1.0f)),
		new PoliticalTitle("Democracy", new Vector2(0.3f, -0.1f)),
		new PoliticalTitle("Republic", new Vector2(0.25f, 0.1f))
	};

	public override void Generate()
	{
		//Create the base name
		//This doesn't include the political titles as they will be applied dynamically based on politics data

		//Base name can be the combination of upto two primary words, succeeded by "The"
		baseName = primaryWords[SimulationManagement.random.Next(0, primaryWords.Count)];

		//Should this have a grander feel?
		bool grander = SimulationManagement.random.Next(0, 101) > 70;

		if (grander)
		{
			baseName = "The " + baseName;
		}

		//Add a second primary word?
		bool secondWord = SimulationManagement.random.Next(0, 101) > 50;

		if (secondWord)
		{
			baseName += " ";
			baseName += primaryWords[SimulationManagement.random.Next(0, primaryWords.Count)];
		}
	}


	const float maximumDistance = 0.4f;
	public override string GetName()
	{
		//Combine the base name with a title based on the current political state
		PoliticalTitle foundTitle = null;

		//Iterate through the political titles
		//Find any we are within a threshold distance of and find the closest among them
		//Pick that one as our title
		if (parent.Get().GetData(DataTags.Political, out PoliticalData polData))
		{
			float currentLowestDistance = float.MaxValue;

			foreach (PoliticalTitle title in politicalTitles)
			{
				float distance = polData.CalculatePoliticalDistance(title.politicalAxesPosition.x, title.politicalAxesPosition.y);
				if (distance <= maximumDistance && distance < currentLowestDistance)
				{
					currentLowestDistance = distance;
					foundTitle = title;
				}
			}
		}

		//Get final name
		string toReturn = baseName;

		if (foundTitle != null)
		{
			toReturn += " " + foundTitle.actualWord;
		}

		return toReturn;
	}

	[MonitorBreak.Bebug.ConsoleCMD("GRAPHTITLES", "Visually graph the political titles")]
	public static void GraphPoliticalTitles()
	{
		const int scale = 200;

		Graph graph = new Graph(new Vector2(0, 0), new Vector2(scale, scale), Color.white);

		foreach (PoliticalTitle title in politicalTitles)
		{
			float x = title.politicalAxesPosition.x;
			float y = title.politicalAxesPosition.y;

			int res = 300;
			for (int i = 0; i < res; i++)
			{
				float percentage = i / (float)(res);
				Vector3 offset = GenerationUtility.GetCirclePositionBasedOnPercentage(percentage, maximumDistance);

				graph.AddNewPoint(x + offset.x, y + offset.z);
			}
		}

		graph.AddBufferPoints(1.1f + maximumDistance, 1.1f + maximumDistance);
		graph.UpdateGraph();
	}
}
