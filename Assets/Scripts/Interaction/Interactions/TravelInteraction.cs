using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return (target.baseLocation != null || target.simulationEntity != null) && PlayerManagement.PlayerEntityExists() && !PlayerCapitalShip.IsJumping();
	}

	public override int GetDrawPriority()
	{
		return 95;
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		//Grab player inventory
		PlayerInventory playerInventory = PlayerManagement.GetInventory();

		//Start ship jump
		if (target.baseLocation != null)
		{
			PlayerCapitalShip.StartJump(target.baseLocation);
		}
		else
		{
			ArbitraryLocation newLocation = new ArbitraryLocation();
			newLocation.SetLocation(target.cellCenter);

			PlayerCapitalShip.StartJump(newLocation);
		}


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

	public override InteractionMapCursor GetMapCursorData(PlayerMapInteraction.UnderMouseData target)
	{
		if (target.baseLocation == null)
		{
			return basicSquareWithLine;
		}

		return noneWithLine;
	}

	public override string GetTitle()
	{
		return "Travel";
	}
}
