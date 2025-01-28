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

	public virtual bool Validate(SimObjectBehaviour interactable)
	{
		return true;
	}

	public virtual void Process(SimObjectBehaviour interactable)
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
		public static bool AttackValidation(SimObjectBehaviour interactable)
		{
			return interactable is BattleBehaviour &&!PlayerBattleBehaviour.IsPlayerBB(interactable as BattleBehaviour);
		}

		public static bool ShopValidation(SimObjectBehaviour interactable)
		{
			if (interactable.Linked())
			{
				return interactable.target.HasShop();
			}

			return false;
		}
	}
}
