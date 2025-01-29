using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : DataBase
{
	public static readonly Dictionary<string, float> statIdentifierToDefault = new Dictionary<string, float>() 
	{
		{Stats.maxHealth.ToString(), 100},
		{Stats.healthRegen.ToString(), 100},
		{Stats.jumpRange.ToString(), 10},
		{Stats.attackPower.ToString(), 10},
		{Stats.moveSpeed.ToString(), 150}
	};

	public static float GetDefaultStatValue(string identifier)
	{
		return statIdentifierToDefault[identifier];
	}

	public void ResetStatsToDefault()
	{
		foreach (KeyValuePair<string, float> entry in statIdentifierToDefault)
		{
			ResetStatToDefault(entry.Key);
		}
	}

	public void ResetStatToDefault(string identifier)
	{
		if (!statToValue.ContainsKey(identifier))
		{
			statToValue.Add(identifier, new List<StatContributor>());
		}

		statToValue[identifier].Clear();
		//Only add the default contributor
		statToValue[identifier].Add(new StatContributor(GetDefaultStatValue(identifier), identifier));
	}

	//Stats dictionary
	public Dictionary<string, List<StatContributor>> statToValue = new Dictionary<string, List<StatContributor>>();

	public void AddContributorToStat(string identifier, StatContributor newValue)
	{
		if (!statToValue.ContainsKey(identifier))
		{
			//Do nothing if we don't have this stat
			return;
		}

		statToValue[identifier].Add(newValue);
	}

	public List<StatContributor> GetStatContributors(string identifier)
	{
		if (statToValue.ContainsKey(identifier))
		{
			return statToValue[identifier];
		}

		return null;
	}

	public float GetStat(string identifier)
	{
		float toReturn = 0.0f;
		if (statToValue.ContainsKey(identifier))
		{
			List<StatContributor> contributors = statToValue[identifier];

			foreach (StatContributor contributor in contributors)
			{
				toReturn += contributor.value;
			}
		}

		return toReturn;
	}

	[MonitorBreak.Bebug.ConsoleCMD("playerstats")]
	public static void OutputPlayerStats()
	{
		string compoundString = "";

        //Get player entity
        PlayerStats target = PlayerManagement.GetStats();

        foreach (KeyValuePair<string, List<StatContributor>> entry in target.statToValue)
        {
            compoundString += $"\n{entry.Key}: {target.GetStat(entry.Key)}";
        }

		MonitorBreak.Bebug.Console.Log(compoundString);
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
	moveSpeed
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
