using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class GameWorld : SimulationEntity
{
	public static GameWorld main;

    public override void Simulate()
    {
        base.Simulate();

		//Set main game world, so it can be accessed later
		//Perhaps a singleton system for certain entites should be created?
		main = this;
    }

    public override void InitEntityTags()
	{
		base.InitEntityTags();
		AddTag(EntityTypeTags.GameWorld);
		AddTag(EntityStateTags.Unkillable);
	}

	public override void InitData()
	{
		base.InitData();
		//Don't add relationship data for this faction, cause it is the gameworld
		AddData(DataTags.GlobalBattle, new GlobalBattleData());
		HistoryData historyData = new HistoryData();
		//Add origin period
		HistoryData.Period originPeriod = new HistoryData.Period();
		originPeriod.name = "Origin Period";
		historyData.AddPeriod(originPeriod);
		//
		AddData(DataTags.Historical, historyData);

		AddData(DataTags.Calamity, new CalamityData());
	}
}
