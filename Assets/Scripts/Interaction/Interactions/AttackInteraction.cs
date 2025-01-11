using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
{
	public override bool Validate(InteractableBase interactable)
	{
		return InteractionValidationHelper.AttackValidation(interactable);
	}

	public override void Process(InteractableBase interactable)
	{
		PlayerBattleBehaviour.ToggleTargetExternal(interactable as BattleBehaviour);
	}

	protected override string GetIconPath()
	{
		return "attackInteraction";
	}
}
