using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(-110, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class PoliticalChangeRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all political factions
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Politics);
		Dictionary<int, PoliticalData> idToPoliticalData = SimulationManagement.GetDataForFactionsList<PoliticalData>(factions, Faction.Tags.Politics.ToString());

		foreach (Faction faction in factions)
		{
			PoliticalData polData = idToPoliticalData[faction.id];
			polData.politicalInstability = 0.001f;

			if (faction.GetData(Faction.Tags.CanFightWars, out WarData milData))
			{
				polData.politicalInstability *= Mathf.Max(1, milData.warExhaustion);
			}

			float changeMultiplier = polData.politicalInstability;

			if (SimulationManagement.random.Next(-10000, 10001) / 10000.0f < changeMultiplier)
			{
				//Low chance for larger change
				changeMultiplier *= 2.0f;
			}


			//Apply randomized change
			polData.authorityAxis += changeMultiplier * (SimulationManagement.random.Next(-100, 101) / 100.0f);
			polData.authorityAxis = Mathf.Clamp(polData.authorityAxis, -1.0f, 1.0f);

			polData.economicAxis += changeMultiplier * (SimulationManagement.random.Next(-100, 101) / 100.0f);
			polData.economicAxis = Mathf.Clamp(polData.economicAxis, -1.0f, 1.0f);
		}
	}
}
