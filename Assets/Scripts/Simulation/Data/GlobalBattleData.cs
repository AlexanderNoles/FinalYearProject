using EntityAndDataDescriptor;
using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using static GlobalBattleData;
using static GlobalBattleData.Battle.DrawnData;

public class GlobalBattleData : DataModule
{
	public class Battle : VisitableLocation
	{
		public RealSpacePosition postion;
		public float backgroundProgression;

		public int startTickID;
		public int firstAttacker = -1;
		public int defender = -1;
		private const float winLimit = 1.2f;

		private List<int> involvedEntities = new List<int>();
		private List<float> involvedEntitiesProgress = new List<float>();
		public bool anyShipsInBattle = true;

		public Dictionary<int, List<int>> oppositionMatrix = new Dictionary<int, List<int>>();

		public List<int> GetInvolvedEntities()
		{
			return involvedEntities;
		}

		public void AddInvolvedEntity(int id)
		{
			involvedEntities.Add(id);
			involvedEntitiesProgress.Add(0.0f);

			drawnData?.involvedEntityDifference.Add(id);
		}

		public void RemoveInvolvedEntity(int id)
		{
			int indexOf = involvedEntities.IndexOf(id);

			if (indexOf == -1)
			{
				return;
			}

			involvedEntities.RemoveAt(indexOf);
			involvedEntitiesProgress.RemoveAt(indexOf);

			drawnData?.involvedEntityDifference.Add(id);
		}

		public void AddToWinProgress(int index, float amount)
		{
			involvedEntitiesProgress[index] += amount;
		}

		public float GetWinProgress(int index)
		{
			return involvedEntitiesProgress[index];
		}

		public bool BattleWon(out int winnerID)
		{
			winnerID = -1;

			if (involvedEntities.Count <= 0 || backgroundProgression >= 1.0f)
			{
				return true;
			}

			for (int i = 0; i < involvedEntities.Count; i++)
			{
				if (involvedEntitiesProgress[i] > winLimit)
				{
					winnerID = involvedEntities[i];
					return true;
				}
			}

			return false;
		}

		public void End(RealSpacePosition actualPos, RealSpacePosition cellCenter, HistoryData historyData, int winnerID)
		{
			//Reslove the territory loss
			ResolveTerritory(cellCenter, historyData, winnerID);

			//For all remaing factions we need to remove their ongoing battle
			foreach (int id in involvedEntities)
			{
				SimulationEntity current = SimulationManagement.GetEntityByID(id);

				if (current == null)
				{
					//This entity has just been removed from the simulation, so we are trying to remove it from this battle
					//but that means we won't be able to find it!
					//So just continue to the next iteration
					continue;
				}

				if (current.GetData(DataTags.Battle, out BattleData data))
				{
					if (!data.positionToOngoingBattles.Remove(actualPos))
					{
						Debug.LogWarning("Entity was unaware of battle it was in!");
					}
					else
					{
						//Battle ended succesfully
						if (data.TryGetLinkedData(DataTags.Strategy, out StrategyData strategyData))
						{
							//Need to ask strategy data what to do on a battle end
							strategyData.OnBattleEnd(actualPos);
						}
					}
				}
			}

			involvedEntities.Clear();
		}

