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
		AddData(DataTags.Emblem, new EmblemData());
		AddData(DataTags.TargetableLocation, new TargetableLocationData("Pirate Crew", "", Color.red));
	}
}
