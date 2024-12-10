using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : IDisplay
{
	public int itemIndex = 0;
	private ItemDatabase.ItemData cachedItemData = null;

	public void ReCacheItemData()
	{
		if (!ItemDatabase.itemIDToItemData.ContainsKey(itemIndex))
		{
			cachedItemData = null;
			return;
		}

		cachedItemData = ItemDatabase.itemIDToItemData[itemIndex];
	}

	public string GetTitle()
	{
		throw new System.NotImplementedException();
	}

	public string GetDescription()
	{
		throw new System.NotImplementedException();
	}

	public Sprite GetIcon()
	{
		throw new System.NotImplementedException();
	}

	public string GetExtraInformation()
	{
		throw new System.NotImplementedException();
	}

	public ItemBase(int itemDataIndex)
	{
		itemIndex = itemDataIndex;
		ReCacheItemData();
	}
}
