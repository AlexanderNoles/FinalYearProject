using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateCrew : Faction
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(Tags.HasMilitary);
		AddTag(Tags.Insignificant);
		AddTag(Tags.Capital);
	}

	public override void InitData()
	{
		base.InitData();
		AddData(Tags.HasMilitary, new MilitaryData());
		AddData(Tags.Capital, new CapitalData("Pirate Crew", "", Color.red));
	}
}
