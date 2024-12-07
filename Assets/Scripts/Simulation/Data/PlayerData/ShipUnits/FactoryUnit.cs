using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryUnit : PlayerShipUnitBase, IDisplay
{
	public Sprite GetBackingImage()
	{
		return null;
	}

	public string GetDescription()
	{
		return $"Increases <color={VisualDatabase.statisticColour}>Production</color> by <color={VisualDatabase.goodColourString}>50</color>.";
	}

	public string GetExtraInformation()
	{
		return "";
	}

	public string GetTitle()
	{
		return "Factory Unit";
	}
}
