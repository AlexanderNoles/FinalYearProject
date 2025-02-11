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

	[Header("City Scape Rendering")]
	public Mesh mesh;
	public Material material;
	private Matrix4x4[] cityBlockData;
	private Matrix4x4[] updatedCityBlockData;
	public Transform lowerCityBase;

	public override void OnLink()
	{
		base.OnLink();

		RegenerateCityBlocks();
	}

	private void RegenerateCityBlocks()
	{
		//Set to a specific state so settlements are generated consistently
		SettlementsData.Settlement.SettlementLocation set = (SettlementsData.Settlement.SettlementLocation)target;
		Random.InitState(set.GetPosition().GetHashCode());

		//Generate scale
		float size = Random.Range(175, 300);
		lowerCityBase.localScale = new Vector3(size, 250.0f, size);

		//Generate city scape positions
		Vector3 constantOffset = Vector3.down * 60;
		List<Matrix4x4> generatedPositions = new List<Matrix4x4>();

		float min = 150;
		float distancePer = 5;
		float max = size - 20.0f;

		int count = Mathf.FloorToInt((max - min) / distancePer);

		for (int i = 0; i < count; i++)
		{
			//Iterate around in a circle
			//Adding positions
			float circleRadius = min + (distancePer * i);
			int countPerIteration = 20;

			float randomOffsetMinMax = (1.0f / countPerIteration) * 2.0f;

			for (int p = 0; p < countPerIteration; p++)
			{
				Vector3 pos = GenerationUtility.GetCirclePositionBasedOnPercentage((p / (float)countPerIteration) + Random.Range(-randomOffsetMinMax, randomOffsetMinMax), circleRadius);

				float highUpChance = Mathf.Pow(Random.Range(0.0f, 1.0f), 6.0f);

				pos += Vector3.down * (Random.Range(0.0f, 5.0f) + Mathf.Lerp(1.0f, 25.0f, i / (float)count) + (highUpChance * -25.0f));
				pos += constantOffset;

				Matrix4x4 newMatrix = Matrix4x4.Translate(pos);

				generatedPositions.Add(newMatrix);
			}
		}

		cityBlockData = generatedPositions.ToArray();
		updatedCityBlockData = new Matrix4x4[cityBlockData.Length];
	}

	private void LateUpdate()
	{
		//Instanced rendering for city scape
		Matrix4x4 localToWorld = Matrix4x4.Translate(transform.parent.position);
		RenderParams rp = new RenderParams(material);
		rp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		for (int i = 0; i < updatedCityBlockData.Length; i++)
		{
			updatedCityBlockData[i] = cityBlockData[i] * localToWorld;
		}

		Graphics.RenderMeshInstanced(rp, mesh, 0, updatedCityBlockData);
	}
}
