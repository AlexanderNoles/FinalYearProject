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
		VisitableLocation targetLocation = (interactable as ContextLinkedInteractable).simulationContext.target;

		if (targetLocation != null)
		{
			//Activate shop ui and have it display and act on shop information
			targetLocation.GetShop();
		}
	}

	protected override string GetIconPath()
	{
		return "shopInteraction";
	}
}
