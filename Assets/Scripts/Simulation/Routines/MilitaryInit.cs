using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-100, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class MilitaryInit : InitRoutineBase
{
	public override bool IsDataToInit(HashSet<Enum> tags)
	{
		return tags.Contains(DataTags.Military);
	}

	public override void Run()
	{
		List<DataModule> militaries = SimulationManagement.GetToInitData(DataTags.Military);

		foreach (MilitaryData module in militaries.Cast<MilitaryData>())
		{
			RealSpacePosition initalPos = null;

			if (module.TryGetLinkedData(DataTags.TargetableLocation, out TargetableLocationData target))
			{
				initalPos = target.actualPosition;
			}

			if (initalPos != null)
			{
				module.origin = initalPos;

				for (int i = 0; i < module.initalCount; i++)
				{
					ShipCollection newFleet = module.GetNewFleet();
					newFleet.Fill(module.parent);
					module.AddFleet(initalPos, newFleet);
				}
			}
		}
	}
}
