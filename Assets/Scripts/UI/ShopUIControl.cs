using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class ShopUIControl : MonoBehaviour
{
	private static ShopUIControl instance;
	private SimObject target;
	private Transform targetsTransform;
	private Shop targetShop;

	public TextMeshProUGUI titleLabel;

	public DraggableWindow draggableWindowControl;
	public GameObject mainUI;
	public GameObject cantBuyBlocker;

	[Header("Item Shop")]
	public GameObject itemShopParent;
	public GameObject slotBaseObject;
	public RectTransform slotArea;
	public List<ShopSlotUI> shopSlots;

	[Header("Stat Shop")]
	public GameObject statShopParent;
	public GameObject statBaseObject;
	public RectTransform statArea;
	public List<StatSlotUI> statSlots;

	[ContextMenu("Generate Shop Slots")]
	public void GenerateShopSlots()
	{
		const int rowsCount = 5;
		shopSlots.Clear();

		//Clear old slots away
		for (int i = 0; i < slotArea.childCount;)
		{
			DestroyImmediate(slotArea.GetChild(0).gameObject);
		}

		float[] xValues = new float[3] { -125, 0, 125};

		for (int i = 0; i < rowsCount; i++)
		{
			float yValue = 75 + (i * 125);

			foreach (float xValue in xValues)
			{
				Vector3 anchoredPos = new Vector3(xValue, -yValue, 0.0f);

				ShopSlotUI newTarget = Instantiate(slotBaseObject, slotArea).GetComponent<ShopSlotUI>();
				(newTarget.transform as RectTransform).anchoredPosition3D = anchoredPos;

				shopSlots.Add(newTarget);
			}
		}
    }

	[ContextMenu("Generate Stat Slots")]
	public void GenerateStatSlots()
	{
		const int count = 4;
		statSlots.Clear();

		//Clear old slots
		for (int i = 0; i < statArea.childCount;)
		{
			DestroyImmediate(statArea.GetChild(0).gameObject);
		}

		for (int i = 0; i < count; i++)
		{
			Vector3 anchoredPos = new Vector3(0, -(80 + (i * 135)), 0.0f);
			StatSlotUI newTarget = Instantiate(statBaseObject, statArea).GetComponent<StatSlotUI>();
			(newTarget.transform as RectTransform).anchoredPosition3D = anchoredPos;

			statSlots.Add(newTarget);
		}
	}

	private void Awake()
	{
		instance = this;
		mainUI.SetActive(false);
	}

	private void Update()
	{
		if (target != null)
		{
			if (UIHelper.CloseInteractionBasedUI(targetsTransform))
			{
				CloseShopUI();
			}
			else
			{
				cantBuyBlocker.SetActive(target.GetPlayerReputation() <= BalanceManagement.properInteractionAllowedThreshold);
			}
		}
	}

	public static void ToggleShopUI(SimObject target, Transform targetsTransform)
	{
		if (target == instance.target)
		{
			//Close shop
			CloseShopUI();
		}
		else
		{
			//Swap to new shop
			instance.target = target;
			instance.Draw(false);
			instance.draggableWindowControl.InitialOffset();

			instance.targetsTransform = targetsTransform;
		}
	}

	public static void CloseShopUI()
	{
		instance.mainUI.SetActive(false);
		instance.target = null;
		instance.targetShop = null;
	}

	public static bool ShopUIOpen()
	{
		return instance.mainUI.activeSelf;
	}

	private void Draw(bool redraw)
	{
		if (target == null)
		{
			return;
		}

		if (target.HasShop())
		{
			targetShop = target.GetShop();

			if (!redraw)
			{
				targetShop.OnShopUIOpened();
				titleLabel.text = targetShop.GetShopTitle();
			}

			itemShopParent.SetActive(false);
			statShopParent.SetActive(false);
			//Based on the type of the shop
			//Set the ui active here
			//If this shop is an item shop, set the slots active
			if (targetShop.type == Shop.ShopType.ItemShop)
			{
				slotArea.gameObject.SetActive(true);
				for (int i = 0; i < shopSlots.Count; i++)
				{
					DrawSlot(i);
				}
			}
			else if (targetShop.type == Shop.ShopType.StatShop)
			{
				PlayerStats playerData = PlayerManagement.GetStats();
				statShopParent.SetActive(true);
				for (int i = 0; i < statSlots.Count; i++)
				{
					DrawStatSlot(i, playerData);
				}
			}

			mainUI.SetActive(true);
		}
	}

	private void DrawSlot(int index)
	{
		if (index >= targetShop.itemsInShop.Count)
		{
			shopSlots[index].Draw(null, () => { });
			shopSlots[index].HidePriceLabel();
			return;
		}

		shopSlots[index].Draw(targetShop.itemsInShop[index].item, () => { BuyItemAtSlot(index); });
		shopSlots[index].DrawPrice(targetShop.itemsInShop[index].calculatedPrice);
	}

	private void DrawStatSlot(int index, PlayerStats targetData)
	{
		if (index >= targetShop.soldStats.Count)
		{
			//Draw empty slot
			statSlots[index].Hide();
			return;
		}

		statSlots[index].Draw(targetShop.soldStats[index], targetData, () => { UpgradeStat(index, statSlots[index]); });
	}

	public void UpgradeStat(int index, StatSlotUI origin)
	{
		string statIdentifier = targetShop.soldStats[index].ToString();

		//Get player inventory and stats
		PlayerInventory playerInventory = PlayerManagement.GetInventory();
		PlayerStats playerStats = PlayerManagement.GetStats();

		//Attempt to subtract cost
		//First we need to calculate the cost
		if (playerStats.baseStatValues.ContainsKey(statIdentifier))
		{
			//Get current level
			int currentLevel = playerStats.baseStatValues[statIdentifier].level;

			//Current level is at or above max stat level
			//This should not happen as the StatSlotUI should disbale the button! So this function is being run 
			if (currentLevel >= PlayerStats.statIdentifierToBaseLevels[statIdentifier].Count)
			{
				Debug.LogWarning("Stat upgrade failed! Are you sure you should be calling this function?");
				return;
			}

			//Don't auto redraw because we are gonna redraw the whole ui anyway
			if (playerInventory.AttemptToBuy(origin.price, false))
			{
				//Above function should auto subtract price from currency
				//Add one to current level
				playerStats.baseStatValues[statIdentifier].level += 1;

				//Redraw slot
				statSlots[index].Redraw();

				//Redraw main ui
				MainInfoUIControl.ForceRedraw();
			}
		}
	}

	public void BuyItemAtSlot(int index)
	{
		ItemBase target = targetShop.itemsInShop[index].item;

		//Does this item base actually link to an item?
		Assert.IsTrue(target.LinkedToItem());

		PlayerInventory playerInventory = PlayerManagement.GetInventory();

		if (playerInventory.AttemptToBuy(target, targetShop.itemsInShop[index].calculatedPrice, targetShop.GetInventorySizeBuffer()))
		{
			bool closeResult = targetShop.OnItemBought();

			//Remove item from target shop
			targetShop.itemsInShop.RemoveAt(index);

			if (closeResult)
			{
				//Close shop
				CloseShopUI();
			}
			else
			{
				//Redraw shop interface
				Draw(true);
			}
		}
	}
}
