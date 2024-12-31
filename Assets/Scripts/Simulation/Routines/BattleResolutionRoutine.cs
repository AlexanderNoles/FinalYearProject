using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using EntityAndDataDescriptor;

[SimulationManagement.ActiveSimulationRoutine(-100)]
public class BattleResolutionRoutine : RoutineBase
{
	public override void Run()
	{
		const float battleLengthMultiplier = 100.0f;

		//Each tick the battle system runs over every created battle (stored in the global battle data)
		//and processes it

		//Get gameworld and universal battle data and histroy data
		GameWorld gameworld = GameWorld.main;
		//Get specific game world data
		gameworld.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);
		gameworld.GetData(DataTags.Historical, out HistoryData historyData);

		//Get all at war factions
		List<Faction> allFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);
		Dictionary<int, MilitaryData> idToMilitaryData = SimulationManagement.GetDataForFactionsList<MilitaryData>(allFactions, Faction.Tags.HasMilitary.ToString());
		Dictionary<int, BattleData> idToBattleData = SimulationManagement.GetDataForFactionsList<BattleData>(allFactions, Faction.battleDataKey);
		Dictionary<int, List<int>> idToOppositionIDs = new Dictionary<int, List<int>>();

		//Pre compute opposition
		foreach (Faction faction in allFactions)
		{
			if (faction.GetData(Faction.relationshipDataKey, out FeelingsData data))
			{
				List<int> newOpposition = new List<int>();

				foreach (KeyValuePair<int, FeelingsData.Relationship> relationship in data.idToFeelings)
				{
					//Change this to some other kinda of check so we can get rid of inConflict (we want to keep things dynamic and setting this flag hampers that)
					if (relationship.Value.inConflict)
					{
						//In conflict
						newOpposition.Add(relationship.Key);
					}
				}

				idToOppositionIDs.Add(faction.id, newOpposition);
			}
		}

		for (int b = 0; b < globalBattleData.battles.Count; b++)
		{
			KeyValuePair<RealSpacePostion, GlobalBattleData.Battle> battleKVP = globalBattleData.battles.ElementAt(b);

			//Battles are visitable locations so we can just pass the battle
			if (!SimulationManagement.PositionIsLazy(battleKVP.Value.GetPosition()))
			{
				//This battle is being proccessed by the typical game loop
				//not by the simulation
				continue;
			}

			GlobalBattleData.Battle battle = battleKVP.Value;

			List<int> involvedFactions = battle.GetInvolvedFactions();
			int involvedFactionsCount = involvedFactions.Count;

			//First we need to khow many ships are in this battle for each faction 
			//ID
			//Ships
			//Ship count
			//This is important because if no ships are hostile then we need to treat this as a battle that has ended (even if only temporarily)

			List<(int, List<ShipCollection>, int)> shipCollections = new List<(int, List<ShipCollection>, int)>();

			for (int i = 0; i < involvedFactionsCount; i++)
			{
				int id = involvedFactions[i];
				MilitaryData militaryData = idToMilitaryData[id];

				if (militaryData.cellCenterToFleets.ContainsKey(battleKVP.Key))
				{
					List<ShipCollection> shipsInCell = militaryData.cellCenterToFleets[battleKVP.Key];
					if (shipsInCell.Count > 0)
					{
						int totalShipCount = 0;

						foreach (ShipCollection collection in shipsInCell)
						{
							totalShipCount += collection.GetShips().Count;
						}

						if (totalShipCount > 0)
						{
							//If this faction has any ships in this chunk
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
						if (idToOppositionIDs[shipCollections[a].Item1].Contains(shipCollections[c].Item1))
						{
							battleOver = false;
						}
					}
				}

				//Calculate how much damage each faction in chunk should take
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
						battle.AddToWinProgress(involvedFactions.IndexOf(id), amountToAddToWinProgress);
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
						List<int> opposition = idToOppositionIDs[id];
						float damagePerEnemy = totalDamage / opposition.Count;

						foreach (int enemyID in opposition)
						{
							if (!involvedFactions.Contains(enemyID))
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
					//Apply damage to all involved factions
					foreach (int id in involvedFactions)
					{
						if (idToDamageToTake.ContainsKey(id))
						{
							float damageToTake = idToDamageToTake[id];

							MilitaryData militaryData = idToMilitaryData[id];

							if (militaryData.cellCenterToFleets.ContainsKey(battleKVP.Key))
							{
								List<ShipCollection> collections = militaryData.cellCenterToFleets[battleKVP.Key];

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
										militaryData.RemoveFleet(battleKVP.Key, collections[i] as Fleet);
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
			//not in the above if(!battleOver) scope so if an outside force removes a faction battles don't freeze in place
			if (battle.BattleWon(out int winnerID))
			{
				battle.ResolveTerritory(battleKVP.Key, historyData, winnerID);

				//For all remaing factions we need to remove their ongoing battle
				battle.End(battleKVP.Key);

				if (!globalBattleData.battles.Remove(battleKVP.Key))
				{
					Console.Log("Trying to remove a battle that doesn't exist!");
				}

				//We remove battle from list
				//Need to decrement index so we don't skip a battle
				b--;
			}
		}
	}
}
