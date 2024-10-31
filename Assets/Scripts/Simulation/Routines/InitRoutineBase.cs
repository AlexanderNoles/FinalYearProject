using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitRoutineBase : RoutineBase
{
    public virtual bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return false;
    }
}
