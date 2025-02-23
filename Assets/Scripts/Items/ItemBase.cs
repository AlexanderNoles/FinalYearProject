using System.Collections;
using System.Collections.Generic;
using EntityAndDataDescriptor;
using UnityEngine;

public class ItemBase : IDisplay
{
	private ItemDatabase.ItemData cachedItemData = null;
	private List<StatContributor> statContributors = new List<StatContributor>();

	public float GetPrice(EntityLink parentFaction)
	{
		float price = cachedItemData.basePrice;

		//Modify price by current global economic conditions
		price *= 1000.0f; //Currently static

		//Modify based on entity economic state
		if (parentFaction != null && parentFaction.Get().GetData(DataTags.Economic, out EconomyData data))
		{
			float estimatedEconomyState = data.EstimatedEconomyState();

			price *= estimatedEconomyState;
		}

		return Mathf.RoundToInt(price);
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

		foreach (KeyValuePair<string, (StatContributor.Type, float)> entry in cachedItemData.keyToStat)
		{
			//Target has this stat
			if (target.statToExtraContributors.ContainsKey(entry.Key))
			{
				StatContributor contributor = new StatContributor(entry.Value.Item2, entry.Key, entry.Value.Item1);

				target.AddContributorToStat(entry.Key, contributor);
				statContributors.Add(contributor);
			}
		}
	}

	public void RemoveStatContributors(PlayerStats target)
	{
		foreach (StatContributor entry in statContributors)
		{
			if (target.statToExtraContributors.ContainsKey(entry.statIdentifier))
			{
				target.statToExtraContributors[entry.statIdentifier].Remove(entry);
			}
		}

		statContributors.Clear();
	}

	public ItemBase Setup(ItemDatabase.ItemData targetData)
	{
		cachedItemData = targetData;
		return this;
	}

	//DISPLAY METHODS
	public string GetTitle()
	{
		return cachedItemData.GetTitle();
	}

	public string GetDescription()
	{
		return cachedItemData.GetDescription();
	}

	public Sprite GetIcon()
	{
		return cachedItemData.GetIcon();
	}

	public string GetExtraInformation()
	{
		return cachedItemData.GetExtraInformation();
	}
	//
}
