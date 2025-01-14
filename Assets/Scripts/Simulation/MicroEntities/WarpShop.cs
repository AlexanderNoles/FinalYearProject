using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpShop : Shop
{
	public override bool RestockShop()
	{
		//Warp shop only restocks when empty
		//This is done to stop people from simply 
		//flying to restock the shop if they don't get the exalted they want
		return itemsInShop.Count == 0;
	}

	public override bool OnItemBought()
	{
		//Remove one instance of the item that allows player to buy from this shop
		PlayerManagement.GetInventory().RemoveItemFromInventoryOfType(typeof(WarpShopItemBase));

		return !PlayerManagement.GetInventory().HasItemOfType(typeof(WarpShopItemBase));
	}

	public override int GetInventorySizeBuffer()
	{
		//So item can be bought from inventory and then above WarpShopItemRemoved
		//even if they are at inventory capacity
		return 1;
	}
}
