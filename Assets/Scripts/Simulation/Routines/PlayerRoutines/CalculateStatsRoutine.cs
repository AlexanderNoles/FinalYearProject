using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Absent, "PlayerStatsCalculation")]
public class CalculateStatsRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all player factions
		//Currently there should only ever be one but just in case
		List<Faction> playerFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		foreach (Faction faction in playerFactions)
		{
			//Get player stats data
			faction.GetData(PlayerFaction.statDataKey, out PlayerStats statData);

			//Reset stats to default
			statData.statToValue.Clear();
			foreach (KeyValuePair<string, float> statAndDefault in PlayerStats.statIdentifierToDefault)
			{
				statData.statToValue.Add(statAndDefault.Key, statAndDefault.Value);
			}
			//

			//Get inventory data
			faction.GetData(PlayerFaction.inventoryDataKey, out PlayerInventory inventory);

			//For every item in the player's inventory apply their effect
			int numberOfItems = inventory.GetInventorySize();

			for (int i = 0; i < numberOfItems; i++)
			{
				//Get item's id
				int itemID = inventory.GetInventoryItemAtPosition(i).itemIndex;

				//Get item data based on id
				ItemDatabase.ItemData item = ItemDatabase.itemIDToItemData[itemID];

				//Get all non predefined stat changes
				//This will also get any erronoues data that doesn't have a defined use
				//So a check is needed to see if the stat exists, this is good practice to make code robust anyway
				foreach (KeyValuePair<string, string> entry in item.nonPredefinedKeyToValue)
				{
					if (statData.statToValue.ContainsKey(entry.Key))
					{
						//Convert second entry string to modifer and apply
						statData.statToValue[entry.Key] += float.Parse(entry.Value);
					}
				}
			}
		}
	}
}