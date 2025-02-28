using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMapInteraction;

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

	public static Interaction GetCurrentInteraction()
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

	public static Interaction chosenInteraction;
	private static Interaction currentInteraction;
	private static bool smartInteractionEnabled = false;

	public List<UIState> uiStatesWithInteractions;
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
		//If player is not hovering over UI (or in a ui state that is not coenabled) try to find any targets that are under mouse

		if (!UIManagement.NeutralEnabled() && (currentInteraction == null || !currentInteraction.OverrideUIState()))
		{
			return;
		}

		List<Sprite> validatedInteractionSprites = PerformInteraction();

		foreach (UIState targetState in uiStatesWithInteractions)
		{
			targetState.mouseState.tagALongs = validatedInteractionSprites;

			if (UIManagement.IsCurrentState(targetState))
			{
				MouseManagement.ReloadMouseState();
			}
		}
	}

	private List<Sprite> PerformInteraction()
	{
		List<Sprite> toReturn = new List<Sprite>();
		chosenInteraction = null;
		SimObjectBehaviour target = null;
		UnderMouseData underMouseData = GetUnderMouseData();

		//Need to find all valid interactions and the current interaction
		//So we can tell the player all the interactions they can have with this object
		if (UIHelper.ElementsUnderMouse().Count <= 0)
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

			//If in pure neutral (not coenabled) shoot out a raycast to find a potential target
			if (UIManagement.InPureNeutral())
			{
				//Get target control
				List<SimObjectBehaviour> foundTargets = new List<SimObjectBehaviour>();
				List<float> ranges = new List<float>();
				bool careAboutRange = true;

				//Get mouse view ray
				Ray mouseViewRay = CameraManagement.GetMainCamera().ScreenPointToRay(InputManagement.GetMousePosition());

				float range = Mathf.Infinity;
				if (currentInteraction != null)
				{
					range = currentInteraction.GetRange();
				}

				//Shoot raycast
				RaycastHit[] hits = Physics.RaycastAll(mouseViewRay, range);
				foreach (RaycastHit hit in hits)
				{
					if (interactables.TryGetValue(hit.collider, out SimObjectBehaviour outTarget))
					{
						//Iterate through ranges until you find one you are less than
						//Insert at that index
						int index = 0;
						for (index = 0; index < ranges.Count;)
						{
							if (ranges[0] > hit.distance)
							{
								//Found one larger than us
								break;
							}

							index++;
						}

						foundTargets.Insert(index, outTarget);
						ranges.Insert(index, hit.distance);
					}
				}

				//Fallbacks, incase no object is found
				if (foundTargets.Count == 0)
				{
					//Warp fallback
					if (PlayerCapitalShip.InJumpTravelStage() && PlayerManagement.GetInventory().HasItemOfType(typeof(WarpShopItemBase)))
					{
						foundTargets.Add(WarpSimBehaviour.GetInstance());

						//Ensure we can always interact with the warp while jumping
						careAboutRange = false;
					}
				}

				//If we have found any targets
				//Run until we have found a target that accepts an interaction
				for (int i = 0; i < foundTargets.Count && chosenInteraction == null; i++)
				{
					target = foundTargets[i];

					foreach (Interaction targetInteraction in targetInteractions)
					{
						//Validate normally and ensure target is within range (when using smart interaction we can't pre limit the range on the raycast check)
						bool validationResult = targetInteraction.ValidateBehaviour(target) && (!careAboutRange || ranges[i] <= targetInteraction.GetRange());

						if (validationResult)
						{
							if (chosenInteraction == null)
							{
								//First one validated
								chosenInteraction = targetInteraction;
							}

							toReturn.Add(targetInteraction.GetIcon());
						}
					}
				}
			}
			else if (MapManagement.MapActive() && !MapManagement.MapIntroRunning())
			{
				//If in map ask player map interaction for a target
				foreach (Interaction targetInteraction in targetInteractions)
				{
					bool validationResult = targetInteraction.ValidateOnMap(underMouseData);

					if (validationResult)
					{
						if (chosenInteraction == null)
						{
							//First one validated
							chosenInteraction = targetInteraction;
						}

						//Add to valid interactions
						toReturn.Add(targetInteraction.GetIcon());
					}
				}
			}

			//Acutally process the interaction if one was found
			if (chosenInteraction != null)
			{
				if (InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
				{
					if (MapManagement.MapActive())
					{
						chosenInteraction.ProcessOnMap(underMouseData);
					}
					else if (target != null)
					{
						chosenInteraction.ProcessBehaviour(target);
					}
				}
			}
		}

		return toReturn;
	}
}
