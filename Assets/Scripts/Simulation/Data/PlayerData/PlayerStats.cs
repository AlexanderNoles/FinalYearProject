using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : DataBase
{
	public static readonly Dictionary<string, float> statIdentifierToDefault = new Dictionary<string, float>() 
	{
		{Stats.health.ToString(), 100},
		{Stats.healthregen.ToString(), 1},
		{Stats.jumprange.ToString(), 25}
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
}

//Defined stat keys
//Used to allow autofill in code editors
public enum Stats
{
	health,
	healthregen,
	jumprange
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
