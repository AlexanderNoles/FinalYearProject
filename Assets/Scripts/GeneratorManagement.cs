using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratorManagement : MonoBehaviour
{
	public static GeneratorManagement _instance;

	//HELPER INDEXES
	public enum STRUCTURES_INDEXES
	{
		SETTLEMENT = 0
	}

	public MultiObjectPool structuresPool;

	private void Awake()
	{
		_instance = this;
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
		//We use linked lists so generators can easily be combined
		public LinkedList<(int, Transform)> targets = new LinkedList<(int, Transform)>();
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
		public StructureGeneration SpawnStructure(STRUCTURES_INDEXES index, Vector3 localPos)
		{
			Transform newTarget = GetStructure((int)index);
			newTarget.parent = parent;
			newTarget.localPosition = localPos;

			targets.AddLast(((int)index, newTarget));

			return this;
		}
	}
}
