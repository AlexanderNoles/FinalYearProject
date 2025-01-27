using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationContextLinkedInteractable : InteractableBase
{
	public LocationContextLink simulationContext;

	private void OnEnable()
	{
		OnSetup(simulationContext);
	}

	protected virtual void OnSetup(LocationContextLink locationContext)
	{
		//Do nothing by default
	}
}
