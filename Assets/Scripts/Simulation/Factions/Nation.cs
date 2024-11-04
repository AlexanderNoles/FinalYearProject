using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : Faction
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(Tags.Territory);
        AddTag(Tags.Settlements);
        AddTag(Tags.Nation);
        AddTag(Tags.Emblem);
        AddTag(Tags.Population);
    }

    public override void InitData()
    {
        base.InitData();
        AddData(Tags.Territory, new TerritoryData());
        AddData(Tags.Emblem, new EmblemData());
        AddData(Tags.Settlements, new SettlementData());
        AddData(Tags.Population, new PopulationData());
    }
}
