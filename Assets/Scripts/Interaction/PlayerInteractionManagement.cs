using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManagement : MonoBehaviour
{
	private static Dictionary<Collider, SimObjectBehaviour> interactables = new Dictionary<Collider, SimObjectBehaviour>();

	public static void AddInteractable(Collider target, SimObjectBehaviour interactable)
	{
		interactables.Add(target, interactable);
	}

	public static void RemoveInteractable(Collider target)
	{
		interactables.Remove(target);
	}

	public static void SetCurrentInteraction(Interaction newInteraction)
	{
		currentInteraction = newInteraction;
		EnableSmartInteraction(false);
	}

	public static object GetCurrentInteraction()
	{
		return currentInteraction;
	}

	public static void EnableSmartInteraction(bool active)
	{
		smartInteractionEnabled = active;

		if (active)
		{
			currentInteraction = null;
		}
	}

	private static Interaction currentInteraction;
	private static bool smartInteractionEnabled = false;

	public UIState uiStateWithInteractions;
	public Sprite interactionValFailed;

	private void Awake()
	{
		currentInteraction = null;
		smartInteractionEnabled = false;
	}

	private void Update()
	{
		if (currentInteraction == null && !smartInteractionEnabled)
		{
			return;
		}

		//Each frame check if we are over an interactable object
		//if so be check if that one can be interacted with
		//If player is not hovering over UI (or in a ui state) try to find any targets that are under mouse

		if (!UIManagement.InNeutral())
		{
			return;
		}

		PerformInteraction(out Sprite mouseTagAlongSprite);

		if (uiStateWithInteractions.mouseState.tagALongImage != mouseTagAlongSprite)
		{
			uiStateWithInteractions.mouseState.tagALongImage = mouseTagAlongSprite;
			MouseManagement.ReloadMouseState();
		}
	}

	private void PerformInteraction(out Sprite interactionIcon)
	{
		interactionIcon = null;

		if (UIHelper.ElementsUnderMouse().Count <= 0)
		{
			//Get mouse view ray
			Ray mouseViewRay = CameraManagement.GetMainCamera().ScreenPointToRay(InputManagement.GetMousePosition());

			float range = Mathf.Infinity;
			if (currentInteraction != null)
			{
				range = currentInteraction.GetRange();
			}

			RaycastHit[] hits = Physics.RaycastAll(mouseViewRay, range);

			//Get target control
			SimObjectBehaviour target = null;
			float currentLowestRange = float.MaxValue;

			foreach (RaycastHit hit in hits)
			{
				if (interactables.TryGetValue(hit.collider, out SimObjectBehaviour outTarget))
				{
					if (hit.distance < currentLowestRange)
					{
						currentLowestRange = hit.distance;
						target = outTarget;
					}
				}
			}

			if (target == null)
			{
				//Warp fallback
				if (PlayerCapitalShip.InJumpTravelStage() && PlayerManagement.GetInventory().HasItemOfType(typeof(WarpShopItemBase)))
				{
					target = WarpSimBehaviour.GetInstance();
				}
			}

			if (target != null)
			{
				List<Interaction> targetInteractions = new List<Interaction>();

				if (smartInteractionEnabled)
				{
					targetInteractions = PlayerManagement.GetInteractions().playersInteractions;
				}
				else
				{
					targetInteractions.Add(currentInteraction);
				}

				foreach (Interaction targetInteraction in targetInteractions)
				{
					bool validationResult = targetInteraction.Validate(target);

					if (validationResult)
					{
						//On mouse over
						if (InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
						{
							//On Interact button pressed
							targetInteraction.Process(target);
						}

						interactionIcon = targetInteraction.GetIcon();
						return;
					}
				}
			}
		}
	}
}