		public void ResolveTerritory(RealSpacePosition cellCenterOfPos, HistoryData historyData, int winnerID)
		{
			// Decide new owner //
			if (winnerID == -1)
			{
				return;
			}

			if (winnerID == defender)
			{
				//Defender is the winner so nothing happens
				return;
			}

			//Otherwise if winner is hostile to defender than we remove this cell from the defender
			SimulationEntity wonEntity = SimulationManagement.GetEntityByID(winnerID);
			SimulationEntity lostEntity = SimulationManagement.GetEntityByID(defender);

			if (lostEntity == null)
			{
				return;
			}

			if (lostEntity.GetData(DataTags.TargetableLocation, out TargetableLocationData targetableLocationData))
			{
				if (targetableLocationData.actualPosition.Equals(postion))
				{
					targetableLocationData.OnDeath();
				}
			}
			else
			{
				//If in conflict or won entity is openly hostile
				bool territoryLost = 
					(wonEntity.GetData(DataTags.Feelings, out FeelingsData relData) && relData.idToFeelings.ContainsKey(defender) && relData.idToFeelings[defender].inConflict) ||
					(wonEntity.GetData(DataTags.ContactPolicy, out ContactPolicyData contactPolicy) && contactPolicy.openlyHostile);

				if (territoryLost)
				{
					if (lostEntity.GetData(DataTags.Territory, out TerritoryData lossData))
					{
						bool territoryRemoved = wonEntity.GetData(DataTags.Strategy, out StrategyData wonStratData) && wonStratData.removeTerritory;

						if (territoryRemoved)
						{
							lossData.RemoveTerritory(cellCenterOfPos);
						}

						float modifier = 1.0f;

						//If this entity is at war with the winner add some additional
						//or the winner is at war with this entity
						//war exhaustion for losing a territory
						if (lostEntity.GetData(DataTags.Strategy, out StrategyData strategyData))
						{
							//Apply war exhaustion
							if (strategyData is WarStrategyData)
							{
								WarStrategyData warData = (WarStrategyData)strategyData;
								bool applyWarExhaustion = false;

								//If at war with winner
								if (warData.atWarWith.Contains(winnerID))
								{
									applyWarExhaustion = true;
								}
								//Or winner is at war with us
								else if (wonEntity.GetData(DataTags.Strategy, out StrategyData wonStrategyData) && wonStrategyData is WarStrategyData)
								{
									WarStrategyData wonWarData = (WarStrategyData)wonStrategyData;

									if (wonWarData.atWarWith.Contains(defender))
									{
										applyWarExhaustion = true;

										//Make this faction now at war with winner
										warData.atWarWith.Add(winnerID);
									}
								}

								if (applyWarExhaustion)
								{
									modifier = warData.warExhaustion;
									warData.warExhaustion += 10.0f * warData.warExhaustionGrowthMultiplier;
								}
							}
						}

						//Set the lost entity to be inConflict with the Won Entity
						if (lostEntity.GetData(DataTags.Feelings, out FeelingsData feelingsData))
						{
							if (feelingsData.idToFeelings.ContainsKey(winnerID))
							{
								feelingsData.idToFeelings[winnerID].inConflict = true;
							}
						}

						//Apply territory cap loss
						//This is too ensure entites don't just get stuck in eternal wars
						//where they keep claiming
						lossData.territoryClaimUpperLimit -= 2f * modifier;

						//Transfer previously owned faction to history data
						if (historyData != null && territoryRemoved)
						{
							if (!historyData.previouslyOwnedTerritories.ContainsKey(cellCenterOfPos))
							{
								historyData.previouslyOwnedTerritories.Add(cellCenterOfPos, new HistoryData.HistoryCell());
							}
							else
							{
								Debug.LogWarning("Battle fighting over unowned territory?");
							}
						}
					}

					//Destroy any settlement in this area
					if (lostEntity.GetData(DataTags.Settlements, out SettlementsData setData))
					{
						setData.settlements.Remove(cellCenterOfPos);
					}
				}
			}
		}

		public Battle(RealSpacePosition pos)
		{
			postion = pos;
		}

		public override RealSpacePosition GetPosition()
		{
			return postion;
		}

		public override string GetTitle()
		{
			return "Battlefield";
		}

		public override Color GetMapColour()
		{
			return Color.yellow;
		}

		public override bool FlashOnMap()
		{
			return true;
		}

		public override string GetDescription()
		{
			//Disabled as battle progress accumulates so fast now they just go from 0 to 100
			return base.GetDescription();

			string descString = "";

			for (int i = 0; i < involvedEntities.Count; i++)
			{
				//Only add to description if the target has emblem data
				//This means entites such as mineral deposits won't show up
				if (SimulationManagement.GetEntityByID(involvedEntities[i]).GetData(DataTags.Emblem, out EmblemData emblemData))
				{
					descString += $"<color={emblemData.mainColourHex}>{Mathf.RoundToInt(100.0f * (involvedEntitiesProgress[i] / winLimit))}%</color>";

					if (PlayerManagement.PlayerEntityExists() && involvedEntities[i].Equals(PlayerManagement.GetTarget().id))
					{
						descString += " (You)";
					}

					descString += "\n";
				}
			}

			return descString;
		}

