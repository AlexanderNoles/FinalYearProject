using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidRendering : MonoBehaviour
{
	//Object representing a loaded in chunk
	public class AsteroidChunk
	{
		public Transform root;
		public List<Transform> asteroids = new List<Transform>();
	}

	public List<AsteroidChunk> chunks = new List<AsteroidChunk>();
	private List<Vector3> offsets = new List<Vector3>();
	private const float scale = 500.0f;

	private void Start()
	{
		//Precompute offsets
		offsets.Clear();

		for (int x = -1; x < 2; x++)
		{
			for (int y = -1; y < 2; y++)
			{
				for (int z = -1; z < 2; z++)
				{
					offsets.Add(new Vector3(x, y, z) * scale);
				}
			}
		}
	}

	private void Update()
	{
		//Clamp current world center to grid
		//Convert that position to world space by getting it's offset from world center converted to a vector3
		RealSpacePosition cellCenterRS = WorldManagement.ClampPositionToGrid(WorldManagement.worldCenterPosition, 100);
		Vector3 cellCenter = WorldManagement.OffsetFromWorldCenter(cellCenterRS, Vector3.zero).AsVector3();

		Debug.DrawRay(cellCenter, Vector3.up, Color.red);
	}
}
