using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratorManagement : MonoBehaviour
{
	public static GeneratorManagement _instance;
	public List<Mesh> asteroidMeshes = new List<Mesh>();
	public MultiObjectPool shipModelsPool;

	//HELPER INDEXES
	public enum POOL_INDEXES
	{
		SETTLEMENT = 0,
		ASTEROID = 1,
		SHIP = 2,
		PIRATEBASE = 3
	}

	public MultiObjectPool structuresPool;
	private Dictionary<Transform, MeshFilter> asteroidToMeshFilter = new Dictionary<Transform, MeshFilter>();

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		asteroidToMeshFilter = structuresPool.GetComponentsOnAllActiveObjects<MeshFilter>((int)POOL_INDEXES.ASTEROID);
	}

	public static void SetOffset(Vector3 offset)
	{
		_instance.transform.position = offset;
	}

	public static Transform GetStructure(int index)
	{
		return _instance.structuresPool.GetObject(index).transform;
	}

	public static void ReturnStructure(int index, Transform transform)
	{
		_instance.structuresPool.ReturnObject(index, transform);
	}

	public class Generation 
	{
		public List<(int, Transform)> targets = new List<(int, Transform)>();
		public Transform parent;

		public void AutoCleanup()
		{
			foreach ((int, Transform) t in targets)
			{
				ReturnStructure(t.Item1, t.Item2);
			}

			targets.Clear();
		}

		public void FinalizeGeneration()
		{
			foreach ((int, Transform) t in targets)
			{
				t.Item2.gameObject.SetActive(true);
			}
		}
	}

	public class StructureGeneration : Generation
	{
		public StructureGeneration SpawnStructure(POOL_INDEXES index, Vector3 localPos)
		{
			Transform newTarget = GetStructure((int)index);
			newTarget.parent = parent;
			newTarget.localPosition = localPos;

			targets.Add(((int)index, newTarget));

			return this;
		}
	}

	public class AsteroidGeneration : Generation
	{
		public AsteroidGeneration SpawnAsteroid(Vector3 localPos)
		{
			Transform newTarget = GetStructure((int)POOL_INDEXES.ASTEROID);
			newTarget.parent = parent;
			newTarget.localPosition = localPos;

			targets.Add(((int)POOL_INDEXES.ASTEROID, newTarget));

			MeshFilter meshFilter = _instance.asteroidToMeshFilter[newTarget];

			meshFilter.mesh = _instance.asteroidMeshes[0];

			return this;
		}
	}

	public static ShipSimObjectBehaviour DrawShip(Vector3 initalPos)
	{
		return _instance.structuresPool.SpawnObject<ShipSimObjectBehaviour>(POOL_INDEXES.SHIP, initalPos).component;
	}

	public static void ReturnShip(ShipSimObjectBehaviour ship)
	{
		_instance.structuresPool.ReturnObject((int)POOL_INDEXES.SHIP, ship.transform);
	}

	public static Transform GetShipModel(int index)
	{
		return _instance.shipModelsPool.SpawnObject(index, Vector3.zero).transform;
	}

	public static void ReturnShipModel(int index, Transform transform)
	{
		_instance.shipModelsPool.ReturnObject(index, transform);
	}
}
