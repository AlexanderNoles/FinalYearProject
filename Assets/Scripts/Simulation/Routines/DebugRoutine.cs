using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.SimulationRoutine(0, SimulationManagement.SimulationRoutine.RoutineTypes.Absent)]
public class DebugRoutine : RoutineBase
{
	public override void Run()
	{
		MonitorBreak.Bebug.Console.Log(SimulationManagement.GetEntityCount(EntityTypeTags.MineralDeposit));
	}
}
