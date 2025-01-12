using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : IDisplay
{
	public static class Ranges
	{
		public const float standard = 500;
		public const float infinity = Mathf.Infinity;
	}


	protected Sprite sprite;

	public virtual bool Validate(InteractableBase interactable)
	{
		return true;
	}

	public virtual void Process(InteractableBase interactable)
	{
		//Do nothing by default
	}

	public virtual float GetRange()
	{
		return Ranges.standard;
	}

	public virtual Interaction Init()
	{
		VisualDatabase.LoadIconFromResources(GetIconPath(), out sprite);
		return this;
	}

	protected virtual string GetIconPath()
	{
		return "";
	}

	//IDisplay

	public virtual string GetTitle()
	{
		return "Interaction";
	}

	public virtual string GetDescription()
	{
		return "";
	}

	public virtual Sprite GetIcon()
	{
		return sprite;
	}

	public virtual string GetExtraInformation()
	{
		return "";
	}

	protected static class InteractionValidationHelper
	{
		public static bool AttackValidation(InteractableBase interactable)
		{
			return interactable is BattleBehaviour &&!PlayerBattleBehaviour.IsPlayerBB(interactable as BattleBehaviour);
		}

		public static bool ShopValidation(InteractableBase interactable)
		{
			if (interactable is ContextLinkedInteractable)
			{
				//Linked to simulation context

				LocationContext targetContext = (interactable as ContextLinkedInteractable).simulationContext;

				if (targetContext != null)
				{
					return targetContext.target.HasShop();
				}
			}

			return false;
		}
	}
}
