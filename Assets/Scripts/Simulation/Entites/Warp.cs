using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : SimulationEntity
{
	public static Warp main;

	public override void Simulate()
	{
		base.Simulate();
		main = this;
	}

	public override void InitTags()
	{
		base.InitTags();

		AddTag(EntityStateTags.Unkillable);
	}

	public override void InitData()
	{
		base.InitData();
		
		Shop warpShop = new Shop();
		warpShop.capacity = 1;
		AddData(DataTags.CentralShop, warpShop);
	}
}