		public override float GetEntryOffset()
		{
			return 100.0f;
		}


		// DRAW FUNCTIONS //
		//Object used to keep track of active ships in the drawn scene
		public class DrawnData
		{
			public Transform parent;
			public Transform battlefieldObject;
			public List<int> involvedEntityDifference = new List<int>();

			public class Participant
			{
				public List<ShipCollectionDrawer> drawnCollections = new List<ShipCollectionDrawer>();
			}

			public Dictionary<int, Participant> idToParticipant = new Dictionary<int, Participant>();

			public float battleMaxScale = 500.0f;
		}

		private DrawnData drawnData = null;

		public override void InitDraw(Transform parent, PlayerLocationManagement.DrawnLocation drawnLocation)
		{
			drawnData = new DrawnData();
			drawnData.parent = parent;
			//Create the battlefield object that will be used to process interactions
			Transform battlefieldObject = GeneratorManagement.GetStructure((int)GeneratorManagement.POOL_INDEXES.BATTLEFIELD);
			battlefieldObject.parent = parent;
			battlefieldObject.localPosition = Vector3.zero;
			battlefieldObject.gameObject.SetActive(true);
			LinkToBehaviour(battlefieldObject);
			//Iterate through all active participants in this battle
			//Generate inital drawn data for them
			//This drawn data will then be updated based on differences per tick
			foreach (int id in involvedEntities)
			{
				Participant participant = new Participant();
				//Do inital draw
				DrawInitialShips(SimulationManagement.GetEntityByID(id), participant, true);

				//Add to data
				drawnData.idToParticipant.Add(id, participant);
			}
		}

		public override void DrawUpdatePostTick()
		{
			//Used to ensure we don't redraw transfered ships that we already drew when we initalized a new participant
			HashSet<int> drawnThisTick = new HashSet<int>();
			//Update drawn data based on changes
			//Involved entity changes are stored as involved entity difference
			foreach (int id in drawnData.involvedEntityDifference)
			{
				if (drawnData.idToParticipant.ContainsKey(id))
				{
					//Needs to be removed
					DrawnData.Participant participant = drawnData.idToParticipant[id];
					drawnData.idToParticipant.Remove(id);

					//Undraw all ship drawers
					UndrawAll(participant);
				}
				else
				{
					//Needs to be added
					DrawnData.Participant participant = new DrawnData.Participant();
					drawnData.idToParticipant.Add(id, participant);

					//For each ship the participant owns in this cell
					//add a ship instance in the scene
					DrawInitialShips(SimulationManagement.GetEntityByID(id), participant, false);
					drawnThisTick.Add(id);
				}
			}

			drawnData.involvedEntityDifference.Clear();

			//Now we need to draw or undraw ships that have been transfered this tick
			//For each participant
			foreach (KeyValuePair<int, DrawnData.Participant> entry in drawnData.idToParticipant)
			{
				SimulationEntity entity = SimulationManagement.GetEntityByID(entry.Key);

				if (entity == null)
				{
					//Participant is dead, so we just need to undraw all their ships, should be handled by the above code next tick
					continue;
				}

				//Get this entites military data
				if (entity.GetData(DataTags.Military, out MilitaryData milData))
				{
					//Check if any collections have been transferred in or out
					//Then draw or undraw accordingly
					if (milData.fromTransfer.ContainsKey(postion))
					{
						List<ShipCollectionDrawer> targetDrawers = entry.Value.drawnCollections;
						List<ShipCollection> collectionsLeft = milData.fromTransfer[postion];

						for (int i = 0; i < targetDrawers.Count; i++)
						{
							int index = collectionsLeft.IndexOf(targetDrawers[i].target);

							//If this collection left this frame
							if (index != -1)
							{
								UnDrawCollection(targetDrawers[i]);
								targetDrawers.RemoveAt(i);
								i--;

								collectionsLeft.RemoveAt(index);
							}
						}
					}

					if (milData.toTransfer.ContainsKey(postion))
					{
						foreach (ShipCollection newTargetCollection in milData.toTransfer[postion])
						{
							//Draw added ship, these ships are always transferring in
							DrawCollection(newTargetCollection, entry.Value, true);
						}
					}

					//Then check for updates for any remaning collections
					foreach (ShipCollectionDrawer drawer in entry.Value.drawnCollections)
					{
						drawer.Update();
					}
				}
			}
		}

