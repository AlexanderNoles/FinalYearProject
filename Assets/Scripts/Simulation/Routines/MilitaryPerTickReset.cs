using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(200)]
public class MilitaryPerTickReset : RoutineBase
{
	public override void Run()
	{
		//Get all military data
		List<DataBase> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

		foreach (MilitaryData militaryData in militaryDatas.Cast<MilitaryData>())
		{
            militaryData.toTransfer.Clear();
			militaryData.fromTransfer.Clear();

            militaryData.totalDamageBuildup = 0.0f;
        }
	}
}
