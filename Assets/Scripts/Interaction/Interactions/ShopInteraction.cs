using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteraction : Interaction
{
	public override bool Validate(InteractableBase interactable)
	{
		return InteractionValidationHelper.ShopValidation(interactable);
	}

	public override void Process(InteractableBase interactable)
	{
		VisitableLocation targetLocation = (interactable as LocationContextLinkedInteractable).simulationContext.target;

		if (targetLocation != null)
		{
			//Activate shop ui and have it display and act on shop information
			//Need to pass the Visitable Location itself so the shop can auto close if the location is too far away!
			ShopUIControl.ToggleShopUI(targetLocation, interactable.transform);
		}
	}

	protected override string GetIconPath()
	{
		return "shopInteraction";
	}
}