		private void DrawInitialShips(SimulationEntity entity, DrawnData.Participant participant, bool onArrival)
		{
			if (entity.GetData(DataTags.Military, out MilitaryData militaryData))
			{
				if (militaryData.positionToFleets.ContainsKey(postion))
				{
					List<ShipCollection> shipCollections = militaryData.positionToFleets[postion];

					foreach (ShipCollection collection in shipCollections)
					{
						DrawCollection(collection, participant, !onArrival);
					}
				}
			}
		}

		private void DrawCollection(ShipCollection collection, DrawnData.Participant participant, bool transferIn)
		{
			//Add each collection as a new collection drawer instance
			ShipCollectionDrawer shipCollectionDrawer = new ShipCollectionDrawer();
			//Set parent
			shipCollectionDrawer.parent = drawnData.parent;
			//Link that collection to this drawer
			shipCollectionDrawer.Link(collection);

			//Do full inital draw
			//Generate spawn info
			ShipCollectionDrawer.SpawnInfo spawnInfo;

			if (transferIn)
			{
				//Transfered in from outside the battle
				//Ideally we would transfer specifically from the direction this collection came from
				//but military data only stores that we transfered to this position or we left from a position
				//done seperately for quicker lookups

				//Would need to iterate through from positions to find this target collection which is rather unperformant
				//We could take the hit but I don't think it matters
				Vector3 pos = GenerationUtility.GetCirclePositionBasedOnPercentage(Random.Range(0.0f, 1.0f), drawnData.battleMaxScale);

				spawnInfo = new ShipCollectionDrawer.SpawnInfo(pos);
			}
			else
			{
				//Spawned from somewhere in the battle, this should mean they where here already when the battle started but the system
				//is open for flexibility reasons

				//Should ask player location management about the current state of the environment
				//For now we just pick a random position within the battle scale that's not too close to the center
				Vector3 center = Random.onUnitSphere;
				center.y = 0.0f;
				center.Normalize();

				center *= Random.Range(100.0f, drawnData.battleMaxScale);

				spawnInfo = new ShipCollectionDrawer.SpawnInfo(center);
			}

			shipCollectionDrawer.DrawShips(spawnInfo);

			//Start tracking collection
			participant.drawnCollections.Add(shipCollectionDrawer);
		}

		private void UnDrawCollection(ShipCollectionDrawer collection)
		{
			collection.UndrawAll();
		}

		private void UndrawAll(DrawnData.Participant participant)
		{
			foreach (ShipCollectionDrawer shipCollectionDrawer in participant.drawnCollections)
			{
				shipCollectionDrawer.UndrawAll();
			}
		}

		public override void Cleanup()
		{
			//Return any remaing drawn ships
			foreach (DrawnData.Participant participant in drawnData.idToParticipant.Values)
			{
				UndrawAll(participant);
			}

			if (drawnData.battlefieldObject != null)
			{
				GeneratorManagement.ReturnStructure((int)GeneratorManagement.POOL_INDEXES.BATTLEFIELD, drawnData.battlefieldObject);
			}

			//Remove drawn data
			drawnData = null;
		}
	}

	public Dictionary<RealSpacePosition, List<Battle>> cellCenterToBattles = new Dictionary<RealSpacePosition, List<Battle>>();
	public static int totalBattlesCount = 0;

