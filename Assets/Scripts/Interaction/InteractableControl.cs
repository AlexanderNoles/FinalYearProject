using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableControl : MonoBehaviour
{
	public bool canBeInteractedWith = true;
	public Collider mouseTargetCollider;

	private List<InteractableBase> controlledInteractables = new List<InteractableBase>();

	public List<InteractableBase> GetControlled()
	{
		return controlledInteractables;
	}

	private void Awake()
	{
		//Find all interactable bases
		GetComponents(controlledInteractables);
	}

	protected virtual void OnEnable()
	{
		if (!canBeInteractedWith) return;

		PlayerInteractionManagement.AddInteractable(mouseTargetCollider, this);
	}

	protected virtual void OnDisable()
	{
		if (!canBeInteractedWith) return;

		PlayerInteractionManagement.RemoveInteractable(mouseTargetCollider);
	}
}
