using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopControlUI : MonoBehaviour
{
	private Shop shopData;
	public MultiObjectPool shopSlotsPool;

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
			Draw();
			
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

	public void Draw()
	{
		//Get positions
		List<(float, Vector2)> positions = UIHelper.CalculateSpreadPositions(shopData.capacity, 110);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
