using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
{
	public override bool ValidateEntity(SimulationEntity target)
	{
		return InteractionValidationHelper.AttackOnMapValidation(target);
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
		return basicSquare;
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
