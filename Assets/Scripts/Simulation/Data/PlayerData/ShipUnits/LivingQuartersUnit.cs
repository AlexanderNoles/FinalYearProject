using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingQuartersUnit : PlayerShipUnitBase, IDisplay
{
	public Sprite GetBackingImage()
	{
		return null;
	}

	public string GetDescription()
	{
		return $"Increases <color={VisualDatabase.statisticColour}>Population Cap</color> by <color={VisualDatabase.goodColourString}>300</color>.";
	}

	public string GetTitle()
	{
		return "Living Quarters Unit";
	}

	public string GetExtraInformation()
	{
		return InformationDatabase.GetPopulationCapInfoString() + "\n" + InformationDatabase.GetPopulationInfoString();
	}
}
