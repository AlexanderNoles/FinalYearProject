using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.defendRoutineStandardPrio, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class NationTroopManagementRoutine : RoutineBase
{
	public override void Run()
	{
		//Copied from Discord:
		//
		//Maybe defence should not exist
		//and instead there is just a routine for generally moving about troops
		//and then the attack routine becomes specifically about wars
		//a.k.a attack routine sets battles for us to take in a current war
		//then "defence" routine moves troops about just to any current battle
		//not just responding to specific defense requests
		//so each tick we just reavluate all current battles and transfer troops based on needs
		
		//Get gameworld and universal battle data
		GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
		gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

		//Manage our current ship collections
		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);
		Dictionary<int, MilitaryData> idToMilitaryData = SimulationManagement.GetDataForFactionsList<MilitaryData>(SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction), Faction.Tags.HasMilitary.ToString());

		foreach (Faction nation in nations)
		{
			//Get nation mil data and battle data
			nation.GetData(Faction.battleDataKey, out BattleData battleData);
			nation.GetData(Faction.Tags.CanFightWars, out WarData warData);

			MilitaryData militaryData = idToMilitaryData[nation.id];

			//Currently static
			int transferBudget = 10;

			//If we have any avaliable battles
			List<RealSpacePostion> targets = new List<RealSpacePostion>();
			if (battleData.ongoingBattles.Count > 0)
			{
				List<float> targetImportance = new List<float>();
				//Iterate through current battles and calculate the most important ones
				//Then transfer free fleets to those 
				foreach (KeyValuePair<RealSpacePostion, BattleData.BattleReference> entry in battleData.ongoingBattles)
				{
					List<int> involvedFactions = globalBattleData.battles[entry.Key].GetInvolvedFactions();

					//Calculate opposing force
					int oppositionCount = 0;
					int personalCount = 0;
					foreach (int otherID in involvedFactions)
					{
						bool isSelf = otherID == nation.id;

						if (!warData.atWarWith.Contains(otherID) && !isSelf)
						{
							continue;
						}

						int totalShips = 0;
						if (idToMilitaryData[otherID].cellCenterToFleets.ContainsKey(entry.Key))
						{
							List<ShipCollection> shipCollections = idToMilitaryData[otherID].cellCenterToFleets[entry.Key];

							foreach (ShipCollection collection in shipCollections)
							{
								totalShips += collection.GetShips().Count;
							}
						}

						if (isSelf)
						{
							personalCount = totalShips;							
						}
						else
						{
							oppositionCount += totalShips;
						}
					}

					//If positive higher importance a.k.a we have less ships in this cell
					int difference = oppositionCount - personalCount;
					//

					const int targetLocationLimit = 3;
					if (targetImportance.Count < targetLocationLimit)
					{
						targets.Add(entry.Key);
						targetImportance.Add(difference);
					}
					else
					{
						//Remove lower importance positions
						bool added = false;
						for (int i = 0; i < targetImportance.Count; i++)
						{
							if (!added)
							{
								if (difference > targetImportance[i])
								{
									targetImportance.Insert(i, difference);
									targets.Insert(i, entry.Key);
									added = true;
								}
							}

							//If exceeded limit
							if (i+1 >= targetLocationLimit)
							{
								targets.RemoveAt(i);
								targetImportance.RemoveAt(i);
								//We only remove one at a time so we can just remove this last one
								break;
							}
						}
					}
				}

				int maxTransferPer = Mathf.CeilToInt(transferBudget / (float)targets.Count);
				foreach (RealSpacePostion target in targets)
				{
					transferBudget -= militaryData.TransferFreeUnits(maxTransferPer, target, battleData);
				}
			}
			else
			{
				//There are no battles going on so just transfer ships back to settlements
				nation.GetData(Faction.Tags.Settlements, out SettlementData setData);

				if (setData.settlements.Count > 0)
				{
					//Get random position among settlements
					RealSpacePostion targetPos = setData.settlements.ElementAt(SimulationManagement.random.Next(0, setData.settlements.Count)).Key;

					//Find ships that are out of nation
					List<(RealSpacePostion, int)> posAndCount = new List<(RealSpacePostion, int)>();

					//Seperate out into two loops to avoid change while iterating over error
					//Current obvious alternative is very unperformant
					foreach (KeyValuePair<RealSpacePostion, List<ShipCollection>> entry in militaryData.cellCenterToFleets)
					{
						if (!setData.settlements.ContainsKey(entry.Key))
						{
							//Not in settlement
							//Transfer as many of these ships as possible
							int thisTransferBudget = Mathf.Min(transferBudget, entry.Value.Count);

							posAndCount.Add((entry.Key, thisTransferBudget));

							transferBudget -= thisTransferBudget;
						}

						if (transferBudget <= 0)
						{
							break;
						}
					}

					//Transfer fleets back to settlements
					foreach ((RealSpacePostion, int) entry in posAndCount)
					{
						for (int i = 0; i < entry.Item2; i++)
						{
							Fleet transferTarget = militaryData.RemoveFleet(entry.Item1);
							militaryData.AddFleet(targetPos, transferTarget);
						}
					}
				}
			}
		}
	}
}
