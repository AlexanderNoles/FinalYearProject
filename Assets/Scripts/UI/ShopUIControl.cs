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
	public DraggableWindow draggableWindowControl;
	public GameObject mainUI;
	public GameObject slotBaseObject;
	public RectTransform slotArea;
	public List<ShopSlotUI> shopSlots;
	public GameObject cantBuyBlocker;

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

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (target != null)
		{
			if (Vector3.Distance(targetsTransform.position, CameraManagement.GetMainCameraPosition()) > Interaction.Ranges.standard)
			{
				CloseShopUI();
			}
			else
			{
				cantBuyBlocker.SetActive(target.GetPlayerReputation() <= -0.1f);
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
			}

			for (int i = 0; i < shopSlots.Count; i++)
			{
				DrawSlot(i);
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
