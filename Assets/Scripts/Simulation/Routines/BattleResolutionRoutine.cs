using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(-100)]
public class BattleResolutionRoutine : RoutineBase
{
	public override void Run()
	{
		const float battleLengthMultiplier = 10.0f;

		//Each tick the battle system runs over every created battle (stored in the global battle data)
		//and processes it
		//If the battle is being updated by the game loop (this location is lazy) then we only update the battles
		//relationship matrix

		//Get gameworld and universal battle data and history data
		GameWorld gameworld = GameWorld.main;
		//Get specific game world data
		gameworld.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);
		gameworld.GetData(DataTags.Historical, out HistoryData historyData);

        //Get all feelings data
        Dictionary<int, FeelingsData> idToFeelingsData = SimulationManagement.GetEntityIDToData<FeelingsData>(DataTags.Feelings);

        //Get all military data
        Dictionary<int, MilitaryData> idToMilitaryData = SimulationManagement.GetEntityIDToData<MilitaryData>(DataTags.Military);

		//Can't remove battles mid foreach loop
		Dictionary<RealSpacePostion, List<GlobalBattleData.Battle>> finishedBattlesToRemove = new Dictionary<RealSpacePostion, List<GlobalBattleData.Battle>>();
		foreach (KeyValuePair<RealSpacePostion, List<GlobalBattleData.Battle>> entry in globalBattleData.cellCenterToBattles)
		{
			int battleCount = entry.Value.Count;
			for (int b = 0; b < battleCount; b++)
			{
				GlobalBattleData.Battle battle = entry.Value[b];
				List<int> involvedEntities = battle.GetInvolvedEntities();

				//Compute opposition matrix for this battle
				//Computed even if battle is not lazy as outside systems may want to use it
				battle.opositionMatrix.Clear();
				//For every entity in the battle
				foreach (int id in involvedEntities)
				{
					battle.opositionMatrix.Add(id, new List<int>());

					if (idToFeelingsData.ContainsKey(id))
					{
						FeelingsData feelingsData = idToFeelingsData[id];
						//For every entity in the battle
						foreach (int otherID in involvedEntities)
						{
							if (otherID != id && feelingsData.idToFeelings.ContainsKey(id))
							{
								//If not this entity and in conflict with this entity...
								if (feelingsData.idToFeelings[otherID].inConflict)
								{
									//...add to opposition matrix
									battle.opositionMatrix[id].Add(otherID);
								}
							}
						}
					}
				}

				//Battles are visitable locations so we can just pass the battle itself
				if (!SimulationManagement.LocationIsLazy(battle))
				{
					//This battle is being proccessed by the typical game loop
					//not by the simulation
					continue;
				}

				int involvedEntitiesCount = involvedEntities.Count;

				//First we need to know many units are in this battle for each entity 
				//ID
				//Ships
				//Ship count
				//This is important because if no ships are hostile then we need to treat this as a battle that has ended (even if only temporarily)

				List<(int, List<ShipCollection>, int)> shipCollections = new List<(int, List<ShipCollection>, int)>();

				for (int i = 0; i < involvedEntitiesCount; i++)
				{
					int id = involvedEntities[i];
					MilitaryData militaryData = idToMilitaryData[id];

					if (militaryData.positionToFleets.ContainsKey(battle.postion))
					{
						List<ShipCollection> shipsInCell = militaryData.positionToFleets[battle.postion];
						if (shipsInCell.Count > 0)
						{
							int totalShipCount = 0;

							foreach (ShipCollection collection in shipsInCell)
							{
								totalShipCount += collection.GetShips().Count;
							}

							if (totalShipCount > 0)
							{
								//If this entity has any ships in this chunk
								shipCollections.Add((id, shipsInCell, totalShipCount));
							}
						}
					}
				}

				if (shipCollections.Count <= 0)
				{
					//Currently no one involved in this battle
					//This can occur from a dead draw between ships
					battle.backgroundProgression += 0.01f;
				}
				else
				{
					//Validate that there are still active hostilities
					bool battleOver = true;
					for (int a = 0; a < shipCollections.Count && battleOver; a++)
					{
						for (int c = a + 1; c < shipCollections.Count && battleOver; c++)
						{
							if (battle.opositionMatrix[shipCollections[a].Item1].Contains(shipCollections[c].Item1))
							{
								battleOver = false;
							}
						}
					}

					//Calculate how much damage each entity in chunk should take
					//Or if the battle is over we should use the number of ships to add to win progress
					Dictionary<int, float> idToDamageToTake = new Dictionary<int, float>();
					float amountToAddToWinProgress = 0.0f;

					for (int i = 0; i < shipCollections.Count; i++)
					{
						int id = shipCollections[i].Item1;
						if (battleOver)
						{
							//Just use total number of ships to add to win progress
							int totalShips = shipCollections[i].Item3;

							amountToAddToWinProgress = Mathf.Max(totalShips / (10f * battleLengthMultiplier), 0.001f);
							//Apply win progress
							battle.AddToWinProgress(involvedEntities.IndexOf(id), amountToAddToWinProgress);
						}
						else
						{
							//Use damage dealt to other ships to add to win progress
							float totalDamage = 0;

							List<ShipCollection> collections = shipCollections[i].Item2;

							foreach (ShipCollection collection in collections)
							{
								List<Ship> collectionShips = collection.GetShips();

								foreach (Ship ship in collectionShips)
								{
									//Because this is a lazy battle we can just estimate the damage to each ship per tick
									totalDamage += ship.GetDamageWithVariance();
								}
							}

							//Add damage that needs to be taken by other factions
							List<int> opposition = battle.opositionMatrix[id];
							float damagePerEnemy = totalDamage / opposition.Count;

							foreach (int enemyID in opposition)
							{
								if (!involvedEntities.Contains(enemyID))
								{
									continue;
								}

								if (!idToDamageToTake.ContainsKey(enemyID))
								{
									idToDamageToTake[enemyID] = 0.0f;
								}

								idToDamageToTake[enemyID] += damagePerEnemy;
							}
						}
					}

					if (!battleOver)
					{
						//Apply damage to all involved entities
						foreach (int id in involvedEntities)
						{
							if (idToDamageToTake.ContainsKey(id))
							{
								float damageToTake = idToDamageToTake[id];

								MilitaryData militaryData = idToMilitaryData[id];

								if (militaryData.positionToFleets.ContainsKey(battle.postion))
								{
									List<ShipCollection> collections = militaryData.positionToFleets[battle.postion];

									float damagePerFleet = damageToTake / collections.Count;

									for (int i = 0; i < collections.Count;)
									{
										//Each time damage is taken
										//Add to total damage buildup
										militaryData.totalDamageBuildup += damagePerFleet;

										if (collections[i].TakeDamage(damagePerFleet))
										{
											//All ships destroyed
											//Remove fleet from cell
											militaryData.RemoveFleet(battle.postion, collections[i] as Fleet);
										}
										else
										{
											i++;
										}
									}
								}
							}
						}
					}
				}

				//Check if battle is won
				//not in the above if(!battleOver) scope so if an outside force removes a entity battles don't freeze in place
				if (battle.BattleWon(out int winnerID))
				{
					battle.ResolveTerritory(entry.Key, battle.postion, historyData, winnerID);

					//For all remaing factions we need to remove their ongoing battle
					battle.End(battle.postion);

					//We remove battle from global battle data
					//So add to the toRemove data structure
					if (!finishedBattlesToRemove.ContainsKey(entry.Key))
					{
						finishedBattlesToRemove.Add(entry.Key, new List<GlobalBattleData.Battle>());
					}

					finishedBattlesToRemove[entry.Key].Add(battle);
				}
			}
		}

		//Prune battle data of battles that have been finished
		foreach (KeyValuePair<RealSpacePostion, List<GlobalBattleData.Battle>> entry in finishedBattlesToRemove)
		{
			if (!globalBattleData.cellCenterToBattles.ContainsKey(entry.Key))
			{
				throw new System.Exception("Trying to remove a battle that doesn't exist! Likely that lookup key is wrong!");
			}

			//Get reference to list
			List<GlobalBattleData.Battle> target = globalBattleData.cellCenterToBattles[entry.Key];
			//Remove all battles from list
			foreach (GlobalBattleData.Battle battle in entry.Value)
			{
				target.Remove(battle);
			}

			//If list is now empty
			if (target.Count == 0)
			{
				globalBattleData.cellCenterToBattles.Remove(entry.Key);
			}
		}
	}
}
