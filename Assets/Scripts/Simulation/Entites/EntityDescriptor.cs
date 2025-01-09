using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityAndDataDescriptor
{
    //There are a couple fundamental tag types
    //Tags are used for filtering or conditionals, they describe something generally about an entity or data

    public enum EntityTypeTags
    {
        Faction,
        Nation,
        Player,
        GameWorld
    }

    public enum EntityStateTags
    {
        Dead, //This entity should be removed from the simulation
        Unkillable, //This entity cannot be removed from the simulation
        Insignificant //Is this entity insignificant?
    }

    public enum DataTags
    {
        GlobalBattle,
        Battle,
        Territory,
        Settlement,
        Population,
        Emblem, 
        Stats,
        Inventory,
		Interactions,
        Military,
        Political,
        Economic, 
        Capital,
        War,
        Feelings,
        ContactPolicy,
        Historical
    }
}
