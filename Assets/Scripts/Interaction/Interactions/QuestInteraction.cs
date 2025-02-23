using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInteraction : Interaction
{
	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.QuestInteraction(interactable);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		QuestUIControl.ToggleQuestUI(interactable.target.GetQuestGiver(), interactable.transform);
	}

	protected override string GetIconPath()
	{
		return "questInteraction";
	}

	public override int GetDrawPriority()
	{
		return 75;
	}
}
