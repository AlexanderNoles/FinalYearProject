using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return target.baseLocation != null && PlayerManagement.PlayerEntityExists() && !PlayerCapitalShip.IsJumping();
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		//Grab player inventory
		PlayerInventory playerInventory = PlayerManagement.GetInventory();

		//Start ship jump
		PlayerCapitalShip.StartJump(target.baseLocation);

		//Subtract fuel if fuel is enabled
		if (PlayerManagement.fuelEnabled)
		{
			//Remove fuel and have player ship change the ui label over the course of the jump
			PlayerCapitalShip.HaveFuelChangeOverJump(playerInventory.fuel, playerInventory.fuel - target.fuelCostToLocation);
			playerInventory.fuel -= target.fuelCostToLocation;
		}
	}

	protected override string GetIconPath()
	{
		return "travelInteraction";
	}

	public override InteractionMapCursor GetMapCursorData()
	{
		return noneWithLine;
	}
}
