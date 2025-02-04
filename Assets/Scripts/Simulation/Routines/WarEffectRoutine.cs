using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-50)]
public class WarEffectRoutine : RoutineBase
{
	public override void Run()
	{
		List<DataModule> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

		foreach (MilitaryData militaryData in militaryDatas.Cast<MilitaryData>())
		{
			if (militaryData.TryGetLinkedData(DataTags.War, out WarData warData))
			{
				if (warData.atWarWith.Count == 0 || (warData.TryGetLinkedData(DataTags.Strategy, out StrategyData strat) && strat.globalStrategy == StrategyData.GlobalStrategy.Defensive))
				{
					warData.warExhaustion = Mathf.Max(0, warData.warExhaustion - (1f / (warData.warExhaustionGrowthMultiplier * 20.0f)));
				}
				else
				{
					warData.warExhaustion += militaryData.totalDamageBuildup * warData.warExhaustionGrowthMultiplier;
				}
            }
		}
	}
}
