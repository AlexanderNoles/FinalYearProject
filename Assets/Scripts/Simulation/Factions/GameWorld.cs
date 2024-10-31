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
}
