using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return target.baseLocation == null && target.simulationEntity != null && target.simulationEntity.HasData(DataTags.Economic) && !PlayerCapitalShip.IsJumping();
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		ShopUIControl.ToggleShopUI(target.simulationEntity, null);
	}

	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.ShopValidation(interactable);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		SimObject targetObject = interactable.target;

		if (targetObject != null)
		{
			//Activate shop ui and have it display and act on shop information
			//Need to pass the Visitable Location itself so the shop can auto close if the location is too far away!
			ShopUIControl.ToggleShopUI(targetObject, interactable.transform);
		}
	}

	public override int GetDrawPriority()
	{
		return 90;
	}

	protected override string GetIconPath()
	{
		return "shopInteraction";
	}

	public override string GetTitle()
	{
		return "Trade";
	}
}
