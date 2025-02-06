using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(SimulationManagement.attackRoutineStandardPrio)]
public class StrategyRoutine : RoutineBase
{
	public static bool strategySwitchEnabled = false;

	public override void Run()
    {
		//Get global battle data
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);

        //Get all war datas
        List<DataModule> stratDatas = SimulationManagement.GetDataViaTag(DataTags.Strategy);
		Dictionary<int, TerritoryData> idToTerritory = SimulationManagement.GetEntityIDToData<TerritoryData>(DataTags.Territory);
		Dictionary<int, SettlementsData> idToSettlements = SimulationManagement.GetEntityIDToData<SettlementsData>(DataTags.Settlements);

        foreach (StrategyData data in stratDatas.Cast<StrategyData>())
        {
			if (strategySwitchEnabled)
			{
				if (SimulationManagement.random.Next(0, 100) < data.GetDefensivePropensity())
				{
					//Switch to defense stratergy
					data.globalStrategy = StrategyData.GlobalStrategy.Defensive;
				}
				else
				{
					data.globalStrategy = StrategyData.GlobalStrategy.Aggresive;
				}
			}

			//Retrieve data for decision making
			bool hasMilitaryData = data.TryGetLinkedData(DataTags.Military, out MilitaryData milData);
			bool hasBattleData = data.TryGetLinkedData(DataTags.Battle, out BattleData batData);

			List<int> targetEntityIDs = data.GetTargets();

			if (targetEntityIDs.Count == 0 || !hasBattleData || !hasMilitaryData)
			{
				//If no selected targets or no neccesary data skip this iteration
				continue;
			}

			//Setup attacks for this tick, this won't neccesarilly be all the battles started this tick
			//as other routines may try to set some up (e.g, desirability system).
			
			//The maximum allowed attacks to do, this means eithier joining an existing battle or creating one
			//Joining an existing battle could mean just transferring more troops to a battle we are already in
			int maxAllowedAttacks = Mathf.RoundToInt(milData.currentFleetCount / 10.0f);

			//How many attacks left
			int attackBudget = maxAllowedAttacks - batData.positionToOngoingBattles.Count;
			//How many fleets should we allocate per attack?
			//Higher modifier than maxAllowedAttacks to help prevent some of the entity's overcomitting
			//(This isn't a huge problem because it doesn't matter a huge amount if an entity is destroyed, only if
			//that destruction is interesting for the player)
			int fleetBudgetPerAttack = Mathf.RoundToInt(milData.currentFleetCount / 20.0f);

			if (fleetBudgetPerAttack <= 0)
			{
				//No budget!
				continue;
			}

			//Iterate through all targets until we run out of attack budger
			for (int i = 0; i < targetEntityIDs.Count && attackBudget > 0; i++)
			{
				int enemyID = targetEntityIDs[i];

				//Number of attacks against this target
				int attacksForThisTarget = Mathf.CeilToInt(attackBudget / (float)targetEntityIDs.Count);

				//Find attack position
				if (idToTerritory.ContainsKey(enemyID) && idToTerritory[enemyID].borders.Count > 0)
				{
					//For each position in their borders
					//Attack!
					//Iterate through borders in a linear fashion so we maintain the same targets
					//across ticks. Also avoids expensive ElementAt calls
					foreach (RealSpacePosition cellCenter in idToTerritory[enemyID].borders)
					{
						//In opposition to the above comment, skip this position if we pass an uncertainty check
						//by default this threshold is set to 0 so this won't happen
						if (SimulationManagement.random.Next(0, 101) / 100.0f < data.attackPositionUncertainty)
						{
							continue;
						}

						//Attempt to transfer free units to this new cell

						//First we need to find an actual position, not just the cell center
						RealSpacePosition actualPos = null;

						if (!globalBattleData.cellCenterToBattles.ContainsKey(cellCenter))
						{
							//This battle is new!
							if (idToSettlements.ContainsKey(enemyID) && idToSettlements[enemyID].settlements.ContainsKey(cellCenter))
							{
								//Lock battle to Settlement if one exists...
								actualPos = idToSettlements[enemyID].settlements[cellCenter].actualSettlementPos;
							}
							else
							{
								//...otherwise pick random position in cell
								actualPos = WorldManagement.RandomPositionInCell(cellCenter, SimulationManagement.random);
							}
						}
						else
						{
							//Otherwise just get the first battle happening in this cell
							actualPos = globalBattleData.cellCenterToBattles[cellCenter][0].postion;
						}

						//Make sure to send units to the actual pos and not the cell center (this is a bug that happened in the past)
						int amountTransferred = milData.TransferFreeUnits(fleetBudgetPerAttack, actualPos, batData);

						//If we actually sent any fleets...
						if (amountTransferred > 0)
						{
							if (data.parent.Get() is VoidSwarm)
							{
								Debug.DrawRay(-actualPos.AsTruncatedVector3(MapManagement.mapRelativeScaleModifier), Vector3.up * amountTransferred, Color.yellow, 3.0f / SimulationManagement.GetSimulationSpeed());
							}

							//Join battle via battle data
							globalBattleData.StartOrJoinBattle(cellCenter, actualPos, milData.parent.Get().id, enemyID, true);
							//Lower remaining attack budget for all targetse
							attackBudget--;
						}

						//Lower amount of attacks for this target specifically
						attacksForThisTarget--;

						//If we can't attack any more break out of
						//searching for attack pos loop
						if (attacksForThisTarget <= 0 || attackBudget <= 0)
						{
							break;
						}
					}
				}
				else
				{
					//No territory so we can't attack!
					continue;
				}
			}
        }
    }
}
