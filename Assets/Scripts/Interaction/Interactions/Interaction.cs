using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : IDisplay
{
	public class InteractionMapCursor
	{
		public PlayerMapInteraction.HighlightMode highlightMode = PlayerMapInteraction.HighlightMode.Border;
		public bool showLineIndicator = false;
	}

	public static readonly InteractionMapCursor basicBorder = new InteractionMapCursor() 
	{
		highlightMode = PlayerMapInteraction.HighlightMode.Border
	};

	public static readonly InteractionMapCursor basicSquare = new InteractionMapCursor()
	{
		highlightMode = PlayerMapInteraction.HighlightMode.Square
	};

	public static readonly InteractionMapCursor basicSquareWithLine = new InteractionMapCursor()
	{
		highlightMode = PlayerMapInteraction.HighlightMode.Square,
		showLineIndicator = true
	};

	public static readonly InteractionMapCursor none = new InteractionMapCursor()
	{
		highlightMode = PlayerMapInteraction.HighlightMode.None
	};

	public static readonly InteractionMapCursor noneWithLine = new InteractionMapCursor()
	{
		highlightMode = PlayerMapInteraction.HighlightMode.None,
		showLineIndicator = true
	};

	public static class Ranges
	{
		public const float standard = 500;
		public const float infinity = Mathf.Infinity;
	}

	protected Sprite sprite;

	public virtual bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return false;
	}

	public virtual void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{

	}

	public virtual InteractionMapCursor GetMapCursorData(PlayerMapInteraction.UnderMouseData target)
	{
		return basicBorder;
	}

	//

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

	public virtual int GetDrawPriority()
	{
		return 0;
	}

	public virtual bool OverrideUIState()
	{
		return false;
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

	public static class InteractionValidationHelper
	{
		public static bool AttackOnMapValidation(PlayerMapInteraction.UnderMouseData target)
		{
			//Should replace with proper can attack check when infrastructure exists
			return target.simulationEntity != null || target.baseLocation != null;
		}

		public static bool AttackValidation(SimObjectBehaviour interactable)
		{
			return !PlayerSimObjBehaviour.IsPlayerSimObjectBehaviour(interactable) && interactable.battleEnabled;
		}

		public static bool ShopValidation(SimObjectBehaviour interactable)
		{
			if (interactable.Linked())
			{
				//Need to have shop and player needs to have a accepetable rep (or if the shop is open so we can click to close it)
				return interactable.target.HasShop() && (PlayerAboveProperInteractionThreshold(interactable) || ShopUIControl.ShopUIOpen());
			}

			return false;
		}

		public static bool QuestInteraction(SimObjectBehaviour interactable)
		{
			if (interactable.Linked())
			{
				return interactable.target.HasQuests() && (PlayerAboveProperInteractionThreshold(interactable) || QuestUIControl.QuestUIOpen());
			}

			return false;
		}

		public static bool PlayerAboveProperInteractionThreshold(SimObjectBehaviour interactable)
		{
			if (interactable.Linked())
			{
				return interactable.target.GetPlayerReputation() > BalanceManagement.properInteractionAllowedThreshold;
			}

			return false;
		}
	}
}
