using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopControlUI : FloatingWindow
{
	[Header("Effects")]
	public FadeOnEnable flash;
	public FadeOnEnable buyFlash;
	private Shop shopData;
	[Header("References")]
	public MultiObjectPool shopSlotsPool;
	public TextMeshProUGUI priceLabel;
	public StandardButton buyButton;
	public InformationDisplayControl selectedItemInformationDisplay;
	private Dictionary<RectTransform, SlotUI> cachedTransformToSlotUI = new Dictionary<RectTransform, SlotUI>();
	private int targetItemIndex;

	private void OnEnable()
	{
		targetItemIndex = -1;

		//Force inactive
		buyFlash.gameObject.SetActive(false);
	}

	public bool IsDisplayedData(Shop shop)
	{
		if (shopData == null || shop == null)
		{
			return false;
		}

		return shopData.Equals(shop);
	}

	public void ToggleOrGrab(Shop shop)
	{
		if (!IsDisplayedData(shop))
		{
			//Set as target
			shopData = shop;

			//Perform initial draw
			Draw(true);
			
			//And set active if not
			//We don't want to set a shop inactive if we just displayed our shop data in it
			gameObject.SetActive(true);
		}
		else
		{
			//Simply toggle shop
			gameObject.SetActive(!gameObject.activeSelf);
		}
	}

	public void Draw(bool initalDraw)
	{
		//Get positions
		List<(float, Vector2)> positions = UIHelper.CalculateSpreadPositions(shopData.capacity, 90);
		const int slotIndexInPool = 0;

		if (initalDraw)
		{
			//Reflash
			flash.Restart();

			//Run shops on shop ui opened
			//This will do things such as restock the shop if enough time has passed
			shopData.OnShopUIOpened();

			shopSlotsPool.ForceSetup();
		}

		//Add a slot for every item in the store
		int itemIndex = 0;
		foreach ((float, Vector2) position in positions)
		{
			RectTransform newSlot = shopSlotsPool.UpdateNextObjectPosition(slotIndexInPool, Vector3.zero) as RectTransform;
			newSlot.anchoredPosition = position.Item2;;

			if (!cachedTransformToSlotUI.ContainsKey(newSlot))
			{
				cachedTransformToSlotUI.Add(newSlot, newSlot.GetComponent<SlotUI>());
			}

			if (itemIndex < shopData.itemsInShop.Count)
			{
				//Still have items to display
				Shop.ShopEntry target = shopData.itemsInShop[itemIndex];
				int indexClone = itemIndex;
				cachedTransformToSlotUI[newSlot].Draw(target.item, () => { DisplayItemForPurchase(indexClone, target); });
			}
			else
			{
				cachedTransformToSlotUI[newSlot].Draw(null, () => { });
			}

			itemIndex++;
		}

		shopSlotsPool.PruneObjectsNotUpdatedThisFrame(slotIndexInPool);
	}

	public void DisplayItemForPurchase(int index, Shop.ShopEntry targetItem)
	{
		if (targetItem.item != null && targetItem.item.LinkedToItem())
		{
			if (targetItemIndex == index)
			{
				//Already selected item
				BuyCurrentlySelected();
			}
			else
			{
				targetItemIndex = index;
				selectedItemInformationDisplay.Draw(targetItem.item);

				//Check we can buy item
				List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

				float price = targetItem.calculatedPrice;
				bool canBuy = false;

				if (players.Count > 0)
				{
					players[0].GetData(PlayerFaction.inventoryDataKey, out PlayerInventory playerInventory);

					canBuy = playerInventory.mainCurrency >= price;
				}

				buyButton.Enable(canBuy);
				priceLabel.text = $"PRICE: {price}";
			}
		}
	}

	public void BuyCurrentlySelected()
	{
		if (targetItemIndex == -1 || shopData.itemsInShop.Count <= targetItemIndex)
		{
			return;
		}

		ItemBase target = shopData.itemsInShop[targetItemIndex].item;

		if (target.LinkedToItem())
		{
			//Get player inventory
			//Attempt to buy
			List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

			if (players.Count > 0)
			{
				//Any player factions exist
				players[0].GetData(PlayerFaction.inventoryDataKey, out PlayerInventory playerInventory); 
				
				if (playerInventory.AttemptToBuy(target, shopData.itemsInShop[targetItemIndex].calculatedPrice))
				{
					//Remove item from shop
					shopData.itemsInShop.RemoveAt(targetItemIndex);

					//Hide ability to buy
					selectedItemInformationDisplay.DisplayBlocker();
					targetItemIndex = -1;

					//Redraw
					Draw(false);

					//Redraw main bar
					//because currency and other stats may have updated
					MainInfoBarControl.ForceRedraw();
				}
			}
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
