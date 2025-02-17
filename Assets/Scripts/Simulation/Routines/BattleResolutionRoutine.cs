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
		const float battleLengthMultiplier = 5.0f;
		const float damagePerTickApproximater = 0.1f;

		//Each tick the battle system runs over every created battle (stored in the global battle data)
		//and processes it
		//If the battle is being updated by the game loop (this location is lazy) then we only update the battles
		//relationship matrix

		//Get gameworld and universal battle data and history data
		GameWorld gameworld = GameWorld.main;
		//Get specific game world data
		gameworld.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);
		gameworld.GetData(DataTags.Historical, out HistoryData historyData);

		//Get all contact policy data
		Dictionary<int, ContactPolicyData> idToContactData = SimulationManagement.GetEntityIDToData<ContactPolicyData>(DataTags.ContactPolicy);

		//Get all feelings data
		Dictionary<int, FeelingsData> idToFeelingsData = SimulationManagement.GetEntityIDToData<FeelingsData>(DataTags.Feelings);

        //Get all military data
        Dictionary<int, MilitaryData> idToMilitaryData = SimulationManagement.GetEntityIDToData<MilitaryData>(DataTags.Military);

		//Can't remove battles mid foreach loop
		Dictionary<RealSpacePosition, List<GlobalBattleData.Battle>> finishedBattlesToRemove = new Dictionary<RealSpacePosition, List<GlobalBattleData.Battle>>();
		foreach (KeyValuePair<RealSpacePosition, List<GlobalBattleData.Battle>> entry in globalBattleData.cellCenterToBattles)
		{
			int battleCount = entry.Value.Count;
			for (int b = 0; b < battleCount; b++)
			{
				GlobalBattleData.Battle battle = entry.Value[b];
				List<int> involvedEntities = battle.GetInvolvedEntities();

				//Compute opposition matrix for this battle
				//Computed even if battle is not lazy as outside systems may want to use it
				battle.oppositionMatrix.Clear();
				//For every entity in the battle
				//Something about this is wrong, not sure what but def something. Battle is saying it is over before it is, and oppositon matrix looks empty
				//(even when I can verify that two entities are in conflict)
				foreach (int id in involvedEntities)
				{
					battle.oppositionMatrix.Add(id, new List<int>());

					if (idToContactData.ContainsKey(id) && idToContactData[id].openlyHostile)
					{
						foreach (int otherID in involvedEntities)
						{
							if (otherID != id)
							{
								battle.oppositionMatrix[id].Add(otherID);
							}
						}
					}
					else if (idToFeelingsData.ContainsKey(id))
					{
						FeelingsData feelingsData = idToFeelingsData[id];
						//For every entity in the battle
						foreach (int otherID in involvedEntities)
						{
							//If not this entity
							if (otherID == id)
							{
								continue;
							}

							if (idToContactData.ContainsKey(otherID) && idToContactData[otherID].openlyHostile)
							{
								//If other entity is outwardly hostile
								//add to opposition matrix
								battle.oppositionMatrix[id].Add(otherID);
							}
							else if (feelingsData.idToFeelings.ContainsKey(otherID))
							{
								//In conflict with this entity...
								if (feelingsData.idToFeelings[otherID].inConflict)
								{
									//...add to opposition matrix
									battle.oppositionMatrix[id].Add(otherID);
								}
							}
						}
					}
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

					if (!idToMilitaryData.ContainsKey(id))
					{
						continue;
					}

					MilitaryData militaryData = idToMilitaryData[id];

					if (militaryData.positionToFleets.ContainsKey(battle.postion))
					{
						List<ShipCollection> shipsInCell = militaryData.positionToFleets[battle.postion];
						if (shipsInCell.Count > 0)
						{
							int totalShipCount = 0;

							foreach (ShipCollection collection in shipsInCell)
							{
								List<Ship> ships = collection.GetShips();

								foreach (Ship ship in ships)
								{
									if (!ship.isWreck)
									{
										//If not a wreck
										totalShipCount++;
									}
								}
							}

							if (totalShipCount > 0)
							{
								//If this entity has any ships in this chunk
								shipCollections.Add((id, shipsInCell, totalShipCount));
							}
						}
					}
				}

				battle.anyShipsInBattle = shipCollections.Count > 0;

				//(Battles are VisitableLocations so we can just pass the battle itself)
				if (!SimulationManagement.LocationIsLazy(battle))
				{
					//This battle is being proccessed by the typical game loop
					//not by the simulation
					continue;
				}

				if (!battle.anyShipsInBattle)
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
							if (battle.oppositionMatrix[shipCollections[a].Item1].Contains(shipCollections[c].Item1))
							{
								battleOver = false;
							}
						}
					}

					//If the battle is over we should use the number of ships to add to win progress
					Dictionary<int, float> idToDamageToTake = new Dictionary<int, float>();

					for (int i = 0; i < shipCollections.Count; i++)
					{
						int id = shipCollections[i].Item1;
						if (battleOver)
						{
							//Just use total number of ships to add to win progress
							int totalShips = shipCollections[i].Item3;

							float amountToAddToWinProgress = Mathf.Max(totalShips / (10f * battleLengthMultiplier), 0.001f);
							//Apply win progress
							//Because this is only done when the battle is considered over, damage was applied (at least) last tick
							//This means other routines can check their current battles to find destroyed fleets to transfer out
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
									List<StandardSimWeaponProfile> weapons = ship.GetWeapons();

									foreach (StandardSimWeaponProfile weapon in weapons)
									{
										//Damage is reduced by an approximater value, this is because in reality ships won't always be able to attack each other
										//plus numerous other factors if we are being honest
										totalDamage += weapon.GetDamageLazy() * damagePerTickApproximater;
									}
								}
							}

							//Add damage that needs to be taken by other factions
							List<int> opposition = battle.oppositionMatrix[id];
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

					//Only ever change the health state of ships in the battle if not over/no win progress being added/battle can't end this tick
					//This is to allow other routines to check their current battles for troops to transfer out before the battle is removed from 
					//their data
					if (!battleOver)
					{
						//Apply damage to all involved entities
						foreach (int id in involvedEntities)
						{
							if (idToDamageToTake.ContainsKey(id) && idToMilitaryData.ContainsKey(id))
							{
								float damageToTake = idToDamageToTake[id];

								MilitaryData militaryData = idToMilitaryData[id];

								if (militaryData.positionToFleets.ContainsKey(battle.postion))
								{
									List<ShipCollection> collections = militaryData.positionToFleets[battle.postion];

									float damagePerFleet = damageToTake / collections.Count;

									for (int i = 0; i < collections.Count; i++)
									{
										//Each time damage is taken
										//Add to total damage buildup
										militaryData.totalDamageBuildup += damagePerFleet;

										//Have all ships in a fleet take equal damage
										collections[i].TakeDamage(damagePerFleet);
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
					//End this battle properly
					//Will be removed from the global battle data later as to not disrupt the loop
					battle.End(battle.postion, entry.Key, historyData, winnerID);

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
		foreach (KeyValuePair<RealSpacePosition, List<GlobalBattleData.Battle>> entry in finishedBattlesToRemove)
		{
			foreach (GlobalBattleData.Battle battle in entry.Value)
			{
				globalBattleData.RemoveBattle(entry.Key, battle);
			}
		}
	}
}
