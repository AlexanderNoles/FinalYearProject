using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSimObjectBehaviour : SimObjectBehaviour
{
	const float battleRange = 2000.0f;

	//This script should verify each frame if any opposition exists within the battle
	//a.k.a did BattleManagement register any opposition

	//If not then end the battle, this essentially means we have to record found targets and their position and then after wards run a function in this script
	//That means we need to register to an event

	protected override void OnEnable()
	{
		base.OnEnable();

		BattleManagement.postGlobalRefresh.AddListener(EvaluateBattle);
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		BattleManagement.postGlobalRefresh.RemoveListener(EvaluateBattle);
	}

	private void EvaluateBattle()
	{
		//Check if any bbs found targets within battle range
		//If not then verify who is still here to pick the winner

		foreach (Vector3 recordedPos in BattleManagement.refreshStats.positionsOfBBsThatFoundTargets)
		{
			if (Vector3.Distance(transform.position, recordedPos) < battleRange)
			{
				//Nothing more to do as battle is ongoing
				return;
			}
		}

		//Did not return so we need to find winner! (and end)

		//Get counts of remaining bbs and find the entity with the most
		//That is the winner
		int winnerID = -1;
		int currentHighestCount = int.MinValue;

		foreach (KeyValuePair<int, int> entry in BattleManagement.refreshStats.idToCount)
		{
			if (entry.Key != -1) //Not unassigned
			{
				if (entry.Value > currentHighestCount)
				{
					currentHighestCount = entry.Value;
					winnerID = entry.Key;
				}
			}
		}

		//End battle
		//This should always work as this is a battle simobject behaviour
		//if it doesn't then the error tells us something is being missassigned!
		GlobalBattleData.Battle battle = target as GlobalBattleData.Battle;

		//Need to get a couple things:
		//Position
		//Cellcenter
		//History Data (to record lost territory as previously owned)
		//Global Battle Data (to remove the battle from)
		//Winner (already calculated)

		RealSpacePosition position = battle.GetPosition();
		RealSpacePosition cellCenter = WorldManagement.ClampPositionToGrid(position);
		GameWorld.main.GetData(DataTags.Historical, out HistoryData historyData);
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattle);

		//If winner id is -1 still, end function will handle it
		battle.End(position, cellCenter, historyData, winnerID);

		//Now we need to remove the battle from the global battle data
		//This is not handled automatically as the BattleResolutionRoutine needs to maintain the consistency of the data it is operating on
		//(so it will remove the battles all at once at the end)
		globalBattle.RemoveBattle(cellCenter, battle);
	}
}
