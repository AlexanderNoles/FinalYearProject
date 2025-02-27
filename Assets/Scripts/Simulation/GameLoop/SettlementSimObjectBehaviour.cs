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
	public float roadIntensity = 0.5f; 
	public float roadLinesFrequency = 5.0f;
	public List<Mesh> meshs;
	public Material material;
	private List<InstancedRenderer> renderers = new List<InstancedRenderer>();

	public Transform lowerCityBase;
	public Transform atmosphere;
	public MeshRenderer atmosphereRenderer;
	public MultiObjectPool additionalDecorationsPool;

	public override void OnLink()
	{
		base.OnLink();

		RegenerateSurroundings();
	}

	private void RegenerateSurroundings()
	{
		renderers.Clear();
		List<List<Matrix4x4>> generatedPositions = new List<List<Matrix4x4>>();

		foreach (Mesh mesh in meshs)
		{
			renderers.Add(new InstancedRenderer()
			{
				mesh = mesh,
				material = material
			});

			generatedPositions.Add(new List<Matrix4x4>());
		}

		//Set to a specific state so settlements are generated consistently
		SettlementsData.Settlement.SettlementLocation set = (SettlementsData.Settlement.SettlementLocation)target;
		Random.InitState(set.GetPosition().GetHashCode());
		atmosphereRenderer.material.SetVector("_RealSpacePosition", set.GetPosition().AsTruncatedVector3(10000.0f));

		//Generate scale
		float size = 300;// Random.Range(175, 300);
		lowerCityBase.localScale = new Vector3(size, 250.0f, size);
		atmosphere.localScale = new Vector3(size + 50, 65.0f, size + 50);

		//Generate city scape positions
		Vector3 constantOffset = Vector3.down * 60;

		float min = 150;
		float distancePer = 3;
		float max = size - 20.0f;

		int count = Mathf.FloorToInt((max - min) / distancePer);

		for (int i = -1; i < count; i++)
		{
			float circleRadius;
			int countPerIteration = 20;
			float randomOffsetMinMax = (1.0f / countPerIteration) * 2.0f;

			if (i == -1)
			{
				circleRadius = 75;
				countPerIteration += 10;
				randomOffsetMinMax /= 4;
			}
			else
			{
				circleRadius = min + (distancePer * i);
			}

			//Iterate around in a circle
			//Adding positions
			float inverseRLF = 1.0f / roadLinesFrequency;

			for (int p = 0; p < countPerIteration; p++)
			{
				float percentage = (p / (float)countPerIteration) + Random.Range(-randomOffsetMinMax, randomOffsetMinMax);
				//Round percentage to the nearest arbitrary decimal
				//Get the difference
				//Add that to percentage multiplied by some constant
				float rounded = Mathf.RoundToInt(percentage / inverseRLF) * inverseRLF;

				float offsetPercentage = percentage + ((rounded - percentage) * roadIntensity);
				Vector3 pos = GenerationUtility.GetCirclePositionBasedOnPercentage(offsetPercentage, circleRadius);

				float highUpChance = Mathf.Pow(Random.Range(0.0f, 1.0f), 6.0f);

				pos += Vector3.down * (Random.Range(0.0f, 5.0f) + Mathf.Lerp(1.0f, 25.0f, Mathf.Max(i, 0.0f) / (float)count) + (highUpChance * -15.0f));
				pos += constantOffset;

				Matrix4x4 newMatrix = Matrix4x4.Translate(pos);

				//Add to random building instanced renderer
				int index = Random.Range(0, generatedPositions.Count);

				generatedPositions[index].Add(newMatrix);
			}
		}

		//Set cennter
		const bool centerGeneration = false;

		if (centerGeneration)
		{
			int centerBuildingCount = Random.Range(3, 8);

			for (int i = 0; i < centerBuildingCount; i++)
			{
				float percentage = i / (float)centerBuildingCount;

				Vector3 pos = GenerationUtility.GetCirclePositionBasedOnPercentage(percentage, 10);
				pos += Vector3.up * 20.0f;


				Matrix4x4 newMatrix = Matrix4x4.Translate(pos);

				//Add to random building instanced renderer
				int index = Random.Range(0, generatedPositions.Count);

				generatedPositions[index].Add(newMatrix);
			}
		}

		//Apply to renderers
		for (int i = 0; i < generatedPositions.Count; i++)
		{
			renderers[i].SetInitalData(generatedPositions[i].ToArray());
		}

		//Reset decorations
		additionalDecorationsPool.PruneObjectsNotUpdatedThisFrame(0, true);
	}

	private void LateUpdate()
	{
		//Instanced rendering for city scape
		Matrix4x4 localToWorld = Matrix4x4.Translate(transform.parent.position);

		foreach (InstancedRenderer renderer in renderers)
		{
			renderer.Update(localToWorld);
			renderer.Render();
		}
	}
}
