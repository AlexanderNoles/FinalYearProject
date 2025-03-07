using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-110)]
public class PoliticalChangeRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all political data
		List<DataModule> politicalDatas = SimulationManagement.GetDataViaTag(DataTags.Political);

		foreach (PoliticalData politicalData in politicalDatas.Cast<PoliticalData>())
		{
            //Set basic political instability
            politicalData.politicalInstability = 0.001f;

            //If this entity can fight wars
            //and has some war exhaustion
            //Increase chance for large political shift
            if (politicalData.TryGetLinkedData(DataTags.Strategy, out StrategyData strategyData) && strategyData is WarStrategy)
            {
                politicalData.politicalInstability *= Mathf.Max(1, (strategyData as WarStrategy).warExhaustion);
            }

            //Cache change multiplier for more readable code
            float changeMultiplier = politicalData.politicalInstability;

            if (SimulationManagement.random.Next(-10000, 10001) / 10000.0f < changeMultiplier)
            {
                //Low chance for larger change
                changeMultiplier *= 2.0f;
            }

            //Apply randomized change
            politicalData.authorityAxis += changeMultiplier * (SimulationManagement.random.Next(-100, 101) / 100.0f);
            politicalData.authorityAxis = Mathf.Clamp(politicalData.authorityAxis, -1.0f, 1.0f);

            politicalData.economicAxis += changeMultiplier * (SimulationManagement.random.Next(-100, 101) / 100.0f);
            politicalData.economicAxis = Mathf.Clamp(politicalData.economicAxis, -1.0f, 1.0f);
        }
	}
}
