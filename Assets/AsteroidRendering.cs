using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AsteroidRendering : MonoBehaviour
{
	//Object representing a loaded in chunk
	public class AsteroidChunk
	{
		public Transform target;
		public int lastHash = -1;
		public Vector3 offset;

		public void SetTarget(Transform target)
		{
			this.target = target;
			target.localPosition = offset;
		}

		public bool HashChanged(int newHash)
		{
			return lastHash != newHash;
		}

		public int ComputeHash(RealSpacePosition center)
		{
			RealSpacePosition offsetPos = center.AddToClone(offset * WorldManagement.invertedInEngineWorldScaleMultiplier);

			return offsetPos.GetHashCode();
		}
	}

	private List<AsteroidChunk> chunks = new List<AsteroidChunk>();

	private List<Vector3> offsets = new List<Vector3>();
	private int centralChunkIndex;
	private const float scale = 5000.0f;
	private const float doubleScale = scale * 2;
	[HideInInspector]
	public new Transform transform;
	public MultiObjectPool pool;
	public Mesh testMesh;

	private void Awake()
	{
		transform = base.transform;
	}

	private void Start()
	{
		//Precompute offsets
		offsets.Clear();

		const int gridDimensions = 1;
		for (int x = -gridDimensions; x <= gridDimensions; x++)
		{
			for (int z = -gridDimensions; z <= gridDimensions; z++)
			{
				offsets.Add(new Vector3(x, 0.0f, z) * scale);
			}
		}

		//Generate needed amount of asteroid chunks (27)
		for (int i = 0; i < offsets.Count; i++)
		{
			//Create a transform that will hold asteroids
			Transform asteroidChunkLocalRoot = new GameObject("Asteroid Chunk").transform;
			asteroidChunkLocalRoot.parent = transform;

			//Generate actual chunk data
			AsteroidChunk chunk = new AsteroidChunk();
			chunk.offset = offsets[i];
			chunk.SetTarget(asteroidChunkLocalRoot);
			chunks.Add(chunk);

			if (chunk.offset.magnitude == 0.0f)
			{
				centralChunkIndex = i;
			}
		}
	}

	private void LateUpdate()
	{
		//Clamp current world center to grid
		//Convert that position to world space by getting it's offset from world center converted to a vector3
		RealSpacePosition cellCenterRS = WorldManagement.ClampPositionToGrid(WorldManagement.worldCenterPosition, scale * WorldManagement.invertedInEngineWorldScaleMultiplier);

		//Remove inaccuracy
		cellCenterRS.x = Math.Round(cellCenterRS.x * 2, MidpointRounding.AwayFromZero) / 2;
		cellCenterRS.z = Math.Round(cellCenterRS.z * 2, MidpointRounding.AwayFromZero) / 2;

		RealSpacePosition difference = cellCenterRS.SubtractToClone(WorldManagement.worldCenterPosition);
		
		Vector3 cellCenter = (difference.AsVector3() * WorldManagement.inEngineWorldScaleMultiplier) + PlayerCapitalShip.GetPosition();
		transform.position = cellCenter;

		//Compute central chunk index hash
		//If it is different than last recorded center hash then we have moved some amount of cells over
		if (chunks[centralChunkIndex].HashChanged(chunks[centralChunkIndex].ComputeHash(cellCenterRS)))
		{
			//Grab old hashes
			List<int> oldHashes = new List<int>();

			//Need to precompute hashes before changing anything so we need two loops
			foreach (AsteroidChunk chunk in chunks)
			{
				oldHashes.Add(chunk.lastHash);
			}

			//If any new hashes match old ones swap their target transforms, saving us the trouble of re-calculating the same chunks asteroid positions
			//We should just be able to give them the old chunk transform as all hash should be unique within neccesary ranges
			foreach (AsteroidChunk chunk in chunks)
			{
				//Compute new hash
				int newHash = chunk.ComputeHash(cellCenterRS);

				//Attempt to find newHash
				bool swapped = false;
				for (int i = 0; i < oldHashes.Count; i++)
				{
					if (oldHashes[i].Equals(newHash))
					{
						//Match found!
						swapped = true;

						//Swap transforms
						//Will update transform local positions to match offsets automaticallly
						Transform ourTransform = chunk.target;
						Transform theirTransform = chunks[i].target;

						chunk.SetTarget(theirTransform);
						chunks[i].SetTarget(ourTransform);

						break;
					}
				}

				//Update last hash to match new one
				chunk.lastHash = newHash;

				if (!swapped)
				{
					//No swap occured, actually generate the new positions

					//Return all children to the pool
					for (int i = 0; i < chunk.target.childCount;)
					{
						pool.ReturnObject(0, chunk.target.GetChild(0));
					}

					//Generate new positions and place asteroids at them
					//Seed rng based on hash
					System.Random random = new System.Random(newHash);

					int count = random.Next(1, 25);
					for (int i = 0; i < count; i++)
					{
						Transform newAsteroid = pool.GetObject(0).transform;
						newAsteroid.gameObject.SetActive(true);
						newAsteroid.parent = chunk.target;

						newAsteroid.localPosition = GenerateChunkPos(random, out Vector3 yDirection);
						Vector3 asteriodScale = new Vector3(
							Mathf.Lerp(5, 100, GeneratePercentage(random, res)),
							Mathf.Lerp(5, 100, GeneratePercentage(random, res)),
							Mathf.Lerp(5, 100, GeneratePercentage(random, res))
							);
						newAsteroid.localScale = asteriodScale;
						newAsteroid.localRotation = Quaternion.Euler(asteriodScale);

						newAsteroid.localPosition += yDirection * (15.0f + asteriodScale.y);
					}
				}
			}
		}
	}

	const int res = 10000;
	private Vector3 GenerateChunkPos(System.Random random, out Vector3 yDirection)
	{
		Vector3 pos = new Vector3(
			Mathf.Lerp(-scale, scale, GeneratePercentage(random, res)),
			Mathf.Lerp(-scale, scale, GeneratePercentage(random, res)),
			Mathf.Lerp(-scale, scale, GeneratePercentage(random, res))
			);

		yDirection = (Vector3.up * pos.y).normalized;

		//Move the position away from x-z plane
		return pos;
	}

	private float GeneratePercentage(System.Random random, int res)
	{
		return random.Next(0, res + 1) / (float)res;
	}
}
