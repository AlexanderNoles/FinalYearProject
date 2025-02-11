using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
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

	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.AttackValidation(interactable);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		PlayerSimObjBehaviour.ToggleTargetExternal(interactable);
	}

	public override InteractionMapCursor GetMapCursorData()
	{
		return basicSquareWithLine;
	}

	protected override string GetIconPath()
	{
		return "attackInteraction";
	}

	public override float GetRange()
	{
		//Infinite target get range
		//This is so you can always target enemies
		return Ranges.infinity;
	}
}
