using System;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;

[SimulationManagement.ActiveSimulationRoutine(90, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class CapitalInitRoutine : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
		return tags.Contains(DataTags.Capital);
    }

    public override void Run()
	{
		List<DataBase> toInit = SimulationManagement.GetToInitData(DataTags.Capital);

		foreach (CapitalData capitalData in toInit.Cast<CapitalData>())
		{
            if (capitalData.position == null)
            {
                //Get position
                if (capitalData.TryGetLinkedData(DataTags.Territory, out TerritoryData territoryData))
                {
                    //Get oldest territory
                    if (territoryData.territoryCenters.Count > 0)
                    {
                        capitalData.position = territoryData.territoryCenters.ElementAt(0);
                    }
                }

                //If no other capital position was assigned
                if (capitalData.position == null)
                {
                    //If this faction does not own territory then just place it in a random spot
                    capitalData.position = WorldManagement.RandomCellCenterWithinSolarSystem();
                }
            }
        }
	}
}