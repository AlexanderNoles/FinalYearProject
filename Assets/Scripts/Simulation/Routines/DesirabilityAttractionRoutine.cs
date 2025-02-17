using EntityAndDataDescriptor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(40)]
public class DesirabilityAttractionRoutine : RoutineBase
{
	//Each tick this routine will cause targetable locations to try and pick fights
	//with other entites
	//This is based on the simulation principle of "entities working in union for the benefit of the player"
	//it doesn't make a lot of logical sense for a mineral depoist to be like "hey come kill me" but it does make
	//sense that if we think of entites as trying to create a believable world for the player

	public override void Run()
	{
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);
		List<DataModule> desirabilityModules = SimulationManagement.GetDataViaTag(DataTags.Desirability);
		List<TerritoryData> territoryDatas = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();
		List<DataModule> militaries = SimulationManagement.GetDataViaTag(DataTags.Military);

		foreach (DesirabilityData data in desirabilityModules.Cast<DesirabilityData>())
		{
			//Only run every 10 ticks
			if (SimulationManagement.currentTickID < data.lastTickTime + 10 || data.parent.Get().HasTag(EntityStateTags.Dead))
			{
				continue;
			}

			//Update desirability
			data.UpdateDesirability();
			data.lastTickTime = SimulationManagement.currentTickID;

			//Before anything we need to check whether this location is going to even try to pick a fight this tick
			//This is meant to represent the willgness by another entity to come claim this one
			//This can be based on certain things but the key assumption should be made clear
			//"All entites want this entity the same amount", this can be adjusted in the future (by getting the target first) if need be
			if (SimulationManagement.random.Next(0, 101) > data.desirability)
			{
				continue;
			}

			SimulationEntity target = null;
			//Find simulation entites that might want to attack this one
			//
			//First check if we are in a territory data;
			foreach (TerritoryData territory in territoryDatas)
			{
				if (territory.Contains(data.GetCellCenter()))
				{
					//We've found one!
					target = territory.parent.Get();
					break;
				}
			}

			if (target == null)
			{
				//Not within territory
				//Pick random military
				int loopClamp = 100;

				do 
				{
					loopClamp--;

					MilitaryData targetMilData = militaries[SimulationManagement.random.Next(0, militaries.Count)] as MilitaryData;

					//Don't want to force a self controlled military to attack us
					if (!targetMilData.selfControlled)
					{
						target = targetMilData.parent.Get();
					}
				}
				while (loopClamp > 0 && target == null);
			}

			//Final null check
			//Start a battle with target
			if (target != null)
			{
				globalBattleData.StartOrJoinBattle(data.GetCellCenter(), data.GetActualPosition(), target.id, data.parent.Get().id, false);
			}
		}
	}
}