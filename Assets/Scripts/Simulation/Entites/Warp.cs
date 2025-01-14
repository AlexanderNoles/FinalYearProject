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
		
		Shop warpShop = new WarpShop();
		warpShop.capacity = 1;
		warpShop.SetTargetRarity(ItemDatabase.ItemRarity.Exalted);
		AddData(DataTags.CentralShop, warpShop);
	}
}
