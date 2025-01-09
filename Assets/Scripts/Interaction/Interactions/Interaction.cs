using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : IDisplay
{
	protected Sprite sprite;

	public virtual bool Validate(IInteractable interactable)
	{
		return true;
	}

	public virtual void Process(IInteractable interactable)
	{
		//Do nothing by default
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

	public static class ValidationHelper
	{
		public static bool AttackValidation(IInteractable interactable)
		{
			return !PlayerBattleBehaviour.instance.Equals(interactable) && interactable is BattleBehaviour;
		}
	}
}
