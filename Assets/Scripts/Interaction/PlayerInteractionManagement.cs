using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManagement : MonoBehaviour
{
	private static Dictionary<Collider, InteractableControl> interactables = new Dictionary<Collider, InteractableControl>();

	public static void AddInteractable(Collider target, InteractableControl interactable)
	{
		interactables.Add(target, interactable);
	}

	public static void RemoveInteractable(Collider target)
	{
		interactables.Remove(target);
	}

	public static void SetCurrentInteraction(Interaction newInteraction, Sprite mouseTagALongSprite)
	{
		currentInteraction = newInteraction;
		tagALongSprite = mouseTagALongSprite;
	}

	private static Interaction currentInteraction;
	private static Sprite tagALongSprite;

	public UIState uiStateWithInteractions;
	public Sprite interactionValFailed;

	private void Awake()
	{
		currentInteraction = null;
	}

	private void Update()
	{
		if (currentInteraction == null)
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

		bool anyValid = false;
		if (UIHelper.ElementsUnderMouse().Count <= 0)
		{
			//Get mouse view ray
			Ray mouseViewRay = CameraManagement.GetMainCamera().ScreenPointToRay(InputManagement.GetMousePosition());

			RaycastHit[] hits = Physics.RaycastAll(mouseViewRay, currentInteraction.GetRange());

			//Get target control
			InteractableControl newTarget = null;
			float currentLowestRange = float.MaxValue;

			foreach (RaycastHit hit in hits)
			{
				if (interactables.TryGetValue(hit.collider, out InteractableControl target))
				{
					if (hit.distance < currentLowestRange)
					{
						currentLowestRange = hit.distance;
						newTarget = target;
					}
				}
			}

			bool bypass = false;
			List<InteractableBase> bypassedControl = new List<InteractableBase>();
			if (PlayerCapitalShip.InJumpTravelStage() && newTarget == null)
			{
				bypass = true;
				bypassedControl.Add(WarpInteractable.GetInstance());
			}

			if (newTarget != null || bypass)
			{
				List<InteractableBase> controlled = bypass ? bypassedControl : newTarget.GetControlled();

				foreach (InteractableBase target in controlled)
				{
					bool validationResult = currentInteraction.Validate(target);

					if (validationResult)
					{

						//On mouse over
						if (InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
						{
							//On Interact button pressed
							currentInteraction.Process(target);
						}

						anyValid = true;
						break;
					}
				}
			}
		}

		Sprite targetSprite = anyValid ? tagALongSprite : interactionValFailed;

		if (uiStateWithInteractions.mouseState.tagALongImage != targetSprite)
		{
			uiStateWithInteractions.mouseState.tagALongImage = targetSprite;
			MouseManagement.ReloadMouseState();
		}
	}
}
