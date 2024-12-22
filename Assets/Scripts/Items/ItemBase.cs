using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : IDisplay
{
	public int itemIndex = 0;
	private ItemDatabase.ItemData cachedItemData = null;
	private string statContributorDescriptions;
	private List<StatContributor> statContributors = new List<StatContributor>();

	public float GetPrice()
	{
		float price = cachedItemData.basePrice;

		//Modify price by current global economic conditions
		price *= 1000.0f; //Currently static

		//Should modify based on faction economic state here

		return price;
	}

	public bool LinkedToItem()
	{
		return cachedItemData != null;
	}

	public int GetStatContributorsCount()
	{
		return statContributors.Count;
	}

	public void ApplyStatContributors(PlayerStats target)
	{
		if (cachedItemData == null)
		{
			return;
		}

		foreach (KeyValuePair<string, string> entry in cachedItemData.nonPredefinedKeyToValue)
		{
			if (target.statToValue.ContainsKey(entry.Key))
			{
				StatContributor contributor = new StatContributor(float.Parse(entry.Value), entry.Key);

				target.statToValue[entry.Key].Add(contributor);
				statContributors.Add(contributor);
			}
		}
	}

	public void RemoveStatContributors(PlayerStats target)
	{
		foreach (StatContributor entry in statContributors)
		{
			if (target.statToValue.ContainsKey(entry.statIdentifier))
			{
				target.statToValue[entry.statIdentifier].Remove(entry);
			}
		}

		statContributors.Clear();
	}

	public void ReCacheItemData()
	{
		if (!ItemDatabase.itemIDToItemData.ContainsKey(itemIndex))
		{
			cachedItemData = null;
			return;
		}
		cachedItemData = ItemDatabase.itemIDToItemData[itemIndex];

		//Generate item additional extra description
		statContributorDescriptions = "\n\n";

		foreach (KeyValuePair<string, string> entry in cachedItemData.nonPredefinedKeyToValue)
		{
			if (float.TryParse(entry.Value, out float modifier))
			{
				string modifierSign = modifier >= 0.0f ? "+" : "";

				statContributorDescriptions = $"{statContributorDescriptions}\n{modifierSign}{modifier} {ItemDatabase.GetKeyAsTitle(entry.Key)}";
			}
		}
	}

	public ItemBase(int itemDataIndex)
	{
		itemIndex = itemDataIndex;
		ReCacheItemData();
	}

	//DISPLAY METHODS
	public string GetTitle()
	{
		return cachedItemData.name;
	}

	public string GetDescription()
	{
		return $"<color=#949494>[{cachedItemData.itemClass.ToString()}]</color>\n\n" + 
		cachedItemData.description;
	}

	public Sprite GetIcon()
	{
		if (cachedItemData == null)
		{
			return null;
		}

		return cachedItemData.icon;
	}

	public string GetExtraInformation()
	{
		return cachedItemData.extraDescription + statContributorDescriptions;
	}
	//
}
