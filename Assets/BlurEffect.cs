using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BlurEffect : MonoBehaviour
{
	private static Volume target;

	private void Awake()
	{
		target = GetComponent<Volume>();
	}

	public static void SetEffectIntensity(float newIntensity)
	{
		if (target == null)
		{
			return;
		}

		target.weight = newIntensity;
		UberPassManagement.EnableOutlines(newIntensity < 0.5f);
	}
}
