using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : Faction
{
    public override void InitTags()
    {
        base.InitTags();
        AddTag(Tags.Nation);
    }
}
