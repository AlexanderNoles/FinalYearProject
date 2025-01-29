using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
{
	public override bool Validate(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.AttackValidation(interactable);
	}

	public override void Process(SimObjectBehaviour interactable)
	{
		PlayerSimObjBehaviour.ToggleTargetExternal(interactable);
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
