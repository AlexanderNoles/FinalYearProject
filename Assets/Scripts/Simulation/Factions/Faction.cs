using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    //Tags
    public enum Tags
    {
        //This is a global tag that allows routines to grab every faction
        Faction,
        //
        Nation,
        GameWorld
    }

    public void AddTag(Tags tag)
    {
        //Add to simulation manager tag find system
        SimulationManager.AddFactionOfTag(tag, this);
    }

    public void RemoveTag(Tags tag)
    {
        //Remove from simulation manager tag find system
        SimulationManager.RemoveFactionOfTag(tag, this);
    }

    public void Simulate()
    {
        InitTags();
    }

    public virtual void InitTags()
    {
        AddTag(Tags.Faction);
    }
}
