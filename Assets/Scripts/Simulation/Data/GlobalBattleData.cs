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

		private List<int> involvedFactions = new List<int>();
		private List<float> involvedFactionsProgress = new List<float>();

		public List<int> GetInvolvedFactions()
		{
			return involvedFactions;
		}

		public void AddInvolvedFaction(int id)
		{
			involvedFactions.Add(id);
			involvedFactionsProgress.Add(0.0f);
		}

		public void RemoveInvolvedFaction(int id)
		{
			int indexOf = involvedFactions.IndexOf(id);

			if (indexOf == -1)
			{
				return;
			}

			involvedFactions.RemoveAt(indexOf);
			involvedFactionsProgress.RemoveAt(indexOf);
		}

		public void AddToWinProgress(int index, float amount)
		{
			involvedFactionsProgress[index] += amount;
		}

		public float GetWinProgress(int index)
		{
			return involvedFactionsProgress[index];
		}

		public bool BattleWon(out int winnerID)
		{
			winnerID = -1;

			if (involvedFactions.Count <= 0 || backgroundProgression >= 1.0f)
			{
				return true;
			}

			for (int i = 0; i < involvedFactions.Count; i++)
			{
				if (involvedFactionsProgress[i] > 1.2f)
				{
					winnerID = involvedFactions[i];
					return true;
				}
			}

			return false;
		}

		public void End(RealSpacePostion pos)
		{
			foreach (int id in involvedFactions)
			{
				Faction current = SimulationManagement.GetFactionByID(id);

				if (current.GetData(Faction.battleDataKey, out BattleData data))
				{
					if (!data.ongoingBattles.Remove(pos))
					{
						Debug.LogWarning("Faction was unaware of battle it was in!");
					}
				}
			}

			involvedFactions.Clear();
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
			if (SimulationManagement.GetFactionByID(winnerID).GetData(Faction.relationshipDataKey, out RelationshipData relData))
			{
				if (relData.idToRelationship.ContainsKey(defender) && relData.idToRelationship[defender].inConflict)
				{
					Faction lossFaction = SimulationManagement.GetFactionByID(defender);

					if (lossFaction.GetData(Faction.Tags.Territory, out TerritoryData lossData))
					{
						lossData.RemoveTerritory(pos);

						float modifier = 1.0f;

						if (lossFaction.GetData(Faction.Tags.CanFightWars, out WarData warData))
						{
							if (warData.atWarWith.Contains(winnerID))
							{
								modifier = warData.warExhaustion;
								warData.warExhaustion += 10.0f * warData.warExhaustionGrowthMultiplier;
							}
						}

						//Apply territory cap loss
						lossData.territoryClaimUpperLimit -= 2f * modifier;


						//Need to add the ability to transfer this to history data of some kind
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
					if (lossFaction.GetData(Faction.Tags.Settlements, out SettlementData setData))
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
	}

	public Dictionary<RealSpacePostion, Battle> battles = new Dictionary<RealSpacePostion, Battle>();
	public static int totalBattlesCount = 0;

	public bool StartBattle(RealSpacePostion pos, int originID, int targetID)
	{
		if (!battles.ContainsKey(pos))
		{
			battles[pos] = new Battle(pos);
			battles[pos].startTickID = SimulationManagement.currentTickID;
			totalBattlesCount++;
		}

		Battle battle = battles[pos];
		List<int> involvedFactions = battle.GetInvolvedFactions();

		if (!involvedFactions.Contains(originID))
		{
			battle.AddInvolvedFaction(originID);

			if (battle.firstAttacker == -1)
			{
				//Set this faction as the one to claim
				battle.firstAttacker = originID;
			}

			//Add new battle reference
			Faction attacker = SimulationManagement.GetFactionByID(originID);
			if (attacker.GetData(Faction.battleDataKey, out BattleData attackBData))
			{
				if (!attackBData.ongoingBattles.ContainsKey(pos))
				{
					//This check should always pass
					attackBData.ongoingBattles.Add(pos, new BattleData.BattleReference());
				}
				else
				{
					throw new System.Exception("major error: faction does not know it is in a battle it is in!");
				}
			}

			//Defending faction
			if (!involvedFactions.Contains(targetID))
			{
				battle.AddInvolvedFaction(targetID);

				if (battle.defender == -1)
				{
					//Set this faction as the one to claim
					battle.defender = targetID;
				}

				//Get target ongoing battle data and add this battle as a new entry
				Faction defender = SimulationManagement.GetFactionByID(targetID);

				if (defender.GetData(Faction.battleDataKey, out BattleData defendBData))
				{
					if (!defendBData.ongoingBattles.ContainsKey(pos))
					{
						//Not currently considering defending this spot already
						//Add it
						defendBData.ongoingBattles.Add(pos, new BattleData.BattleReference());
					}
				}
			}

			return true;
		}

		return false;
	}
}
