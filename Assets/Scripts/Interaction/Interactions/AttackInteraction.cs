using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackInteraction : Interaction
{
	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return InteractionValidationHelper.AttackValidation(interactable);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		UnityAction onAccept = () => { PlayerSimObjBehaviour.ToggleTargetExternal(interactable); };

		if (interactable.Linked())
		{
			//Ask player if they are sure if they have a good rep with this target
			//and the target isn't openly hostile
			if (!PlayerSimObjBehaviour.CurrentlyTargeting(interactable) && 
				interactable.target.GetPlayerReputation(BalanceManagement.properInteractionAllowedThreshold) > BalanceManagement.properInteractionAllowedThreshold &&
				!(interactable.target.parent != null && interactable.target.parent.Get().GetData(DataTags.ContactPolicy, out ContactPolicyData data) && data.openlyHostile))
			{
				//Has good rep with this target
				//Show confirm popup
				PopupUIControl.SetActive("Warning!", "You have a good reputation with this entity! Are you sure you want to attack?", onAccept);
			}
			else
			{
				//If bad or not rep just run on accept
				onAccept.Invoke();
			}
		}
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

	public override string GetTitle()
	{
		return "Attack";
	}
}
