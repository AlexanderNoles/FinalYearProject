using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopDirectInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return InteractionValidationHelper.AttackOnMapValidation(target) && !PlayerCapitalShip.IsJumping();
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{       
		//Get position
		RealSpacePosition rps = null;
		int targetID = -1;

		//Targeting a specific loaction has preference over a general entity
		if (target.baseLocation != null)
		{
			rps = target.baseLocation.GetPosition();
		}

		if (target.simulationEntity != null)
		{
			if (rps == null)
			{
				rps = target.cellCenter;
			}

			targetID = target.simulationEntity.id;
		}

		//Open attack ui
		if (rps != null)
		{
			//Need to pass the target entities id
			//And our id
			//Alongside the target position
			TroopTransferUIControl.Show(PlayerManagement.GetTarget().id, targetID, rps);
		}
	}

	public override InteractionMapCursor GetMapCursorData()
	{
		return basicSquareWithLine;
	}

	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		//Currently only allowing attacking visitable locations
		return InteractionValidationHelper.AttackValidation(interactable) && interactable.target is VisitableLocation;
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		//Attempt to find a position to place the new battlefield
		RealSpacePosition rps = null;
		int targetID = -1;

		if (interactable.target is VisitableLocation)
		{
			//If target is a visitable location
			rps = (interactable.target as VisitableLocation).GetPosition();
			targetID = interactable.target.parent.Get().id;
		}

		if (rps != null)
		{
			TroopTransferUIControl.Show(PlayerManagement.GetTarget().id, targetID, rps);
		}
	}

	protected override string GetIconPath()
	{
		return "troopDirection";
	}
}
