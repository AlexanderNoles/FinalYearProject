using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InformationDatabase
{
	public static string GetPopulationCapInfoString()
	{
		return $"<color={VisualDatabase.statisticColour}>Population Cap</color> controls your max population.";
	}

	public static string GetPopulationInfoString()
	{
		return $"<color={VisualDatabase.statisticColour}>Population</color> acts as the underlying foundation of your faction. " +
			$"Without <color={VisualDatabase.statisticColour}>Population</color> your faction cannot function properly, reaching 0 <color={VisualDatabase.statisticColour}>Population</color> will result in a game over. \n" +
			$"Certain <color={VisualDatabase.statisticColour}>Population</color> thresholds gatekeep other upgrades but total <color={VisualDatabase.statisticColour}>Population</color> value controls the chance for certain useful events to occur.";
	}
}
