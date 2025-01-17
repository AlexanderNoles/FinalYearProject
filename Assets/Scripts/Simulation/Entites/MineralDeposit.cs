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
		TargetableLocationData targetableLocationData = new TargetableLocationData("Ore Deposit", "", Color.green);

		//give this location a random desirability
		float t = SimulationManagement.random.Next(0, 101) / 100.0f;
		targetableLocationData.desirability = Mathf.CeilToInt(Mathf.Lerp(1, 31, Mathf.Pow(t, 3)));

		AddData(DataTags.TargetableLocation, targetableLocationData);
	}
}
