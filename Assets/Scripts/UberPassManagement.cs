using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MonitorBreak.InitializeAtRuntime("UberPassManagement")]
public class UberPassManagement : MonoBehaviour
{
	public Material mat;
	private static UberPassManagement instance;

	private void Awake()
	{
		instance = this;
		EnableOutlines(true);
	}

	private void OnLevelWasLoaded(int level)
	{
		EnableOutlines(true);
	}

	public static void EnableOutlines(bool active)
	{
		if (instance == null)
		{
			return;
		}

		instance.mat.SetInt("_OutlinesEnabled", active ? 1 : 0);
	}
}
