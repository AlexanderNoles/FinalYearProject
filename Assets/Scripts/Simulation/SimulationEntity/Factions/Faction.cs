using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class Faction : SimulationEntity
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(EntityTypeTags.Faction);
    }

    public override void InitData()
    {
        base.InitData();

        //Factions should always be able to have relationships (it's what makes them interesting!)
        //It also makes it very easy to remove relationships when a faction is fully removed (as that's a MetaRoutine Responsibility)!
        AddData(DataTags.Relationship, new FeelingsData());
        //Allows a faction to fight, something they should always be able to do
        //just because they can does not mean they will
        AddData(DataTags.Relationship, new BattleData());
    }
}
