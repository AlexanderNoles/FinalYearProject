using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    public void Simulate()
    {
        InitTags();
        InitData();
    }

    //Tags
    public enum Tags
    {
        //This is a global tag that allows routines to grab every faction
        Faction,
        //
        Territory, //Does this faction contain some territory?
        Settlements, //Does this faction have settlements?
        Nation, //Is this faction a nation?
        Emblem, //Does this faction have an emblem?
        GameWorld
    }

    public void AddTag(Tags tag)
    {
        //Add to simulation manager tag find system
        SimulationManagement.AddFactionOfTag(tag, this);
    }

    public void RemoveTag(Tags tag)
    {
        //Remove from simulation manager tag find system
        SimulationManagement.RemoveFactionOfTag(tag, this);
    }

    public virtual void InitTags()
    {
        AddTag(Tags.Faction);
    }
    //


    //Data
    private Dictionary<string, DataBase> dataModules;

    public virtual void InitData()
    {
        //Add only faction data by default
        dataModules = new Dictionary<string, DataBase>();
        AddData(Tags.Faction, new FactionData());
    }

    public bool GetData(string dataIdentifier, out DataBase data)
    {
        return dataModules.TryGetValue(dataIdentifier, out data);
    }

    public bool GetData(Tags tag, out DataBase data)
    {
        return GetData(tag.ToString(), out data);
    }

    public void AddData(string dataIdentifier, DataBase data)
    {
        if (dataModules.ContainsKey(dataIdentifier))
        {
            return;
        }

        dataModules.Add(dataIdentifier, data);
    }

    public void AddData(Tags tag, DataBase data)
    {
        AddData(tag.ToString(), data);
    }

    public void RemoveData(string dataIdentifier)
    {
        dataModules.Remove(dataIdentifier);
    }

    public void RemoveData(Tags tag)
    {
        RemoveData(tag.ToString());
    }
    //



    //General helper functions
    public Color GetColour()
    {
        if (GetData(Tags.Emblem, out DataBase data2))
        {
            return (data2 as EmblemData).mainColour;
        }

        return Color.white;
    }
    //
}
