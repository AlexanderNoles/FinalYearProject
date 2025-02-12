using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-3000)]
public class MetaRoutine : RoutineBase
{
    public override void Run()
    {
        List<SimulationEntity> deadEntities = SimulationManagement.GetEntitiesViaTag(EntityStateTags.Dead);
        List<(int, SimulationEntity)> removedEntites = new List<(int, SimulationEntity)>();

        for (int i = 0; i < deadEntities.Count;)
        {
            SimulationEntity deadEntity = deadEntities[0];

            if (!deadEntity.HasTag(EntityStateTags.Unkillable))
            {
				//Remove this entity from the simulation
                SimulationManagement.RemoveEntityFromSimulation(deadEntity);
                removedEntites.Add((deadEntity.id, deadEntity));
                //Perform nothing else for this entity as it is now dead (including incrementing the index)
                continue;
            }
            else
            {
                deadEntity.RemoveTag(EntityStateTags.Dead);
                //Remove dead state from this entity or it will continue to try and be killed each tick
                //Because this removes it from the list we can't increment the index
                continue;
            }
        }

        List<DataModule> allFeelingsData = SimulationManagement.GetDataViaTag(DataTags.Feelings);

        //This should arguably be conjoined with the evaluation routine code
        //So cleanup can be implemented in a modularised fashion
        //Perhaps a new specific type of routine?
        if (removedEntites.Count > 0)
        {
            //Removed entities this tick

            //Do cleanup
            //This cleanup violates the modular principles in some sense
            //But the nature of feelings data requires this data to be removed (otherwise it would just sit there taking up space)
            foreach (FeelingsData feelingsData in allFeelingsData.Cast<FeelingsData>())
            {
                foreach ((int, SimulationEntity) entry in removedEntites)
                {
                    feelingsData.idToFeelings.Remove(entry.Item1);
                }
            }

            GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);

            //Remove from battles if they were partaking
            foreach (KeyValuePair<RealSpacePosition, List<GlobalBattleData.Battle>> battleEntry in globalBattleData.cellCenterToBattles)
            {
                foreach ((int, SimulationEntity) entry in removedEntites)
                {
					//We should have battle data that tells us what battle we are in?
					//Why is this done this way instead past me?
					foreach (GlobalBattleData.Battle battle in battleEntry.Value) 
					{
						battle.RemoveInvolvedEntity(entry.Item1);
					}
                }
            }

			removedEntites.Clear();
        }
    }
}
