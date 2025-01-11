using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSlotUI : SlotUI
{
	public TextMeshProUGUI priceLabel;

	public void DrawPrice(float price)
	{
		if (PlayerManagement.GetInventory().CanAfford(price)) 
		{
			priceLabel.color = Color.white;
		}
		else
		{
			priceLabel.color = Color.red;
		}

		priceLabel.text = price.ToString();
	}

	public void HidePriceLabel()
	{
		priceLabel.text = "";
	}
}
