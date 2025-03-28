using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : InventoryBase
{
	//Currently static, could allow inventory size increase as a more controlled form of player power
	//(Allows for easier balancing!)
	public int numberOfWarpDrivesCollected = 3;
	private List<ItemBase> itemBases = new List<ItemBase>();
	private PlayerStats target = null;
	public float mainCurrency = 5000;
	public float fuel = 500;

	public void AdjustCurrency(float value)
	{
		mainCurrency += value;
		MainInfoUIControl.ForceCurrencyRedraw();
	}

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
		return 2 + Mathf.Max(0, 2 * (numberOfWarpDrivesCollected - 1));
	}

	public override ItemBase GetInventoryItemAtPosition(int index)
	{
		if (index >= itemBases.Count)
		{
			return base.GetInventoryItemAtPosition(index);
		}

		return itemBases[index];
	}

	public bool AttemptToBuy(ItemBase target, float price, int inventorySizeBuffer)
	{
		//Do we have space?
		if (itemBases.Count < GetInventoryCapacity() + inventorySizeBuffer)
		{
			//Can we afford item?
			if (CanAfford(price))
			{
				//Subtract price
				mainCurrency -= price;

				//Redraw info ui
				MainInfoUIControl.ForceCurrencyRedraw();

				//Add item to inventory
				AddItemToInventory(target);

				return true;
			}
		}

		return false;
    }

	public bool AttemptToBuy(float price, bool autoRedrawUI = true)
	{
		if (CanAfford(price))
		{
			//Auto subtract currency
			mainCurrency -= price;

			if (autoRedrawUI)
			{
				MainInfoUIControl.ForceCurrencyRedraw();
			}

			return true;
		}

		return false;
	}

	public bool CanAfford(float price)
	{
		return price <= mainCurrency;
	}

	public bool HasItemOfType(Type type)
	{
		foreach (ItemBase item in itemBases)
		{
			if (item.GetType().Equals(type))
			{
				return true;
			}
		}

		return false;
	}

	public void RemoveItemFromInventoryOfType(Type type)
	{
		ItemBase target = null;
		foreach (ItemBase item in itemBases)
		{
			if (item.GetType().Equals(type))
			{
				target = item;
				break;
			}
		}

		if (target != null)
		{
			RemoveItemFromInventory(target);
		}
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
			PlayerStatsPostItemAddVerify();
		}

		//Add item to inventory
		itemBases.Add(item);

		//Ask inventory ui to redraw, if the ui element is not active this function will do nothing
		InventoryUIManagement.DrawSlot(itemBases.Count - 1);
	}

	private void PlayerStatsPostItemAddVerify()
	{
		if (target == null)
		{
			return;
		}

		if (target.GetStat(Stats.maxHealth.ToString()) <= 0.0f)
		{
			PlayerManagement.KillPlayer();
		}
	}

	public override void RemoveItemFromInventory(ItemBase item)
	{
		int indexOf = itemBases.IndexOf(item);

		if (itemBases.Remove(item))
		{
			//If item was succesfully removed
			if (item.GetStatContributorsCount() > 0 && target != null)
			{
				//Remove items contributions to stats
				item.RemoveStatContributors(target);
			}

			//Ask inventory ui to redraw slot
			InventoryUIManagement.ForceRedraw();
		}
	}
}
