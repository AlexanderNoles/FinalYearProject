using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PirateCrew : Faction
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(EntityStateTags.Insignificant);
	}

	public override void InitData()
	{
		base.InitData();
		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.Capital, new CapitalData("Pirate Crew", "", Color.red));
	}
}
