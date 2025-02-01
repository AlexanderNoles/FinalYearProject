using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatSlotUI : MonoBehaviour
{
	public Stats targetStat;
	public Image iconImage;
	private PlayerStats targetData;
	public TextMeshProUGUI label;
	public TextMeshProUGUI priceLabel;
	public Button upgradeButton;
	public SectionedBarIndicatorUI currentLevelBar;
	private string statAsString;
	public float price;

	public void Draw(Stats stat, PlayerStats targetData, UnityAction onClick)
	{
		statAsString = stat.ToString();
		if (!targetData.baseStatValues.ContainsKey(statAsString))
		{
			//Player doesn't have this stat, so we can't sell upgrades to them
			Hide();
			return;
		}

		gameObject.SetActive(true);
		this.targetStat = stat;
		iconImage.sprite = VisualDatabase.GetStatSprite(stat);
		this.targetData = targetData;

		upgradeButton.onClick.RemoveAllListeners();
		upgradeButton.onClick.AddListener(onClick);

		Redraw();
	}

	public void Redraw()
	{
		//Draw based on the state of this stat in the player's data
		//Plus the overall amount of levels this stat has

		//Get max stat level
		int maxStatLevel = PlayerStats.statIdentifierToBaseLevels[statAsString].Count;

		//Get current player level
		//Add one as level acts as an index
		int currentPlayerLevel = targetData.baseStatValues[statAsString].level + 1;

		//Price goes up exponentially
		price = 100 + (Mathf.Pow(currentPlayerLevel, 1.2f) * 100);
		const float roundTarget = 50.0f;
		price = Mathf.Round(price / roundTarget) * roundTarget;

		priceLabel.text = price.ToString();
		//

		currentLevelBar.Draw(currentPlayerLevel, maxStatLevel);
		label.text = ItemDatabase.GetKeyAsTitle(statAsString);

		bool atMaxLevel = currentPlayerLevel >= maxStatLevel;
		upgradeButton.interactable = !atMaxLevel;
		priceLabel.gameObject.SetActive(!atMaxLevel);
	}

	private void Update()
	{
		bool canAfford = PlayerManagement.GetInventory().CanAfford(price);
		priceLabel.color = canAfford ? Color.green : Color.red;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
