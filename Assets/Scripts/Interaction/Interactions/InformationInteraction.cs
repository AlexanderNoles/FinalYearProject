using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationInteraction : Interaction
{
	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return false;
	}

	protected override string GetIconPath()
	{
		return "informationInteraction";
	}
}
