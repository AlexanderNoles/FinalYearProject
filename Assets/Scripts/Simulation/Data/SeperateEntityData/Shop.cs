using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop
{
	public int capacity = 8;
	public List<ItemBase> itemsInShop = new List<ItemBase>();

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
				itemsInShop.Add(new ItemBase(ItemDatabase.GetRandomItemIndex()));
			}

			//Every 10 minutes if we don't include warp travel's effect on time
			nextShopUpdate += 200;
		}
	}
}
