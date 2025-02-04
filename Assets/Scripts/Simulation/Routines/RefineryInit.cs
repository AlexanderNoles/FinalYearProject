using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(85, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class RefineryInit : InitRoutineBase
{
	public override bool IsDataToInit(HashSet<Enum> tags)
	{
		return tags.Contains(DataTags.Refinery);
	}

	public override void Run()
	{
		//Get refinieries init this tick
		List<DataModule> refinieries = SimulationManagement.GetToInitData(DataTags.Refinery);

		foreach (RefineryData module in refinieries.Cast<RefineryData>())
		{
			if (module.TryGetLinkedData(DataTags.TargetableLocation, out TargetableLocationData targetable))
			{
				module.refineryPosition = targetable.actualPosition;
			}
		}
	}
}
