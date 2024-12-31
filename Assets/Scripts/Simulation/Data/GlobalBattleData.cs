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
				if (involvedEntitiesProgress[i] > 1.2f)
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
					if (!data.ongoingBattles.Remove(pos))
					{
						Debug.LogWarning("Entity was unaware of battle it was in!");
					}
				}
			}

			involvedEntities.Clear();
		}

		public void ResolveTerritory(RealSpacePostion pos, HistoryData historyData, int winnerID, bool isLazy = true)
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
			if (SimulationManagement.GetEntityByID(winnerID).GetData(DataTags.Feelings, out FeelingsData relData))
			{
				if (relData.idToFeelings.ContainsKey(defender) && relData.idToFeelings[defender].inConflict)
				{
					SimulationEntity lostEntity = SimulationManagement.GetEntityByID(defender);

					if (lostEntity.GetData(DataTags.Territory, out TerritoryData lossData))
					{
						lossData.RemoveTerritory(pos);

						float modifier = 1.0f;

						//If this entity is at war with the winner add some additional
						//war exhaustion for losing a territory
						if (lostEntity.GetData(DataTags.War, out WarData warData))
						{
							if (warData.atWarWith.Contains(winnerID))
							{
								modifier = warData.warExhaustion;
								warData.warExhaustion += 10.0f * warData.warExhaustionGrowthMultiplier;
							}
						}

						//Apply territory cap loss
						//This is too ensure entites don't just get stuck in eternal wars
						//where they keep claiming
						lossData.territoryClaimUpperLimit -= 2f * modifier;

						//Transfer previously owned faction to history data
						if (historyData != null)
						{
							if (!historyData.previouslyOwnedTerritories.ContainsKey(pos))
							{
								historyData.previouslyOwnedTerritories.Add(pos, new HistoryData.HistoryCell());
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
						setData.settlements.Remove(pos);
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
	}

	public Dictionary<RealSpacePostion, Battle> battles = new Dictionary<RealSpacePostion, Battle>();
	public static int totalBattlesCount = 0;

	public bool StartOrJoinBattle(RealSpacePostion key, RealSpacePostion actualPos, int originID, int targetID)
	{
		if (!battles.ContainsKey(key))
		{
			battles[key] = new Battle(actualPos);
			battles[key].startTickID = SimulationManagement.currentTickID;
			totalBattlesCount++;
		}

		Battle battle = battles[key];
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
				if (!attackBData.ongoingBattles.ContainsKey(key))
				{
					//This check should always pass
					attackBData.ongoingBattles.Add(key, new BattleData.BattleReference());
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
					if (!defendBData.ongoingBattles.ContainsKey(key))
					{
						//Not currently considering defending this spot already
						//Add it
						defendBData.ongoingBattles.Add(key, new BattleData.BattleReference());
					}
				}
			}

			return true;
		}

		return false;
	}
}
