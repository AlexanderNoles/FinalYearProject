using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManagement : MonoBehaviour
{
	private static Dictionary<Collider, IInteractable> interactables = new Dictionary<Collider, IInteractable>();

	public static void AddInteractable(Collider target, IInteractable interactable)
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
	}

	private static Interaction currentInteraction;

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

		const float maxSelectDistance = 1000;
		//Each frame check if we are over an interactable objects
		//if so be check if that one can be interacted with
		//If player is not hovering over UI (or in a ui state) try to find any targets that are under mouse
		if (UIHelper.ElementsUnderMouse().Count <= 0 && UIManagement.InNeutral())
		{
			//Get mouse view ray
			Ray mouseViewRay = CameraManagement.GetMainCamera().ScreenPointToRay(InputManagement.GetMousePosition());

			RaycastHit[] hits = Physics.RaycastAll(mouseViewRay, maxSelectDistance);

			IInteractable newTarget = null;
			float currentLowestRange = float.MaxValue;

			foreach (RaycastHit hit in hits)
			{
				if (interactables.TryGetValue(hit.collider, out IInteractable target))
				{
					if (hit.distance < currentLowestRange)
					{
						currentLowestRange = hit.distance;
						newTarget = target;
					}
				}
			}

			if (newTarget != null)
			{
				//On mouse over
				bool validationResult = currentInteraction.Validate(newTarget);

				if (validationResult && InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
				{
					//On Interact button pressed
					currentInteraction.Process(newTarget);
				}
			}
		}
	}
}
