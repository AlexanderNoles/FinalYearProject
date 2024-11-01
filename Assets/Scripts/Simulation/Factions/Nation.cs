using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : Faction
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(Tags.Territory);
        AddTag(Tags.Nation);
        AddTag(Tags.Emblem);
    }

    public override void InitData()
    {
        base.InitData();
        AddData(Tags.Territory, new TerritoryData());
        AddData(Tags.Emblem, new EmblemData());
    }
}
