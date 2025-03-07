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

	public override void InitEntityTags()
	{
		base.InitEntityTags();

		AddTag(EntityStateTags.Unkillable);
	}

	public override void InitData()
	{
		base.InitData();

		//Make this an exalted ItemShop with 1 capacity
		Shop warpShop = new WarpShop
		{
			type = Shop.ShopType.ItemShop,
			capacity = 1
		};
		warpShop.SetTargetRarity(ItemDatabase.ItemRarity.Exalted);
		AddData(DataTags.CentralShop, warpShop);
	}
}
