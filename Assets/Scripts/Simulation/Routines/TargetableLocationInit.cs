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
            if (targetableLocation.position == null)
            {
                //Get position
                if (targetableLocation.TryGetLinkedData(DataTags.Territory, out TerritoryData territoryData))
                {
                    //Get oldest territory
                    if (territoryData.territoryCenters.Count > 0)
                    {
                        targetableLocation.position = territoryData.territoryCenters.ElementAt(0);
                    }
                }

                //If no other capital position was assigned
                if (targetableLocation.position == null)
                {
                    //If this faction does not own territory then just place it in a random spot
                    targetableLocation.position = WorldManagement.RandomCellCenterWithinSolarSystem();
                }

				//Move on map position to be off grid
				targetableLocation.location.actualPos = WorldManagement.RandomPositionInCell(targetableLocation.position, SimulationManagement.random);
			}
        }
	}
}