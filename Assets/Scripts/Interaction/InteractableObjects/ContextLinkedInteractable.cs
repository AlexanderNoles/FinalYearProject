using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextLinkedInteractable : InteractableBase
{
	public LocationContext simulationContext;

	private void OnEnable()
	{
		OnSetup(simulationContext);
	}

	protected virtual void OnSetup(LocationContext locationContext)
	{
		//Do nothing by default
	}
}
