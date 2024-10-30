using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    public bool hasRunInit = false;

    public virtual FactionData GetFactionData()
    {
        return null;
    }
}
