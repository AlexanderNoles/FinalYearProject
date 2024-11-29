using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.attackRoutineStandardPrio)]
public class NationAttackRoutine : RoutineBase
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
				if (entry.Value.inConflict)
				{
					//In conflict with this faction
					//Add it's id to warOpponents

					warOpponentFactionIDs.Add(entry.Key);
				}
			}

			int attackBudget = maxAllowedAttacks - batData.ongoingBattles.Count;
			int fleetBudgerPerAttack = Mathf.RoundToInt(milData.currentFleetCount / 20.0f);

			while(0 < warOpponentFactionIDs.Count && attackBudget > 0 && fleetBudgerPerAttack > 0)
			{
				int attacksForThisEnemy = Mathf.CeilToInt(attackBudget / (float)warOpponentFactionIDs.Count);
				int index = SimulationManagement.random.Next(0, warOpponentFactionIDs.Count);

				//Pick random enemy to attack
				int enemyID = warOpponentFactionIDs[index];
				warOpponentFactionIDs.RemoveAt(index);

				if (enemyID == nation.id)
				{
					throw new System.Exception("We are trying to attack ourself");
				}

				Faction enemy = SimulationManagement.GetFactionByID(enemyID);

				if (enemy.GetData(Faction.Tags.Territory, out TerritoryData terData))
				{
					if (terData.borders.Count == 0)
					{
						//Enemy has no territory!
						//War is over!

						//We are no longer in conflict with them
						//relData.idToRelationship[enemyID].inConflict = false;
						continue;
					}
					else
					{
						for (int a = 0; a < attacksForThisEnemy; a++)
						{
							//Create new attack
							//Currently just pick a position from the enemy at random
							RealSpacePostion newAttackPos = terData.borders.ElementAt(SimulationManagement.random.Next(0, terData.borders.Count));

							//Transfer fleets to new attack if they are free
							//Function should automatically check if we already have ships there and adjust the budget accordingly
							int amountTransferred = milData.TransferFreeFleets(fleetBudgerPerAttack, newAttackPos, batData); ;

							if (amountTransferred > 0)
							{
								globalBattleData.StartBattle(newAttackPos, nation.id, enemy.id);
								//Lower remaining attack budget
								attackBudget--;
							}
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
