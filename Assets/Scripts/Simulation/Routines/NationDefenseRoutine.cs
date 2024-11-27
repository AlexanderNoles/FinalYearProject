using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.defendRoutineStandardPrio)]
public class NationDefenseRoutine : RoutineBase
{
	public override void Run()
	{
		//Respond to attack requests
		//any fleet that is within our borders (and isn't currently defending) can be used to respond to a threat
		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

		foreach (Faction nation in nations)
		{
			//Get nation mil data and battle data
			nation.GetData(Faction.battleDataKey, out BattleData battleData);
			nation.GetData(Faction.Tags.HasMilitary, out MilitaryData militaryData);

			//Iterate through pending defences
			//Send free fleets there

			//Finally add a battle reference

			//The system is structured this way because in the future there will be a chance for nations to "refuse" defence requests
			//this can be for a varierty of reasons, (e.g., We are modeling an information state and they don't "know" about the attack yet)

			int defLength = battleData.pendingDefences.Count;
			int defenceBudget = Mathf.RoundToInt(militaryData.currentFleetCount / 25.0f);

			for (int i = 0; i < defLength;)
			{
				//Current defence
				KeyValuePair<RealSpacePostion, BattleData.PendingDefence> currentDef = battleData.pendingDefences.ElementAt(i);

				if (!battleData.ongoingBattles.ContainsKey(currentDef.Key))
				{
					//Not already engaged here
					//Add battle reference
					battleData.ongoingBattles.Add(currentDef.Key, new BattleData.BattleReference());
				}

				//Transfer free fleets here
				//Function should automatically check if we already have ships there and adjust the budget accordingly
				militaryData.TransferFreeFleets(defenceBudget, currentDef.Key, battleData);

				//Currently we always remove cause there is no chance not to respond to the defence request
				battleData.pendingDefences.Remove(currentDef.Key);
			}
		}
	}
}
