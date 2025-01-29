using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementSimObjectBehaviour : SimObjectBehaviour
{
	[Header("Settlement Specific Settings")]
	public int firePointCount = 10;
	public float firePointRadius = 25;
	public Vector3 firePointsOffset = Vector3.zero;

	[ContextMenu("Generate Fire Points")]
	public void GeneratePoints()
	{
		firePoints.Clear();

		for (int i = 0; i < firePointCount; i++)
		{
			//Generate position along circle
			float percentage = ((float)i / firePointCount) * 2 * Mathf.PI;

			Vector3 pos = new Vector3();
			pos.x = Mathf.Cos(percentage);
			pos.z = Mathf.Sin(percentage);

			pos *= firePointRadius;

			firePoints.Add(pos + firePointsOffset);
		}
	}
}
