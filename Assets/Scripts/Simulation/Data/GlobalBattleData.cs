using EntityAndDataDescriptor;
using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBattleData : DataBase
{
	public class Battle : VisitableLocation
	{
		public RealSpacePostion postion;
		public float backgroundProgression;

		public int startTickID;
		public int firstAttacker = -1;
		public int defender = -1;
		private const float winLimit = 1.2f;

		private List<int> involvedEntities = new List<int>();
		private List<float> involvedEntitiesProgress = new List<float>();

		public Dictionary<int, List<int>> opositionMatrix = new Dictionary<int, List<int>>();

		public List<int> GetInvolvedEntities()
		{
			return involvedEntities;
		}

		public void AddInvolvedEntity(int id)
		{
			involvedEntities.Add(id);
			involvedEntitiesProgress.Add(0.0f);
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

		public void End(RealSpacePostion pos)
		{
			foreach (int id in involvedEntities)
			{
				SimulationEntity current = SimulationManagement.GetEntityByID(id);

				if (current.GetData(DataTags.Battle, out BattleData data))
				{
					if (!data.positionToOngoingBattles.Remove(pos))
					{
						Debug.LogWarning("Entity was unaware of battle it was in!");
					}
				}
			}

			involvedEntities.Clear();
		}

		public void ResolveTerritory(RealSpacePostion cellCenterOfPos, HistoryData historyData, int winnerID)
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

            if (wonEntity.GetData(DataTags.Feelings, out FeelingsData relData))
			{
				if (relData.idToFeelings.ContainsKey(defender) && relData.idToFeelings[defender].inConflict)
				{
					SimulationEntity lostEntity = SimulationManagement.GetEntityByID(defender);

					if (lostEntity.GetData(DataTags.Territory, out TerritoryData lossData))
					{
                        lossData.RemoveTerritory(cellCenterOfPos);

						float modifier = 1.0f;

						//If this entity is at war with the winner add some additional
						//or the winner is at war with this entity
						//war exhaustion for losing a territory
						if (lostEntity.GetData(DataTags.War, out WarData warData))
						{
							bool applyWarExhaustion = false;

							//If at war with winner
							if (warData.atWarWith.Contains(winnerID))
							{
								applyWarExhaustion = true;
							}
							//Or winner is at war with us
							else if (wonEntity.GetData(DataTags.War, out WarData wonWarData) && wonWarData.atWarWith.Contains(defender))
							{
								applyWarExhaustion = true;

								//Make this faction now at war with winner
								warData.atWarWith.Add(winnerID);
							}

							if (applyWarExhaustion)
                            {
                                modifier = warData.warExhaustion;
                                warData.warExhaustion += 10.0f * warData.warExhaustionGrowthMultiplier;
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
						if (historyData != null)
						{
							if (!historyData.previouslyOwnedTerritories.ContainsKey(cellCenterOfPos))
							{
								historyData.previouslyOwnedTerritories.Add(cellCenterOfPos, new HistoryData.HistoryCell());
							}
							else
							{
								Debug.LogError("Battle fighting over unowned territory?");
							}
						}
					}

					//Destroy any settlement in this area
					if (lostEntity.GetData(DataTags.Settlement, out SettlementData setData))
					{
						setData.settlements.Remove(cellCenterOfPos);
					}
				}
			}
		}

		public Battle(RealSpacePostion pos)
		{
			postion = pos;
		}

		public override RealSpacePostion GetPosition()
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

		public override string GetDescription()
		{
			string descString = "";

			for (int i = 0; i < involvedEntities.Count; i++)
			{
				string color = "#ffffff";

				if (SimulationManagement.GetEntityByID(involvedEntities[i]).GetData(DataTags.Emblem, out EmblemData emblemData))
				{
					color = emblemData.mainColourHex;
				}

				descString += $"<color={color}>{Mathf.RoundToInt(100.0f * (involvedEntitiesProgress[i] / winLimit))}%</color>\n";
			}

			return descString;
		}

		public override float GetEntryOffset()
		{
			return 100.0f;
		}
	}

	public Dictionary<RealSpacePostion, List<Battle>> cellCenterToBattles = new Dictionary<RealSpacePostion, List<Battle>>();
	public static int totalBattlesCount = 0;

	public bool StartOrJoinBattle(RealSpacePostion key, RealSpacePostion actualPos, int originID, int targetID, bool mergeToExisting)
	{
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
			if (mergeToExisting || storedBattle.postion.Equals(actualPos))
			{
				battle = storedBattle;

				if (mergeToExisting)
				{
					actualPos = storedBattle.postion;
				}

				break;
			}
		}

		if (battle == null)
		{
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
}