	public bool StartOrJoinBattle(RealSpacePosition key, RealSpacePosition actualPos, int originID, int targetID, bool autoMergeToExisting)
	{
		if (!SimulationManagement.EntityExists(originID) || !SimulationManagement.EntityExists(targetID))
		{
			//If eithier don't exist, don't allow battle to happen
			return false;
		}

		if (!cellCenterToBattles.ContainsKey(key))
		{
			//Create list
			cellCenterToBattles[key] = new List<Battle>();
		}

		//Check if battle is already going on
		//If it isn't make a new battle
		Battle battle = null;

		foreach (Battle storedBattle in cellCenterToBattles[key])
		{
			//If merge is enabled we simply join a battle if any exist already
			if (autoMergeToExisting || storedBattle.postion.Equals(actualPos))
			{
				battle = storedBattle;

				if (autoMergeToExisting)
				{
					actualPos = storedBattle.postion;
				}

				break;
			}
		}

		if (battle == null)
		{
			//Start brand new battle
			battle = new Battle(actualPos);
			battle.startTickID = SimulationManagement.currentTickID;

			cellCenterToBattles[key].Add(battle);

			totalBattlesCount++;
		}

		List<int> involvedFactions = battle.GetInvolvedEntities();

		if (!involvedFactions.Contains(originID))
		{
			battle.AddInvolvedEntity(originID);

			if (battle.firstAttacker == -1)
			{
				//Set this faction as the one to claim
				battle.firstAttacker = originID;
			}

			//Add new battle reference
			SimulationEntity attacker = SimulationManagement.GetEntityByID(originID);
			if (attacker.GetData(DataTags.Battle, out BattleData attackBData))
			{
				if (!attackBData.positionToOngoingBattles.ContainsKey(actualPos))
				{
					//This check should always pass
					attackBData.positionToOngoingBattles.Add(actualPos, battle);
				}
				else
				{
					throw new System.Exception("major error: entity does not know it is in a battle it is in!");
				}
			}

			//Defending faction
			if (!involvedFactions.Contains(targetID))
			{
				battle.AddInvolvedEntity(targetID);

				if (battle.defender == -1)
				{
					//Set this faction as the one to claim
					battle.defender = targetID;
				}

				//Get target ongoing battle data and add this battle as a new entry
				SimulationEntity defender = SimulationManagement.GetEntityByID(targetID);

				if (defender.GetData(DataTags.Battle, out BattleData defendBData))
				{
					if (!defendBData.positionToOngoingBattles.ContainsKey(actualPos))
					{
						//Not currently considering defending this spot already
						//Add it
						defendBData.positionToOngoingBattles.Add(actualPos, battle);
					}
				}
			}

			return true;
		}

		return false;
	}

	public bool RemoveBattle(RealSpacePosition cellCenter, Battle battle)
	{
		if (!cellCenterToBattles.ContainsKey(cellCenter))
		{
			Debug.LogWarning("Trying to remove a battle that doesn't exist! Likely that lookup key is wrong!");
			return false;
		}

		if (!cellCenterToBattles[cellCenter].Remove(battle))
		{
			//Nothing removed
			return false;
		}

		if (cellCenterToBattles[cellCenter].Count == 0)
		{
			//Remove the list as it is now empty
			cellCenterToBattles.Remove(cellCenter);
		}

		return true;
	}

	public bool BattleExists(RealSpacePosition pos, out Battle foundBattle)
	{
		foundBattle = null;

		//Generate lookup key
		RealSpacePosition cellCenter = WorldManagement.ClampPositionToGrid(pos);

		if (cellCenterToBattles.ContainsKey(cellCenter))
		{
			List<Battle> battles = cellCenterToBattles[cellCenter];

			foreach (Battle battle in battles)
			{
				if (battle.postion.Equals(pos))
				{
					foundBattle = battle;
					return true;
				}
			}
		}

		return false;
	}

	[MonitorBreak.Bebug.ConsoleCMD("PBattles", "Display player battle information")]
	public static void DisplayPlayerBattleInformation()
	{
		//Iterate through every single battle
		//If the player is a part of it then add to the output string
		string outputString = "";

		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData data);

		foreach (KeyValuePair<RealSpacePosition, List<Battle>> entry in data.cellCenterToBattles)
		{
			foreach (Battle b in entry.Value)
			{
				if (b.GetInvolvedEntities().Contains(PlayerManagement.GetTarget().id))
				{
					outputString += $"\nOpposition Matrix ({b.oppositionMatrix.Count}):\n";

					foreach (KeyValuePair<int, List<int>> line in b.oppositionMatrix)
					{
						string tempString = $"	{line.Key}: ";

						foreach (int opposition in line.Value)
						{
							tempString += $"{opposition}, ";
						}

						outputString += tempString + "\n";
					}

					outputString += "\n";
				}
			}
		}

		MonitorBreak.Bebug.Console.Log(outputString);
	}
}
