using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(-500, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class NationDeadRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all nations
		//If they have no territory they are dead

		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

		for (int i = 0; i < nations.Count; i++)
		{
			Faction nation = nations[i];
			if (nation.GetData(Faction.Tags.Territory, out TerritoryData territoryData))
			{
				if (territoryData.territoryCenters.Count == 0)
				{
					nation.GetData(Faction.Tags.Faction, out FactionData factionData);
					//Set death flag
					factionData.deathFlag = true;
				}
			}
		}
	}
}
