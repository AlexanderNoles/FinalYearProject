using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorld : Faction
{
    private GameWorldData data;

    public override FactionData GetFactionData()
    {
        return data;
    }

    public GameWorld()
    {
        data = new GameWorldData();
    }
}
