using System.Collections.Generic;
using System.Linq;

[SimulationManagement.ActiveSimulationRoutine(90, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class CapitalInitRoutine : InitRoutineBase
{
	public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
	{
		return tags.Contains(Faction.Tags.Capital);
	}

	public override void Run()
	{
		//Get all capital factions
		List<Faction> capitalHavingFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Capital);

		foreach (Faction faction in capitalHavingFactions)
		{
			if (faction.GetData(Faction.Tags.Capital, out CapitalData capitalData))
			{
				if (capitalData.position == null)
				{
					//Get position
					if (faction.GetData(Faction.Tags.Territory, out TerritoryData territoryData))
					{
						//Get oldest territory
						if (territoryData.territoryCenters.Count > 0)
						{
							capitalData.position = territoryData.territoryCenters.ElementAt(0);
						}
					}
					else
					{
						//If this faction does not own territory then just place it in a random spot
						capitalData.position = WorldManagement.RandomCellCenterWithinSolarSystem();
					}
				}
			}
		}
	}
}