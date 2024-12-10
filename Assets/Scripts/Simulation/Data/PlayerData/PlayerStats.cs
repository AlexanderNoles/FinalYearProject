using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : DataBase
{
	public static readonly Dictionary<string, float> statIdentifierToDefault = new Dictionary<string, float>() 
	{
		{Stats.health.ToString(), 100}
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
			statToValue.Add(identifier, 0);
		}

		statToValue[identifier] = GetDefaultStatValue(identifier);
	}

	//Stats dictionary
	public Dictionary<string, float> statToValue = new Dictionary<string, float>();

	public void SetStat(string identifier, float newValue)
	{
		statToValue[identifier] = newValue;
	}

	public float GetStat(string identifer)
	{
		return statToValue[identifer];
	}
}

//Defined stat keys
//Used to allow autofill in code editors
public enum Stats
{
	health
}
