using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteraction : Interaction
{
	public override bool Validate(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.ShopValidation(interactable);
	}

	public override void Process(SimObjectBehaviour interactable)
	{
		SimObject targetObject = interactable.target;

		if (targetObject != null)
		{
			//Activate shop ui and have it display and act on shop information
			//Need to pass the Visitable Location itself so the shop can auto close if the location is too far away!
			ShopUIControl.ToggleShopUI(targetObject, interactable.transform);
		}
	}

	protected override string GetIconPath()
	{
		return "shopInteraction";
	}
}
