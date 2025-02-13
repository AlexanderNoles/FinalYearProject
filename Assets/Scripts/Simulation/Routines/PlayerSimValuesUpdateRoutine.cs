using EntityAndDataDescriptor;
using System.Collections.Generic;
using System.Linq;

[SimulationManagement.SimulationRoutine(250)]
public class PlayerSimValuesUpdateRoutine : RoutineBase
{
	public override void Run()
	{
		List<DataModule> playerStats = SimulationManagement.GetDataViaTag(DataTags.Stats);

		foreach (PlayerStats playerStatsEntry in playerStats.Cast<PlayerStats>())
		{
			if (playerStatsEntry.TryGetLinkedData(DataTags.Population, out PopulationData popData))
			{
				popData.populationNaturalGrowthLimt = playerStatsEntry.GetStat(Stats.populationCap.ToString());
			}
		}
	}
}