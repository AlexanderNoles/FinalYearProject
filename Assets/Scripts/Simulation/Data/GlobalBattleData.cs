using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBattleData : DataBase
{
	public class Battle : VisitableLocation
	{
		public int firstAttacker = -1;
		public int defender = -1;

		public List<int> involvedFactions = new List<int>();

		public bool BattleWon()
		{
			//Are any factions still in conflict with each other?
			//If so battle has not been won

			foreach (int id in involvedFactions)
			{
				if (SimulationManagement.GetFactionByID(id).GetData(Faction.relationshipDataKey, out RelationshipData data))
				{
					foreach (int otherId in involvedFactions)
					{
						if (data.idToRelationship.ContainsKey(otherId))
						{
							if (data.idToRelationship[otherId].conflict > 0)
							{
								//Still in conflict
								return false;
							}
						}
					}
				}
			}


			return true;
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

		public void ResolveTerritoryTransfer(RealSpacePostion pos, bool isLazy = true)
		{
			// Decide new owner //

			//If defender is still in battle do nothing, territory stays with them
			if (involvedFactions.Contains(defender))
			{
				return;
			}

			//If primary (original) attacker is still in battle territory goes to them

			int receiver = -1;
			if (involvedFactions.Contains(firstAttacker))
			{
				receiver = firstAttacker;
			}
			else if(involvedFactions.Count > 0)
			{
				//Otherwise pick randomly from remaing factions (if any exist)
				if (isLazy)
				{
					receiver = involvedFactions[SimulationManagement.random.Next(0, involvedFactions.Count)];
				}
				else
				{
					receiver = involvedFactions[Random.Range(0, involvedFactions.Count)];
				}
			}

			// Apply transfer //

			if (receiver != -1)
			{
				//If this check fails then battle was a complete draw
				//That would be considered an exact defense so no transfer would take place eithier

				//If the factions don't have territory (for example if they are some kind of huge beast or something) then they simply won't gain or lose territory through this process

				//Disabled territory gain, simply loss
				//Faction gainFaction = SimulationManagement.GetFactionByID(receiver);

				//if (gainFaction.GetData(Faction.Tags.Territory, out TerritoryData gainData))
				//{
				//	if (!gainData.territoryCenters.Contains(pos))
				//	{
				//		gainData.AddTerritory(pos);
				//	}
				//}

				Faction lossFaction = SimulationManagement.GetFactionByID(defender);

				if (lossFaction.GetData(Faction.Tags.Territory, out TerritoryData lossData))
				{
					lossData.RemoveTerritory(pos);

					lossData.territoryClaimUpperLimit -= 1.1f;
				}

				//Destroy any settlement in this area
				if (lossFaction.GetData(Faction.Tags.Settlements, out SettlementData setData))
				{
					setData.settlements.Remove(pos);
				}
			}
		}
	}

	public Dictionary<RealSpacePostion, Battle> battles = new Dictionary<RealSpacePostion, Battle>();
	public static int totalBattlesCount = 0;

	public bool StartBattle(RealSpacePostion pos, int originID, int targetID)
	{
		if (!battles.ContainsKey(pos))
		{
			battles[pos] = new Battle();
			totalBattlesCount++;
		}

		Battle battle = battles[pos];

		if (!battle.involvedFactions.Contains(originID))
		{
			battle.involvedFactions.Add(originID);

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
			if (!battle.involvedFactions.Contains(targetID))
			{
				battle.involvedFactions.Add(targetID);

				if (battle.defender == -1)
				{
					//Set this faction as the one to claim
					battle.defender = targetID;
				}

				//Get target pending defence data and add this battle as a new entry
				Faction defender = SimulationManagement.GetFactionByID(targetID);

				if (defender.GetData(Faction.battleDataKey, out BattleData defendBData))
				{
					if (!defendBData.pendingDefences.ContainsKey(pos))
					{
						//Not currently considering defending this spot already
						//Add it
						defendBData.pendingDefences.Add(pos, new BattleData.PendingDefence());
					}
				}
			}

			return true;
		}

		return false;
	}
}
