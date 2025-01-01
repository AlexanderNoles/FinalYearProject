using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0)]
public class MilitaryCapacityFromPopulationRoutine : RoutineBase
{
    public override void Run()
    {
        List<DataBase> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

        foreach (MilitaryData militaryData in militaryDatas.Cast<MilitaryData>())
        {
            if (militaryData.TryGetLinkedData(DataTags.Population, out PopulationData populationData))
            {
                militaryData.maxMilitaryCapacity = MathHelper.ValueTanhFalloff(populationData.currentPopulationCount, 1000, 9000);
            }
        }
    }
}