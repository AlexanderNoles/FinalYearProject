using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(SimulationManagement.evaluationRoutineStandardPrio)]
public class WarEvaluationRoutine : RoutineBase
{
	public override void Run()
	{
		//For every entity that can fight wars, evaluate whether those wars should be over
		//If they should be then end them

		List<DataBase> warDatas = SimulationManagement.GetDataViaTag(DataTags.War);

		foreach (WarData warData in warDatas.Cast<WarData>())
		{
            //Iterate through each war
            for (int i = 0; i < warData.atWarWith.Count;)
            {
                int enemyID = warData.atWarWith[i];

                //Currently all wars are ones of complete eradication
                //Meaning a war can only end if the entity is gone

                //If enemy is gone
                //This is an evaluation routine so it runs after the MetaRoutine (which handles the final removal of factions)
                //This means we can just check if a entity was removed this tick
                if (!SimulationManagement.EntityWithIDExists(enemyID))
                {
                    warData.atWarWith.Remove(enemyID);
                }
                else
                {
                    i++;
                }
            }
        }
	}
}
