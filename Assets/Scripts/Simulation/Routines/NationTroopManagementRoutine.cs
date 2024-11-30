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

			if (warData.atWarWith.Count <= 0)
			{
				continue;
			}

			MilitaryData militaryData = idToMilitaryData[nation.id];

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
			}
			else
			{
				//Transfer ships back to settlements
				nation.GetData(Faction.Tags.Settlements, out SettlementData setData);

				//Get first settlement
				targets.Add(setData.settlements.ElementAt(0).Key);
			}

			//Currently static
			int transferBudger = 10;

			int maxTransferPer = Mathf.CeilToInt(transferBudger / (float)targets.Count);
			foreach (RealSpacePostion target in targets)
			{
				transferBudger -= militaryData.TransferFreeUnits(maxTransferPer, target, battleData);
			}
		}
	}
}
