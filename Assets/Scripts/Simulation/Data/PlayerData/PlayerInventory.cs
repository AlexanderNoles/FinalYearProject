using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : InventoryBase
{
	//Currently static, could allow inventory size increase as a more controlled form of player power
	//(Allows for easier balancing!)
	protected const int inventorySize = 6;
	private List<ItemBase> itemBases = new List<ItemBase>();
	private PlayerStats target = null;

	public void SetStatsTarget(PlayerStats newTarget)
	{
		target = newTarget;
	}

	public override int GetInventorySize()
	{
		return itemBases.Count;
	}

	public override int GetInventoryCapacity()
	{
		return inventorySize;
	}

	public override ItemBase GetInventoryItemAtPosition(int index)
	{
		if (index >= itemBases.Count)
		{
			return base.GetInventoryItemAtPosition(index);
		}

		return itemBases[index];
	}

	public override void AddItemToInventory(ItemBase item)
	{
		//Add the item
		//Get item base to calculate and then store it's contributors to stats
		//If it's contributors count is above 0 already then this item is already contributing somehow and should not be added
		if (item.GetStatContributorsCount() > 0)
		{
			//Item is already contributing to player
			return;
		}

		//Apply the contributions of these stats to the player
		if (target != null)
		{
			item.ApplyStatContributors(target);
		}

		//Add item to inventory
		itemBases.Add(item);
	}

	public override void RemoveItemFromInventory(ItemBase item)
	{
		if (itemBases.Remove(item))
		{
			//If item was succesfully removed
			if (item.GetStatContributorsCount() > 0 && target != null)
			{
				//Remove items contributions to stats
				item.RemoveStatContributors(target);
			}
		}
	}
}
