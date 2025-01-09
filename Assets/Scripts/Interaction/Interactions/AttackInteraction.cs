using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteraction : Interaction
{
	public override bool Validate(IInteractable interactable)
	{
		return ValidationHelper.AttackValidation(interactable);
	}

	public override void Process(IInteractable interactable)
	{
		PlayerBattleBehaviour.ToggleTargetExternal(interactable as BattleBehaviour);
	}

	protected override string GetIconPath()
	{
		return "attackInteraction";
	}
}
