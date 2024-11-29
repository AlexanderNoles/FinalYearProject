using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.defendRoutineStandardPrio)]
public class NationDefenseRoutine : RoutineBase
{
	public override void Run()
	{
		//Get gameworld and universal battle data
		GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
		gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

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

			int defenceBudget = Mathf.CeilToInt(militaryData.currentFleetCount / 15.0f);

			for (int i = 0; i < battleData.pendingDefences.Count;)
			{
				KeyValuePair<RealSpacePostion, BattleData.PendingDefence> currentDef = battleData.pendingDefences.ElementAt(i);

				int transferCount = militaryData.TransferFreeFleets(defenceBudget, currentDef.Key, battleData);

				if (transferCount <= 0)
				{
					//Need to notify the battle that we are nont participating
					//Otherwise the battle will hang

					//In the future (current date: 28/11/2024) we should make it so the battle is based more on a progress bar
					//So we can wait to defend for a few ticks (but arriving late would incurr a penalty)
					//
					//One of the major things to keep in mind is we need to have some AttemptToJoin battle function
					//Incase the battle is already over
					globalBattleData.battles[currentDef.Key].RemoveInvolvedFaction(nation.id);
				}
				else
				{
					if (!battleData.ongoingBattles.ContainsKey(currentDef.Key))
					{
						//Add battle reference
						battleData.ongoingBattles.Add(currentDef.Key, new BattleData.BattleReference());

						if (!nation.HasTag(Faction.Tags.AtWar))
						{
							nation.AddTag(Faction.Tags.AtWar);
						}
					}
				}

				battleData.pendingDefences.Remove(currentDef.Key);
			}

			//Retreat code
			//This should be included here as it's a part of defence
		}
	}
}
