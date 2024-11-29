using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliticalData : DataBase
{
	//SQRT OF 8, or sqrt(2^2 + 2^2)
	public const float maxDistance = 2.82842712475f;

	public float economicAxis;
	public float authorityAxis;

	public float CalculatePoliticalDistance(float eco, float auth)
	{
		float ecoDiff = Mathf.Abs(eco - economicAxis);
		float authDiff = Mathf.Abs(auth - authorityAxis);

		return Mathf.Sqrt((ecoDiff * ecoDiff) + (authDiff * authDiff));
	}
}
