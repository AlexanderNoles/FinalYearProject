using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainInfoBarControl : PostTickUpdate
{
	public TextMeshProUGUI populationLabel;
	public TextMeshProUGUI healthLabel;

	protected override void PostTick()
	{
		//Get player data
		List<Faction> player = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		foreach (Faction faction in player)
		{
			faction.GetData(PlayerFaction.statDataKey, out PlayerStats playerStats);
			faction.GetData(Faction.Tags.Population, out PopulationData populationData);
			
			populationLabel.text = Mathf.FloorToInt(populationData.currentPopulationCount).ToString();
			healthLabel.text = Mathf.FloorToInt(playerStats.GetStat(Stats.health.ToString())).ToString();
        }
	}
}
