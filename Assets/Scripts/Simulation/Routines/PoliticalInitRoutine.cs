using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(110, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class PoliticalInitRoutine : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
        return tags.Contains(DataTags.Political);
    }

	public override void Run()
	{
		//Get all political data modules
		List<DataBase> politicalDatas = SimulationManagement.GetToInitData(DataTags.Political);

		foreach (PoliticalData politicalData in politicalDatas.Cast<PoliticalData>())
		{
            //Give randomized political leaning
            politicalData.economicAxis = SimulationManagement.random.Next(-100, 101) / 100.0f;
            politicalData.authorityAxis = SimulationManagement.random.Next(-100, 101) / 100.0f;
        }
	}
}
