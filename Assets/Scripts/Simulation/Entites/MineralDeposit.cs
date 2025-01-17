using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralDeposit : SimulationEntity
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(EntityStateTags.Insignificant);
	}

	public override void InitData()
	{
		base.InitData();
		//Allows the mineral depoist to pick fights
		//It has no military so it cannot fight back
		AddData(DataTags.Battle, new BattleData());
		AddData(DataTags.TargetableLocation, new TargetableLocationData("Ore Deposit", "", Color.green));
	}
}
