using MonitorBreak.Bebug;
using System.Collections.Generic;

[SimulationManagement.ActiveSimulationRoutine(2900)]
public class PlayerPopulationRoutine : RoutineBase
{
	public override void Run()
	{
		List<Faction> playerFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		foreach (Faction faction in playerFactions)
		{
			//Get general ship data
			faction.GetData(PlayerFaction.shipDataKey, out PlayerShipData shipData);

			if (faction.GetData(Faction.Tags.Population, out PopulationData populationData))
			{
				//Static cap for now
				//Should grow based on number of population units
				populationData.populationNaturalGrowthLimt = 300;

				//Growth rate changes based on the nature of the ship
				populationData.populationNaturalGrowthSpeed = (shipData.amenities * 0.3f) * (populationData.currentPopulationCount * 0.001f);

				//Currently death speed is just proportional to nautral growth speed, in the future
				//this could be replaced with a system that factors in poor working conditions, etc.
				populationData.populationNaturalDeathSpeed = populationData.populationNaturalGrowthSpeed / 2.0f;
			}
		}
	}
}