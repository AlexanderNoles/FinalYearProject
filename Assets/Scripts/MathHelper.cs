using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static float ValueTanhFalloff(float input, float modifier = 1, float additionalHorizontalStretch = 1)
	{
		modifier = Mathf.Max(modifier, 0);

		return modifier * (float)System.Math.Tanh(input / (modifier + 1 + additionalHorizontalStretch));
	}
}
