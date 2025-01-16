using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(150, SimulationManagement.SimulationRoutine.RoutineTypes.Absent)]
public class WarStratergyRoutine : RoutineBase
{
    public override void Run()
    {
        //Get all war datas
        List<DataBase> warDatas = SimulationManagement.GetDataViaTag(DataTags.War);

        foreach (WarData data in warDatas.Cast<WarData>())
        {
            if (SimulationManagement.random.Next(0, 100) < data.warExhaustion * data.defensivePropensity)
            {
                //Switch to defense stratergy
                data.globalStratergy = WarData.GlobalStratergy.Defensive;
            }
            else
            {
                data.globalStratergy = WarData.GlobalStratergy.Aggresive;
            }
        }
    }
}
