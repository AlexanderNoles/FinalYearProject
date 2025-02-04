using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(150, SimulationManagement.SimulationRoutine.RoutineTypes.Absent)]
public class StrategyRoutine : RoutineBase
{
    public override void Run()
    {
        //Get all war datas
        List<DataModule> stratDatas = SimulationManagement.GetDataViaTag(DataTags.War);

        foreach (StrategyData data in stratDatas.Cast<StrategyData>())
        {
			float warExhaustion = 0;
			if (data.TryGetLinkedData(DataTags.War, out WarData warData))
			{
				warExhaustion = warData.warExhaustion;
			}

            if (SimulationManagement.random.Next(0, 100) < warExhaustion * data.defensivePropensity)
            {
                //Switch to defense stratergy
                data.globalStrategy = StrategyData.GlobalStrategy.Defensive;
            }
            else
            {
                data.globalStrategy = StrategyData.GlobalStrategy.Aggresive;
            }
        }
    }
}
