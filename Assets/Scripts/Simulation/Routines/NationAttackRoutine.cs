using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.attackRoutineStandardPrio, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Debug)]
public class NationAttackRoutine : DebugRoutine
{
	public override void Run()
	{
		//Get positions to try and attack
		//Open battle requests in enemy factions data
		//bish bash bosh

		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

		//Get global battle data
		SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0].GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

		foreach (Faction nation in nations)
		{
			if (!nation.HasTag(Faction.Tags.AtWar))
			{
				//Not at war
				continue;
			}

			//Get data
			nation.GetData(Faction.relationshipDataKey, out RelationshipData relData);
			nation.GetData(Faction.battleDataKey, out BattleData batData);
			nation.GetData(Faction.Tags.HasMilitary, out MilitaryData milData);

			//Choose points to attack
			//Current max amount of attacks going on
			int maxAllowedAttacks = Mathf.RoundToInt(milData.currentFleetCount / 10); //Just use a static 10 modifier

			//Get all enemy factions that we want to attack
			List<int> warOpponentFactionIDs = new List<int>();
			foreach (KeyValuePair<int, RelationshipData.Relationship> entry in relData.idToRelationship)
			{
				if (entry.Value.conflict > 0)
				{
					//In conflict with this faction
					//Add it's id to warOpponents

					warOpponentFactionIDs.Add(entry.Key);
				}
			}

			int attackBudget = maxAllowedAttacks - batData.ongoingBattles.Count;
			int fleetBudgerPerAttack = Mathf.CeilToInt(milData.currentFleetCount / 20.0f);

			for (int i = 0; i < warOpponentFactionIDs.Count && attackBudget > 0; i++)
			{
				int attacksForThisEnemy = Mathf.CeilToInt(attackBudget / (float)warOpponentFactionIDs.Count);
				Faction enemy = SimulationManagement.GetFactionByID(warOpponentFactionIDs[i]);

				if (enemy.GetData(Faction.Tags.Settlements, out SettlementData settleData))
				{
					if (settleData.settlements.Count == 0)
					{
						//Enemy has no territory!
						//Currently just continue cause we wanna test this
						//system works without making every other system in the whole game
						continue;
					}
					else
					{
						for (int a = 0; a < attacksForThisEnemy; a++)
						{
							//Create new attack
							//Currently just pick a position from the enemy at random
							RealSpacePostion newAttackPos = settleData.settlements.ElementAt(SimulationManagement.random.Next(0, settleData.settlements.Count)).Key;

							//Add new attack refrence
							bool newBattleStarted = globalBattleData.StartBattle(newAttackPos, nation.id, enemy.id);

							//MonitorBreak.Bebug.Console.Log(newBattleStarted);

							int remainingFleetBudget = fleetBudgerPerAttack;
							if (!newBattleStarted)
							{
								//If we already engaged in battle on this cell
								remainingFleetBudget = Mathf.RoundToInt(remainingFleetBudget * 0.5f);
							}

							//Transfer fleets to new attack if they are free
							//Function should automatically check if we already have ships there and adjust the budget accordingly
							milData.TransferFreeFleets(remainingFleetBudget, newAttackPos, batData);

							//Lower remaining attack budget
							attackBudget--;
						}
					}
				}
				else
				{
					break;
				}
			}
		}
	}
}
