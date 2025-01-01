using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class Nation : Faction
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(EntityTypeTags.Nation);
    }

    public override void InitData()
    {
        base.InitData();
        AddData(DataTags.Territory, new TerritoryData());
        AddData(DataTags.Emblem, new EmblemData());
        AddData(DataTags.Settlement, new SettlementData());
        AddData(DataTags.Population, new PopulationData());
		AddData(DataTags.Political, new PoliticalData());
		AddData(DataTags.Military, new MilitaryData());
		AddData(DataTags.War, new WarData());
		AddData(DataTags.Economic, new EconomyData());
		AddData(DataTags.ContactPolicy, new ContactPolicyData());
    }
}
