using EntityAndDataDescriptor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SimulationManagement.SimulationRoutine(10, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class EconomyInit : InitRoutineBase
{
	public override bool IsDataToInit(HashSet<Enum> tags)
	{
		return tags.Contains(DataTags.Economic);
	}

	public override void Run()
	{
		List<DataModule> economicDatas = SimulationManagement.GetToInitData(DataTags.Economic);

		foreach (EconomyData economyData in economicDatas.Cast<EconomyData>())
		{
			economyData.market = new Shop();
			economyData.market.type = Shop.ShopType.ItemShop;
			economyData.market.capacity = 1;
			economyData.market.SetTargetRarity(ItemDatabase.ItemRarity.Rare);
		}
	}
}