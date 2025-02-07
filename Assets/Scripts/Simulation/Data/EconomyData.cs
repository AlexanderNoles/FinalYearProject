using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyData : DataModule
{
	public Shop market = new Shop();
	public float purchasingPower;

	public float EstimatedEconomyState()
	{
		return Mathf.Clamp(purchasingPower / 500.0f, 0.5f, 1.0f);
	}
}
