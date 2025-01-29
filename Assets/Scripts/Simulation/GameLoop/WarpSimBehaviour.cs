using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpSimBehaviour : SimObjectBehaviour
{
	private static WarpSimBehaviour instance;

	protected override void Awake()
	{
		instance = this;
		base.Awake();
	}

	protected override void OnEnable()
	{
		Link(PlayerLocationManagement.GetWarpLocation());

		base.OnEnable();
	}

	public static WarpSimBehaviour GetInstance()
	{
		return instance;
	}
}
