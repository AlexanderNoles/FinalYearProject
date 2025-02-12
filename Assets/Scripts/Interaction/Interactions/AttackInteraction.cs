using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
{
	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.AttackValidation(interactable);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		PlayerSimObjBehaviour.ToggleTargetExternal(interactable);
	}

	protected override string GetIconPath()
	{
		return "attackInteraction";
	}

	public override int GetDrawPriority()
	{
		return 100;
	}

	public override float GetRange()
	{
		//Infinite target get range
		//This is so you can always target enemies
		return Ranges.infinity;
	}
}
