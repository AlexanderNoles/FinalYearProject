using System;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(90, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class TargetableLocationInit : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
		return tags.Contains(DataTags.TargetableLocation);
    }

    public override void Run()
	{
		List<DataBase> toInit = SimulationManagement.GetToInitData(DataTags.TargetableLocation);

		foreach (TargetableLocationData targetableLocation in toInit.Cast<TargetableLocationData>())
		{
            if (targetableLocation.cellCenter == null)
            {
                //Get position
                if (targetableLocation.TryGetLinkedData(DataTags.Territory, out TerritoryData territoryData))
                {
                    //Get oldest territory
                    if (territoryData.territoryCenters.Count > 0)
                    {
                        targetableLocation.cellCenter = territoryData.territoryCenters.ElementAt(0);
                    }
                }

                //If no other capital position was assigned
                if (targetableLocation.cellCenter == null)
                {
                    //If this faction does not own territory then just place it in a random spot
                    targetableLocation.cellCenter = WorldManagement.RandomCellCenterWithinSolarSystem();
                }

				//Move on map position to be off grid
				targetableLocation.actualPosition = WorldManagement.RandomPositionInCell(targetableLocation.cellCenter, SimulationManagement.random);
			}
        }
	}
}