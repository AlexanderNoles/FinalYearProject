using System.Collections.Generic;

[SimulationManagement.ActiveSimulationRoutine(2900, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class PlayerInitRoutine : InitRoutineBase
{
	public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
	{
		return tags.Contains(Faction.Tags.Player);
	}

	public override void Run()
	{
		//Get all players
		List<Faction> playerFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		foreach (Faction faction in playerFactions)
		{
			//Set initial population
			if (faction.GetData(Faction.Tags.Population, out PopulationData popData))
			{
				popData.currentPopulationCount = SimulationManagement.random.Next(40, 50);
			}
		}
	}
}