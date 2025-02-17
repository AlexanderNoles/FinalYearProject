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
		//Get all military data
		//This needs to be accessed by id by other militaries so get in dictionary form
		Dictionary<int, MilitaryData> idToMilitary = SimulationManagement.GetEntityIDToData<MilitaryData>(DataTags.Military);

		foreach (KeyValuePair<int, MilitaryData> entry in idToMilitary)
		{
            int id = entry.Key;
            MilitaryData militaryData = entry.Value;

            bool hasBattleData = militaryData.TryGetLinkedData(DataTags.Battle, out BattleData battleData);

			//First evaluate all the current battles and try to send free troops if neccesary
			//Then evaluate the retreat buffer and attempt to send some of those troops back to the refinery/settlements for repair

			//If we have any avaliable battles
			if (hasBattleData && battleData.positionToOngoingBattles.Count > 0)
			{
				//Get data
				bool hasFeelings = militaryData.TryGetLinkedData(DataTags.Feelings, out FeelingsData feelingsData);
				//

				//Setup
				List<RealSpacePosition> targets = new List<RealSpacePosition>();
				List<float> targetImportance = new List<float>();
				//

				//Budget
				//Based on overall size but always rounded up so we get atleast 1 allowed transfer
				int transferBudget = Mathf.CeilToInt(militaryData.currentFleetCount / 10.0f);
				//

				//Iterate through current battles and calculate the most important ones
				//Then transfer free fleets to those 
				foreach (KeyValuePair<RealSpacePosition, GlobalBattleData.Battle> battle in battleData.positionToOngoingBattles)
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
					//

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
				foreach (RealSpacePosition target in targets)
				{
					transferBudget -= militaryData.TransferFreeUnits(maxTransferPer, target);
				}
			}

			if (militaryData.retreatBuffer.Count > 0)
			{
				int retreatBudget = Mathf.CeilToInt(militaryData.currentFleetCount / 5.0f);

				//Don't retreat more than allowed
				retreatBudget = Mathf.Min(retreatBudget, militaryData.retreatBuffer.Count);

				//Get target positions
				List<RealSpacePosition> safetyPositions = militaryData.GetSafetyPositions();

				//How many positions to spread to per tick
				//This ensures all the troops don't get put in the same place on retreat
				int spreadCount = 5;
				int retreatPer = Mathf.CeilToInt(retreatBudget / (float)spreadCount);
				int indexOffset = SimulationManagement.random.Next(0, safetyPositions.Count);

				for (int i = 0; i < safetyPositions.Count && retreatBudget > 0; i++)
				{
					int currentIndex = (i + indexOffset) % safetyPositions.Count;

					//Actually retreat the troops
					for (int r = 0; r < retreatPer && retreatBudget > 0; r++)
					{
						ShipCollection fleet = militaryData.RemoveNextFleetFromRetreatBuffer();

						if (fleet != null)
						{
							militaryData.AddFleet(safetyPositions[currentIndex], fleet);
							retreatBudget--;
						}
					}
				}
			}
        }
	}
}
