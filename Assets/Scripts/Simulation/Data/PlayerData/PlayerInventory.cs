using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : InventoryBase
{
	//Currently static, could allow inventory size increase as a more controlled form of player power
	//(Allows for easier balancing!)
	protected const int inventorySize = 6;
	private List<ItemBase> itemBases = new List<ItemBase>();

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
		itemBases.Add(item);
	}
}
