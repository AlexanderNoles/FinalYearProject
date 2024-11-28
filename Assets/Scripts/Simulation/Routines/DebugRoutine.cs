using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(-4000, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Debug)]
public class DebugRoutine : RoutineBase
{
    public override void Run()
	{
		GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
		gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

		Debug.Log(globalBattleData.battles.Count);
	}
}
