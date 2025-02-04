using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonitorBreak.Bebug;
using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(SimulationManagement.attackRoutineStandardPrio)]
public class WarAttackRoutine : RoutineBase
{
	public override void Run()
	{
		//Get positions to try and attack,
		//If there is already a battle there try to join it

		//Get all war data modules
		List<DataModule> warDatas = SimulationManagement.GetDataViaTag(DataTags.War);
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);

		foreach (WarData warData in warDatas.Cast<WarData>())
		{
			int warCount = warData.atWarWith.Count;

            bool hasMilitary = warData.TryGetLinkedData(DataTags.Military, out MilitaryData milData);
            bool hasBattleData = warData.TryGetLinkedData(DataTags.Battle, out BattleData batData);

            if (warCount <= 0 || !hasBattleData || !hasMilitary)
			{
                //If not at war or we have no battle data or military
				continue;
			}

            //Need to iterate through current wars and create battles, the nation troops control routines will act on these battles afterwards but
            //we still send an initial number of fleets. Because the troop control routine has a transfer limit this means an this battle won't immediately be abandoned

            //Choose points to attack
            //Current max amount of attacks going on
            int maxAllowedAttacks = Mathf.RoundToInt(milData.currentFleetCount / 10); //Just use a static 10 modifier

            int attackBudget = maxAllowedAttacks - batData.positionToOngoingBattles.Count;
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
                SimulationEntity enemy = SimulationManagement.GetEntityByID(warOpponentID);

                //Get random position in enemy territory
                if (enemy.GetData(DataTags.Territory, out TerritoryData terData))
                {
                    if (terData.borders.Count == 0)
                    {
                        //Enenmy has no territory!
                        //This should mean the war will be ended this tick
                        continue;
                    }
                    else
                    {
                        foreach (RealSpacePosition attackCell in terData.borders)
                        {
                            //Transfer fleets to new attack if they are free
                            //Function should automatically check if we already have ships there and adjust the budget accordingly
                            int amountTransferred = milData.TransferFreeUnits(fleetBudgerPerAttack, attackCell, batData);

                            if (amountTransferred > 0)
                            {
                                RealSpacePosition actualPos = null;

                                //If this is a brand new battle
                                if (!globalBattleData.cellCenterToBattles.ContainsKey(attackCell))
                                {
                                    if (enemy.GetData(DataTags.Settlements, out SettlementsData setData) && setData.settlements.ContainsKey(attackCell))
                                    {
                                        //If target has a settlement in this cell target that settlement specifically
                                        actualPos = setData.settlements[attackCell].actualSettlementPos;
                                    }
                                    else
                                    {
                                        actualPos = WorldManagement.RandomPositionInCell(attackCell, SimulationManagement.random);
                                    }
                                }
								else
								{
									actualPos = globalBattleData.cellCenterToBattles[attackCell][0].postion;
								}

                                globalBattleData.StartOrJoinBattle(attackCell, actualPos, milData.parent.Get().id, enemy.id, true);
                                //Lower remaining attack budget
                                attackBudget--;
                            }

                            attacksForThisWar--;

                            if (attacksForThisWar <= 0 || attackBudget <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
	}
}
