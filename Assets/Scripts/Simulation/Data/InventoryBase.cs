using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBase : DataModule
{
	public virtual int GetInventoryCapacity()
	{
		return 0;
	}

	public virtual int GetInventorySize()
	{
		return GetInventoryCapacity();
	}

	public virtual ItemBase GetInventoryItemAtPosition(int index)
	{
		return null;
	}

	public virtual void AddItemToInventory(ItemBase item)
	{

	}

	public virtual void RemoveItemFromInventory(ItemBase item)
	{

	}
}
