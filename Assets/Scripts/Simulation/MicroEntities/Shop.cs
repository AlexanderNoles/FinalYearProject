using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : DataBase
{
	public int capacity = 8;

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
		if (nextShopUpdate <= SimulationManagement.currentTickID)
		{
			//Update the shop with new items
			itemsInShop.Clear();

			for (int i = 0; i < capacity; i++)
			{
				ItemBase newItem = new ItemBase(ItemDatabase.GetRandomItemIndex());
				itemsInShop.Add(new ShopEntry(newItem, newItem.GetPrice(parent)));
			}

			//Every 10 minutes if we don't include warp travel's effect on time
			nextShopUpdate = SimulationManagement.currentTickID + 200;
		}
	}
}
