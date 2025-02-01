using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : DataModule
{
	public int capacity = 8;

	public bool rarityLimited = false;
	public ItemDatabase.ItemRarity rarityLimitedToo;

	public void SetTargetRarity(ItemDatabase.ItemRarity newTarget)
	{
		rarityLimited = true;
		rarityLimitedToo = newTarget;
	}

	public class ShopEntry
	{
		public ItemBase item;
		public float calculatedPrice;

		public ShopEntry(ItemBase target, float price)
		{
			item = target;
			calculatedPrice = price;
		}
	}

	public List<ShopEntry> itemsInShop = new List<ShopEntry>();

	//The tick this shop should be updated
	//If this is behind the current tick we update the shop with new items
	private int nextShopUpdate = -1;

	public void OnShopUIOpened()
	{
		if (RestockShop())
		{
			//Update the shop with new items
			itemsInShop.Clear();

			for (int i = 0; i < capacity; i++)
			{
				ItemBase newItem;
				if (rarityLimited)
				{
					newItem = ItemHelper.Wrap(ItemHelper.GetRandomItemOfRarity(rarityLimitedToo));
				}
				else
				{
					newItem = ItemHelper.Wrap(ItemHelper.GetItemForGeneralPurpose());
				}

				itemsInShop.Add(new ShopEntry(newItem, newItem.GetPrice(parent)));
			}

			//Every 10 minutes if we don't include warp travel's effect on time
			nextShopUpdate = SimulationManagement.currentTickID + 200;
		}
	}

	public virtual bool RestockShop()
	{
		return nextShopUpdate <= SimulationManagement.currentTickID;
	}

	public virtual bool OnItemBought()
	{
		return false;
	}

	public virtual int GetInventorySizeBuffer()
	{
		return 0;
	}
}
