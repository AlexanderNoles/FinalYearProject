using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStats : DataModule
{
	public static readonly Dictionary<string, List<float>> statIdentifierToBaseLevels = new Dictionary<string, List<float>>() 
	{
		{Stats.maxHealth.ToString(), new List<float>() { 100.0f, 150.0f, 200.0f, 250.0f, 300.0f, 400.0f, 500.0f } },
		{Stats.healthRegen.ToString(), new List<float>() { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f }},
		{Stats.jumpRange.ToString(), new List<float>() { 10.0f }},
		{Stats.attackPower.ToString(), new List<float>() { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f }},
		{Stats.moveSpeed.ToString(), new List<float>() { 150.0f, 175.0f, 200.0f, 225.0f, 250.0f, 350.0f, 500.0f }},
		{Stats.populationCap.ToString(), new List<float>() { 0.0f, 50.0f, 150.0f, 200.0f, 400.0f, 450.0f, 900.0f }}
	};

	public static float GetDefaultStatValue(string identifier)
	{
		return statIdentifierToBaseLevels[identifier][0];
	}

	public void Init()
	{
		foreach (string key in statIdentifierToBaseLevels.Keys)
		{
			ResetStatToDefault(key);
		}
	}

	public void ResetStatToDefault(string identifier)
	{
		//Reset base stat
		if (!baseStatValues.ContainsKey(identifier))
		{
			baseStatValues.Add(identifier, new BaseStat(identifier));
		}

		baseStatValues[identifier].level = 0;
		//

		//Clear extra contributors
		if (!statToExtraContributors.ContainsKey(identifier))
		{
			statToExtraContributors.Add(identifier, new List<StatContributor>());
		}

		statToExtraContributors[identifier].Clear();
		//
	}

	//Stats dictionary
	public Dictionary<string, BaseStat> baseStatValues = new Dictionary<string, BaseStat>();
	public Dictionary<string, List<StatContributor>> statToExtraContributors = new Dictionary<string, List<StatContributor>>();

	public void AddContributorToStat(string identifier, StatContributor newValue)
	{
		if (!statToExtraContributors.ContainsKey(identifier))
		{
			//Do nothing if we don't have this stat
			return;
		}

		statToExtraContributors[identifier].Add(newValue);
	}

	public List<StatContributor> GetStatContributors(string identifier)
	{
		if (statToExtraContributors.ContainsKey(identifier))
		{
			return statToExtraContributors[identifier];
		}

		return null;
	}

	public float GetStat(string identifier)
	{
		float toReturn = 0.0f;
		if (statToExtraContributors.ContainsKey(identifier))
		{
			//Get base stat
			//If we have contributors then we should have a base stat
			Assert.IsTrue(baseStatValues.ContainsKey(identifier));

			toReturn = statIdentifierToBaseLevels[identifier][baseStatValues[identifier].level];

			//Add up extra stat contributors
			List<StatContributor> contributors = statToExtraContributors[identifier];
			foreach (StatContributor contributor in contributors)
			{
				toReturn += contributor.value;
			}
			//
		}

		return toReturn;
	}

	[MonitorBreak.Bebug.ConsoleCMD("playerstats")]
	public static void OutputPlayerStats()
	{
		string compoundString = "";

        //Get player entity
        PlayerStats target = PlayerManagement.GetStats();

        foreach (KeyValuePair<string, List<StatContributor>> entry in target.statToExtraContributors)
        {
            compoundString += $"\n{entry.Key}: {target.GetStat(entry.Key)}";
        }

		MonitorBreak.Bebug.Console.Log(compoundString);
	}

	[MonitorBreak.Bebug.ConsoleCMD("GODMODE")]
	public static void GodModeStats()
	{
		PlayerStats target = PlayerManagement.GetStats();

		target.AddContributorToStat(Stats.maxHealth.ToString(), new StatContributor(10000, Stats.maxHealth.ToString()));
		target.AddContributorToStat(Stats.healthRegen.ToString(), new StatContributor(10000, Stats.healthRegen.ToString()));
	}
}

//Defined stat keys
//Used to allow autofill in code editors
public enum Stats
{
	maxHealth,
	healthRegen,
	jumpRange,
	attackPower,
	moveSpeed,
	populationCap
}

public class StatContributor
{
	public string statIdentifier;
	public float value;

	public StatContributor(float newValue, string statIdentifier)
	{
		value = newValue;
		this.statIdentifier = statIdentifier;
	}
}

public class BaseStat
{
	public string statIdenttifier;
	public int level;

	public BaseStat(string identifier)
	{
		statIdenttifier = identifier;
		level = 0;
	}
}
