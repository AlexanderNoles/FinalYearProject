using MonitorBreak.Bebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBattleData : DataBase
{
	public class Battle
	{
		public List<int> involvedFactions = new List<int>();
	}

	public Dictionary<RealSpacePostion, Battle> battles = new Dictionary<RealSpacePostion, Battle>();

	public bool StartBattle(RealSpacePostion pos, int originID, int targetID)
	{
		Console.Log(battles.Count);

		if (!battles.ContainsKey(pos))
		{
			battles[pos] = new Battle();
		}

		Battle battle = battles[pos];

		if (!battle.involvedFactions.Contains(originID))
		{
			battle.involvedFactions.Add(originID);
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
