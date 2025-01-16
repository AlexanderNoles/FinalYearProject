using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(SimulationManagement.defendRoutineStandardPrio, SimulationManagement.SimulationRoutine.RoutineTypes.Normal)]
public class MilitaryTroopManagementRoutine : RoutineBase
{
	public override void Run()
	{
		//Get gameworld and universal battle data
		GameWorld gameworld = GameWorld.main;
		gameworld.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);

		//Get all military data
		//This needs to be accessed by id by other militaries so get in dictionary form
		Dictionary<int, MilitaryData> idToMilitary = SimulationManagement.GetEntityIDToData<MilitaryData>(DataTags.Military);

		foreach (KeyValuePair<int, MilitaryData> entry in idToMilitary)
		{
            int id = entry.Key;
            MilitaryData militaryData = entry.Value;

            bool hasBattleData = militaryData.TryGetLinkedData(DataTags.Battle, out BattleData battleData);
            bool hasSettlements = militaryData.TryGetLinkedData(DataTags.Settlement, out SettlementData setData);
            bool hasFeelings = militaryData.TryGetLinkedData(DataTags.Feelings, out FeelingsData feelingsData);

            //Currently static
            int transferBudget = 10;

            //If we have any avaliable battles
            List<RealSpacePostion> targets = new List<RealSpacePostion>();
            if (hasBattleData && battleData.positionToOngoingBattles.Count > 0)
            {
                List<float> targetImportance = new List<float>();
                //Iterate through current battles and calculate the most important ones
                //Then transfer free fleets to those 
                foreach (KeyValuePair<RealSpacePostion, GlobalBattleData.Battle> battle in battleData.positionToOngoingBattles)
                {
                    List<int> involvedEntities = battle.Value.GetInvolvedEntities();

                    //Calculate opposing force
                    int oppositionCount = 0;
                    int personalCount = 0;
                    foreach (int otherID in involvedEntities)
                    {
                        bool isSelf = otherID == id;

                        if (!isSelf || (hasFeelings && feelingsData.idToFeelings.ContainsKey(otherID) && !feelingsData.idToFeelings[otherID].inConflict))
                        {
                            //If this is not us or we are not in conflict with them
                            continue;
                        }

                        int totalShips = 0;
                        if (idToMilitary[otherID].positionToFleets.ContainsKey(battle.Key))
                        {
                            List<ShipCollection> shipCollections = idToMilitary[otherID].positionToFleets[battle.Key];

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
                        targets.Add(battle.Key);
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
                                    targets.Insert(i, battle.Key);
                                    added = true;
                                }
                            }

                            //If exceeded limit
                            if (i + 1 >= targetLocationLimit)
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
            else if (hasSettlements)
            {
                //There are no battles going on so just transfer ships back to settlements (if we have them)
                if (setData.settlements.Count > 0)
                {
                    //Get random position among settlements
                    RealSpacePostion targetPos = setData.settlements.ElementAt(SimulationManagement.random.Next(0, setData.settlements.Count)).Key;

                    //Find ships that are out of nation
                    List<(RealSpacePostion, int)> posAndCount = new List<(RealSpacePostion, int)>();

                    //Seperate out into two loops to avoid change while iterating over error
                    //Current obvious alternative is very unperformant (element at with a standard for loop)
                    foreach (KeyValuePair<RealSpacePostion, List<ShipCollection>> heldPosition in militaryData.positionToFleets)
                    {
                        if (!setData.settlements.ContainsKey(heldPosition.Key))
                        {
                            //Not in settlement
                            //Transfer as many of these ships as possible
                            int thisTransferBudget = Mathf.Min(transferBudget, heldPosition.Value.Count);

                            posAndCount.Add((heldPosition.Key, thisTransferBudget));

                            transferBudget -= thisTransferBudget;
                        }

                        if (transferBudget <= 0)
                        {
                            break;
                        }
                    }

                    //Transfer fleets back to settlements
                    foreach ((RealSpacePostion, int) target in posAndCount)
                    {
                        for (int i = 0; i < target.Item2; i++)
                        {
                            Fleet transferTarget = militaryData.RemoveFleet(target.Item1);
                            militaryData.AddFleet(targetPos, transferTarget);
                        }
                    }
                }
            }
        }
	}
}
