using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorld : Faction
{
	public override void InitTags()
	{
		base.InitTags();
		AddTag(Tags.GameWorld);
	}

	public override void InitData()
	{
		dataModules = new Dictionary<string, DataBase>();

		//Don't add relationship data for this faction, cause it is the gameworld
		AddData(Tags.Faction, new FactionData());
		AddData(Tags.GameWorld, new GlobalBattleData());
		AddData(Tags.Historical, new HistoryData());
	}
}
