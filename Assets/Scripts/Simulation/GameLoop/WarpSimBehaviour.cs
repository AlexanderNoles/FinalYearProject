using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpSimBehaviour : SimObjectBehaviour
{
	private static WarpSimBehaviour instance;

	protected override void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		target = PlayerLocationManagement.GetWarpLocation();
	}

	public static WarpSimBehaviour GetInstance()
	{
		return instance;
	}
}
