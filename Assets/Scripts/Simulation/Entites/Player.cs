using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

//The player's corresponding simulation entity, stores a lot of data about the player
public class Player : SimulationEntity
{
	public override void InitTags()
	{
		base.InitTags();

		AddTag(EntityTypeTags.Player);
		AddTag(EntityStateTags.Unkillable);
	}

	public override void InitData()
	{
		base.InitData();

		AddData(DataTags.Emblem, new EmblemData());

		PlayerStats stats = new PlayerStats();
		stats.ResetStatsToDefault();
		AddData(DataTags.Stats, stats);

		PlayerInventory inventory = new PlayerInventory();
		inventory.SetStatsTarget(stats);
		AddData(DataTags.Inventory, inventory);

		PlayerInteractions interactions = new PlayerInteractions();
		//Add basic interactions that all players have
		//The order of interactions matters as the smart interaction will
		//iterate through them in that order
		interactions.playersInteractions.Add(new ShopInteraction().Init());
		interactions.playersInteractions.Add(new AttackInteraction().Init());
		//
		AddData(DataTags.Interactions, interactions);
	}
}
