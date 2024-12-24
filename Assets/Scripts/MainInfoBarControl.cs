using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainInfoBarControl : PostTickUpdate
{
	private static MainInfoBarControl instance;

	public TextMeshProUGUI populationLabel;
	public TextMeshProUGUI healthLabel;
	public TextMeshProUGUI currencyLabel;
	public TextMeshProUGUI fuelLabel;

	private void Awake()
	{
		instance = this;
		//Inital draw
		ForceRedraw();
	}

	public static void ForceRedraw()
	{
		if (instance == null)
		{
			return;
		}

		instance.PostTick();
	}

	protected override void PostTick()
	{
		//Get player data
		List<Faction> player = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		foreach (Faction faction in player)
		{
			//We could cache these but they have a complexity of O(1) so it's probably fine to do it like this
			//Plus in the future GetData might end the typical simulation tick instead of the simulation clamp, so we should be using GetData.
			PlayerStats playerStats = PlayerManagement.GetStats();
			faction.GetData(Faction.Tags.Population, out PopulationData populationData);
			faction.GetData(PlayerFaction.inventoryDataKey, out PlayerInventory playerInventory);

			populationLabel.text = Mathf.FloorToInt(populationData.currentPopulationCount).ToString();
			healthLabel.text = Mathf.FloorToInt(playerStats.GetStat(Stats.maxHealth.ToString())).ToString();
			currencyLabel.text = Mathf.FloorToInt(playerInventory.mainCurrency).ToString();

			if (!PlayerCapitalShip.IsJumping())
			{
				//During jump don't update fuel label automatically
				UpdateFuelLabel(playerInventory.fuel);
			}
		}
	}

	public static void UpdateFuelLabel(float value)
	{
		instance.fuelLabel.text = Mathf.FloorToInt(value).ToString();
	}
}
