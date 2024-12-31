using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.ActiveSimulationRoutine(200)]
public class MilitaryPerTickReset : RoutineBase
{
	public override void Run()
	{
		//Get all military data
		List<DataBase> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

		foreach (MilitaryData militaryData in militaryDatas.Cast<MilitaryData>())
		{
            militaryData.markedTransfers.Clear();
            militaryData.totalDamageBuildup = 0.0f;
        }
	}
}
