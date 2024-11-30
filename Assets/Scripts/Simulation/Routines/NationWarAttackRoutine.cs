using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.attackRoutineStandardPrio)]
public class NationWarAttackRoutine : RoutineBase
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
			//Get data
			nation.GetData(Faction.Tags.CanFightWars, out WarData warData);
			int warCount = warData.atWarWith.Count;

			if (warCount <= 0)
			{
				continue;
			}

			nation.GetData(Faction.relationshipDataKey, out RelationshipData relData);
			nation.GetData(Faction.battleDataKey, out BattleData batData);
			nation.GetData(Faction.Tags.HasMilitary, out MilitaryData milData);

			//Need to iterate through current wars and create battles, the nation troops control routines will act on these battles afterwards but
			//we still send an initial number of fleets. Because the troop control routine has a transfer limit this means an this battle won't immediately be abandoned

			//Choose points to attack
			//Current max amount of attacks going on
			int maxAllowedAttacks = Mathf.RoundToInt(milData.currentFleetCount / 10); //Just use a static 10 modifier

			int attackBudget = maxAllowedAttacks - batData.ongoingBattles.Count;
			int fleetBudgerPerAttack = Mathf.RoundToInt(milData.currentFleetCount / 20.0f);

			if (fleetBudgerPerAttack <= 0)
			{
				//No fleet budget!
				continue;
			}

			//Iterate through all wars (or until we run out of budge)
			for (int i = 0; i < warCount && attackBudget > 0; i++)
			{
				int warOpponentID = warData.atWarWith[i];
				int attacksForThisWar = Mathf.CeilToInt(attackBudget / (float)warCount);

				//Get actual enemy
				Faction enemy = SimulationManagement.GetFactionByID(warOpponentID);

				//Get random position in enemy territory
				if (enemy.GetData(Faction.Tags.Territory, out TerritoryData terData))
				{
					if (terData.borders.Count == 0)
					{
						//Enenmy has no territory!
						//This should mean the war will be ended this tick
						continue;
					}
					else
					{
						for (int a = 0; a < attacksForThisWar; a++)
						{
							//Create new attack
							//Currently just pick a position from the enemy at random
							RealSpacePostion newAttackPos = terData.borders.ElementAt(SimulationManagement.random.Next(0, terData.borders.Count));

							//Transfer fleets to new attack if they are free
							//Function should automatically check if we already have ships there and adjust the budget accordingly
							int amountTransferred = milData.TransferFreeUnits(fleetBudgerPerAttack, newAttackPos, batData);

							if (amountTransferred > 0)
							{
								globalBattleData.StartBattle(newAttackPos, nation.id, enemy.id);
								//Lower remaining attack budget
								attackBudget--;
							}
						}
					}
				}
			}
		}
	}
}
