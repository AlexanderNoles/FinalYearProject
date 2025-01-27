using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpInteractable : LocationContextLinkedInteractable
{
	private static WarpInteractable instance;

	public static WarpInteractable GetInstance()
	{
		return instance;
	}

	public static LocationContextLink GetWarpContext()
	{
		return instance.simulationContext;
	}

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		if (PlayerLocationManagement.GetWarp() == null)
		{
			return;
		}

		simulationContext.SetContext(PlayerLocationManagement.GetWarpLocation());
	}
}
