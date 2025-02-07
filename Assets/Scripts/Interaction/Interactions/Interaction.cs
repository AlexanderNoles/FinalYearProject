using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : IDisplay
{
	public class InteractionMapCursor
	{
		public PlayerMapInteraction.HighlightMode highlightMode = PlayerMapInteraction.HighlightMode.Border;
	}

	public static readonly InteractionMapCursor basicBorder = new InteractionMapCursor() 
	{
		highlightMode = PlayerMapInteraction.HighlightMode.Border
	};

	public static readonly InteractionMapCursor basicSquare = new InteractionMapCursor()
	{
		highlightMode = PlayerMapInteraction.HighlightMode.Square
	};

	public static class Ranges
	{
		public const float standard = 500;
		public const float infinity = Mathf.Infinity;
	}

	protected Sprite sprite;

	public virtual bool ValidateEntity(SimulationEntity target)
	{
		return false;
	}

	public virtual void ProcessEntity(SimulationEntity target)
	{

	}

	public virtual InteractionMapCursor GetMapCursorData()
	{
		return basicBorder;
	}

	public virtual bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return false;
	}

	public virtual void ProcessBehaviour(SimObjectBehaviour interactable)
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
		public static bool AttackOnMapValidation(SimulationEntity entity)
		{
			//Should replace with proper can attack check when infrastructure exists
			return true;
		}

		public static bool AttackValidation(SimObjectBehaviour interactable)
		{
			return !PlayerSimObjBehaviour.IsPlayerBB(interactable) && interactable.battleEnabled;
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
