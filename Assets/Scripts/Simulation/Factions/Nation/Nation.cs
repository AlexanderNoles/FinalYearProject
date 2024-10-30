using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : Faction
{
    private NationData data;

    public override FactionData GetFactionData()
    {
        return data;
    }

    public Nation()
    {
        data = new NationData();
    }
}
