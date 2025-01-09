using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBase : MonoBehaviour, IInteractable
{
	public bool canBeInteractedWith = true;
	public Collider targetCollider;

	protected virtual void OnEnable()
	{
		if (!canBeInteractedWith) return;

		PlayerInteractionManagement.AddInteractable(targetCollider, this);
	}

	protected virtual void OnDisable()
	{
		if (!canBeInteractedWith) return;

		PlayerInteractionManagement.RemoveInteractable(targetCollider);
	}
}
