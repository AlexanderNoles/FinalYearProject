using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.evaluationRoutineStandardPrio, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class WarEvaluationRoutine : RoutineBase
{
	public override void Run()
	{
		//For every faction that can fight wars, evaluate whether those wars should be over
		//If they should be then end them
		List<Faction> atWarFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.CanFightWars);
		Dictionary<int, WarData> idToWarData = SimulationManagement.GetDataForFactionsList<WarData>(atWarFactions, Faction.Tags.CanFightWars.ToString());

		foreach (Faction faction in atWarFactions)
		{
			WarData warData = idToWarData[faction.id];

			//Iterate through each war
			for (int i = 0; i < warData.atWarWith.Count;)
			{
				int enemyID = warData.atWarWith[i];

				//Currently all wars are ones of complete eradication
				//Meaning a war can only end if the faction is gone

				//If enemy is gone
				//This is an evaluation routine so it runs after the MetaRoutine (which handles the final removal of factions)
				//This means we can just check if a faction was removed this tick
				if (!SimulationManagement.FactionWithIDExists(enemyID))
				{
					warData.atWarWith.Remove(enemyID);
				}
				else
				{
					i++;
				}
			}
		}
	}
}
