using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityAndDataDescriptor
{
    //There are a couple fundamental tag types
    //Tags are used for filtering or conditionals, they describe something generally about an entity or data

    public enum EntityTypeTags
    {
		VoidSwarm,
        Faction,
        Nation,
        Player,
		MineralDeposit,
        GameWorld,
		PirateCrew,
		AntiVoidKnights
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
        Settlements,
		Refinery,
        Population,
		Desirability,
        Emblem, 
		EntitySpawner,
		SpawnSource,
		CentralShop,
		Quests,
		Calamity,
        Stats,
        Inventory,
		Policies,
		Interactions,
        Military,
		Name,
		Strategy,
        Political,
        Economic,
        TargetableLocation,
		Timer,
        Feelings,
        ContactPolicy,
        Historical
    }
}
