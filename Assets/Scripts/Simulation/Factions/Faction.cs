using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    public int id = -1;

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
		GameWorld, //This is the game world faction
        //
        Territory, //Does this faction contain some territory?
        Settlements, //Does this faction have settlements?
        Nation, //Is this faction a nation?
        Population, //Does this faction have a population?
        Emblem, //Does this faction have an emblem?
		HasMilitary, //Does this faction have a military?
		Politics, //Does this faction engage in politics?
        //Is this faction visible in a way that makes it so every other faction has a relationship with it?
        //(This means that every faction will will know if it's existence)
        //This doesn't mean it immediately has a dual relationship with every other faction (cause it may not know about them, for example not all nations immediately know about the player)
        OpenRelationship,
		CanFightWars, //Ceratin factions can not fight wars, this does not mean they cannot fight battles just not full scale wars
		Historical //Is this faction historical in some sense (e.g., does it represent a fallen empire)
    }

    //Useful keys for data that doesn't cleanly belong to a tag
    public const string relationshipDataKey = "RelationshipData";
    public const string battleDataKey = "BattleData";


	private HashSet<Tags> tags;

    public bool HasTag(Tags tag)
    {
        return tags.Contains(tag);
    }

    public void AddTag(Tags tag)
    {
        //Add to simulation manager tag find system
        SimulationManagement.AddFactionOfTag(tag, this);
        //Add to personal tags
        tags.Add(tag);
    }

    public void RemoveTag(Tags tag)
    {
        //Remove from simulation manager tag find system
        SimulationManagement.RemoveFactionOfTag(tag, this);
        //Remove from personal tags
        tags.Remove(tag);
    }

    public virtual void InitTags()
    {
        tags = new HashSet<Tags>();
        AddTag(Tags.Faction);
    }
    //


    //Data
    protected Dictionary<string, DataBase> dataModules;

    public virtual void InitData()
    {
        //Add only basic faction data by default
        dataModules = new Dictionary<string, DataBase>();

        //Meta data used by every faction
        //This generally holds info on how the MetaRoutine should deal with this faction
        AddData(Tags.Faction, new FactionData());
        //Factions should always be able to have relationships (it's what makes them interesting!)
        //It also makes it very easy to remove relationships when a faction is fully removed (as that's a MetaRoutine Responsibility)!
        AddData(relationshipDataKey, new RelationshipData());
		//Allows a faction to fight, something they should always be able to do
		//just because they can does not mean they will
		AddData(battleDataKey, new BattleData());
    }

    public bool GetData<T>(string dataIdentifier, out T data) where T : DataBase
    {
        bool toReturn = dataModules.TryGetValue(dataIdentifier, out DataBase tempData);
        data = tempData as T;

        return toReturn;
    }

    public bool GetData<T>(Tags tag, out T data) where T : DataBase 
    {
        return GetData<T>(tag.ToString(), out data);
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
        if (GetData(Tags.Emblem, out EmblemData emblemData))
        {
            return emblemData.mainColour;
        }

        return Color.white;
    }
    //
}
