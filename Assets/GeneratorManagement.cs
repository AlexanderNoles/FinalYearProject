using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratorManagement : MonoBehaviour
{
	public static GeneratorManagement _instance;

	//HELPER INDEXES
	public enum PRIMITIVE_INDEXES
	{
		CUBE
	}

	public MultiObjectPool primitivePool;

	private void Awake()
	{
		_instance = this;
	}

	public static Transform GetPrimitive(int index) 
	{
		return _instance.primitivePool.GetObject(index).transform;
	}

	public static void ReturnPrimitive(Transform target, int index)
	{
		_instance.primitivePool.ReturnObject(index, target);
	}

	//This script should contain generator functions that apply some effect to their list of objects
	//These objects should typically be primitives (cubes, spheres, etc.) but because these functions will work on transforms 
	//Imagine the sorta chaining you get in shader graph and it will make more sense

	//Generator functions are split into two distinct phases
	//Init generations that return a GenerationInit
	//and Generation that return just a Generation
	//GenerationInit is a child of Generation so GenerationInit can call all the Generation functions
	//Calling one of these functions is the way to end the init phase.
	//If you really need to go back to the init phase the function ForceBackToInit can be called.
	//When the generation chain is done, FinalizeGeneration can be called. This will set all the objects visible.
	public class Generation 
	{
		//We use linked lists so generators can easily be combined
		public LinkedList<(int, Transform)> targets = new LinkedList<(int, Transform)>();
		public int currentPrimitveIndex = (int)PRIMITIVE_INDEXES.CUBE;

		public Generation SetTargetPrimitive(PRIMITIVE_INDEXES newIndex)
		{
			currentPrimitveIndex = (int)newIndex;

			return this;
		}

		// Some helper functions //

		public void FinalizeGeneration()
		{
			foreach ((int, Transform) t in targets)
			{
				t.Item2.gameObject.SetActive(true);
			}
		}

		public void AutoCleanup()
		{
			foreach((int, Transform) t in targets)
			{
				ReturnPrimitive(t.Item2, t.Item1);
			}
		}

		/// <summary>
		/// Force the generation chain back to init phase
		/// This is not typically neccesarry so think about what you are doing
		/// </summary>
		/// <returns>This generation as GenerationInit</returns>
		public GenerationInit ForceBackToInit()
		{
			return this as GenerationInit;
		}

		public Generation Concat(Generation other)
		{
			Concat(other.targets);
			return this;
		}

		public Generation Concat(LinkedList<(int, Transform)> otherList)
		{
			foreach ((int, Transform) entry in otherList)
			{
				targets.AddLast(entry);
			}
			return this;
		}

		public Generation Replace(Generation other)
		{
			targets = other.targets;
			return this;
		}

		// Modification functions //
		public Generation MultiplySize(Vector3 modifier)
		{
			foreach ((int, Transform) entry in targets)
			{
				Vector3 scale = entry.Item2.localScale;

				scale.x *= modifier.x;
				scale.y *= modifier.y;
				scale.z *= modifier.z;

				entry.Item2.localScale = scale;
			}

			return this;
		}

		public Generation SetSize(Vector3 scale)
		{
			foreach ((int, Transform) entry in targets)
			{
				entry.Item2.localScale = scale;
			}

			return this;
		}

		public Generation Array(Vector3 arrayOffset, bool offsetRelative, int count)
		{
			LinkedList<(int, Transform)> newTargets = new LinkedList<(int, Transform)>();
			foreach ((int, Transform) entry in targets) 
			{
				Vector3 offset = arrayOffset;

				if (offsetRelative)
				{
					offset.x *= entry.Item2.localScale.x;
					offset.y *= entry.Item2.localScale.y;
					offset.z *= entry.Item2.localScale.z;
				}

				Vector3 spawnPos = entry.Item2.position;

				for (int i = 0; i < count; i++)
				{
					spawnPos += offset;
					Transform newPrim = GetPrimitive(currentPrimitveIndex);

					newPrim.position = spawnPos;
					newPrim.localScale = entry.Item2.localScale;

					newTargets.AddLast((currentPrimitveIndex, newPrim));
				}
			}

			Concat(newTargets);
			return this;
		}

		public Generation MultiplySize(float modifier)
		{
			return MultiplySize(Vector3.one * modifier);
		}
	}

	public class GenerationInit : Generation
	{
		public GenerationInit AtSpot(Vector3 worldPos, int count) 
		{
			for (int i = 0; i < count; i++)
			{
				AtSpot(worldPos);
			}

			return this;
		}

		public GenerationInit AtSpot(Vector3 worldPos)
		{
			Transform newTarget = GetPrimitive(currentPrimitveIndex);
			newTarget.position = worldPos;

			targets.AddLast((currentPrimitveIndex, newTarget));

			return this;
		}
	}
}
