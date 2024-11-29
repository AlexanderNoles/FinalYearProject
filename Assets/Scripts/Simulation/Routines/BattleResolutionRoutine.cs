using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(-100)]
public class BattleResolutionRoutine : RoutineBase
{
	public override void Run()
	{
		//Get gameworld and universal battle data
		GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
		gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

		//Get all at war factions
		List<Faction> atWarFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.AtWar);
		Dictionary<int, MilitaryData> idToMilitaryData = SimulationManagement.GetDataForFactionsList<MilitaryData>(atWarFactions, Faction.Tags.HasMilitary.ToString());
		Dictionary<int, BattleData> idToBattleData = SimulationManagement.GetDataForFactionsList<BattleData>(atWarFactions, Faction.battleDataKey);
		Dictionary<int, List<int>> idToOppositionIDs = new Dictionary<int, List<int>>();

		//Pre compute opposition
		foreach (Faction faction in atWarFactions)
		{
			if (faction.GetData(Faction.relationshipDataKey, out RelationshipData data))
			{
				List<int> newOpposition = new List<int>();

				foreach (KeyValuePair<int, RelationshipData.Relationship> relationship in data.idToRelationship)
				{
					if (relationship.Value.conflict > 0)
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
			if (!SimulationManagement.LocationIsLazy(battleKVP.Value))
			{
				//This battle is being proccessed by the typical game loop
				//not by the simulation
				continue;
			}

			GlobalBattleData.Battle battle = battleKVP.Value;

			List<int> involvedFactions = battle.GetInvolvedFactions();
			int involvedFactionsCount = involvedFactions.Count;
			bool battleOver = battle.NoConflictingFactions();

			//If enemies are still left this will be used
			Dictionary<int, float> idToDamageToTake = new Dictionary<int, float>();

			//Because this is a lazy battle we can just estimate the damage to each ship per tick
			//For this we first need to calculate relative power of each faction
			//And get their opposition
			//And then apply their damage spread evenly across all enemy ships 
			//(Some variance to damage calulation of course)
			const float battleLengthMultiplier = 10.0f;

			for (int i = 0; i < involvedFactionsCount; i++)
			{
				int id = involvedFactions[i];
				MilitaryData militaryData = idToMilitaryData[id];

				if (militaryData.cellCenterToFleets.ContainsKey(battleKVP.Key))
				{
					//Has ships in this cell
					//Calculate damage
					List<ShipCollection> collections = militaryData.cellCenterToFleets[battleKVP.Key];

					float amountToAddToWinProgress = 0.0f;
					if (battleOver)
					{
						int totalShips = 0;

						foreach (ShipCollection collection in collections)
						{
							totalShips += collection.GetShips().Count;
						}

						amountToAddToWinProgress = Mathf.Max(totalShips / (250.0f * battleLengthMultiplier), 0.001f);
					}
					else
					{
						float totalDamage = 0;
						foreach (ShipCollection collection in collections)
						{
							List<Ship> collectionShips = collection.GetShips();

							foreach (Ship ship in collectionShips)
							{
								totalDamage += ship.GetDamageWithVariance();
							}
						}

						amountToAddToWinProgress += totalDamage / (500.0f * battleLengthMultiplier);

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

					battle.AddToWinProgress(i, amountToAddToWinProgress);
				}
			}

			if (!battleOver)
			{
				//Apply damage to all involved factions
				List<int> factionsLostBattleThisTick = new List<int>();

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
								if (collections[i].TakeDamage(damagePerFleet))
								{
									//All ships destroyed
									//Remove fleet from cell
									collections.RemoveAt(i);
								}
								else
								{
									i++;
								}
							}

							if (collections.Count == 0)
							{
								//Remove them from the involved factions
								factionsLostBattleThisTick.Add(id);
							}
						}
					}
				}

				//Any change in number of participants
				if (factionsLostBattleThisTick.Count > 0)
				{
					foreach (int id in factionsLostBattleThisTick)
					{
						//No ships here
						idToMilitaryData[id].cellCenterToFleets.Remove(battleKVP.Key);

						//Remove from battle
						battle.RemoveInvolvedFaction(id);
						idToBattleData[id].ongoingBattles.Remove(battleKVP.Key);
					}
				}
			}

			//Check if battle is won
			//not in the above if statement so if an outside force removes a faction battles don't freeze in place
			if (battle.BattleWon(out int winnerID))
			{
				battle.ResolveTerritoryTransfer(battleKVP.Key);

				//For all remaing factions we need to remove their ongoing battle
				battle.End(battleKVP.Key);

				//We remove battle from list
				//Need to decrement index so we don't skip a battle
				b--;
				globalBattleData.battles.Remove(battleKVP.Key);
			}
		}
	}
}
