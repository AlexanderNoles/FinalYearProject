using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSimObjectBehaviour : SimObjectBehaviour
{
	private static List<ShipSimObjectBehaviour> registeredShips = new List<ShipSimObjectBehaviour>();
	private static Dictionary<Vector3, List<Vector3>> cellCenterToShips = new Dictionary<Vector3, List<Vector3>>();
	private static int lastFrameGenerated = -1;
	private const int cellSize = 15;

	private ShipCollectionDrawer collectionDrawer;

	private int modelPoolIndex;
	private Transform model;

	//Movement
	private Vector3 velocity;

	public void Init(ShipCollectionDrawer parent)
	{
		collectionDrawer = parent;

		//Set position
		transform.localPosition = parent.spawnInfo.GeneratePosition();

		if ((target as Ship).isWreck)
		{
			//Destroy rendered ship and place wreck there instead
			OnDeath(new TakenDamageResult());
		}
		else
		{
			//Match health
			currentHealth = (target as Ship).health;

			//If we are holding onto a model give it back
			//This can't be done in OnDisable because we can't set parents when activating or deactivating
			if (model != null)
			{
				GeneratorManagement.ReturnShipModel(modelPoolIndex, model);
				model = null;
			}

			//Get model
			modelPoolIndex = 0;
			model = GeneratorManagement.GetShipModel(modelPoolIndex);
			model.parent = transform;

			model.localPosition = Vector3.zero;
			//Me when the model has problems and issues
			model.localRotation = Quaternion.Euler(0, -90, 0);
		}
	}

	protected override TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		TakenDamageResult result = base.TakeDamage(rawDamageNumber, origin);

		if (target != null)
		{
			//Match health of ship and sim object
			(target as Ship).health = currentHealth;
		}

		return result;
	}

	private static Vector3 GetCellCenter(Vector3 pos)
	{
		return new Vector3(Mathf.RoundToInt(pos.x / cellSize), 0, Mathf.RoundToInt(pos.z / cellSize)) * cellSize;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		registeredShips.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		registeredShips.Remove(this);
	}

	protected override void Update()
	{
		//Run normal update loop
		base.Update();
		//

		//Check if data needs to be regenerated
		//Done by first ship rather than some management class so we can have the data centralized
		//Might need to exapnd it out into a seperate class if other systems need to use it frequently
		//Important that this is done after any damage is processed (in base.Update) as well as a ship
		//could be destroyed during damage application, which we don't want to bother spending time processing

		if (lastFrameGenerated != Time.frameCount)
		{
			//Update last frame generated so this only runs once this frame
			lastFrameGenerated = Time.frameCount;

			//Reset
			cellCenterToShips.Clear();

			foreach (ShipSimObjectBehaviour ship in registeredShips)
			{
				//Generate entry in cellCenterToShips

				//First get our key
				Vector3 pos = ship.transform.position;
				Vector3 cellCenter = GetCellCenter(pos);

				//Does an entry exist?
				if (!cellCenterToShips.ContainsKey(cellCenter))
				{
					cellCenterToShips.Add(cellCenter, new List<Vector3>());
				}

				//Add to created entry
				cellCenterToShips[cellCenter].Add(pos);
			}
		}
		//

		//Movement
		//Three primary vectors:
		//	Pathing Vector, points towards the destination
		//	Enviromental Avoidance Vector, iterates through surrounding "environmental objects", calculating a move away vector if neccesary
		//	Other ships Avoidance Vector, iterates through surrounding ships, calculating their influence on our movement
		//Vectors are distinct to allow specific mixing rules,


		// PATHING VECTOR //
		//Each from path towards our first target
		Vector3 pathingVector = Vector3.zero;

		if (currentTargets.Count > 0)
		{
			Vector3 targetPos = currentTargets[0].bb.transform.position;
			Vector3 displacement = targetPos - transform.position;

			//As we get close to the target this vector loses magnitude
			float slowDownFactor = 1.0f - Mathf.Clamp01((displacement.magnitude - 50.0f) / 100.0f);

			pathingVector = displacement.normalized * (1.0f - slowDownFactor);
		}
		//

		// ENVIRONMENTAL VECTOR //
		//Currently just iterating through all loaded environmental colliders, could use a spatial lookup table but because there are only 
		//two max at any time rn (27/02/2025) this is actually more efficient

		Vector3 environmentalVector = EnvironmentCollider.GetDisplacementFromColliders(transform.position);
		//

		// OTHER SHIPS AVOIDANCE VECTOR //
		//Iterate through all 9 surrounding cells, calculating if within some distance of a ship
		//If so apply some directional force
		Vector3 shipAvoidanceVector = Vector3.zero;
		Vector3 ourCellCenter = GetCellCenter(transform.position);
		const float shipAvoidanceMaxForce = 2.0f;
		const float shipAvoidanceDistance = 15.0f;
		bool foundOurself = false;

		foreach (Vector3 omniOffset in GenerationUtility.nineDirectionalOffsets)
		{
			Vector3 currentCellCenter = ourCellCenter + (omniOffset * cellSize);

			if (cellCenterToShips.ContainsKey(currentCellCenter))
			{
				foreach (Vector3 position in cellCenterToShips[currentCellCenter])
				{
					Vector3 displacement = position - transform.position;
					float mag = displacement.magnitude;

                    if (mag == 0)
                    {
						if (!foundOurself)
						{
							foundOurself = true;
						}
						else
						{
							displacement = Random.onUnitSphere;
							displacement.y = 0;
						}
                    }

                    float percentageDistance = Mathf.Pow(1.0f - Mathf.Clamp01(mag / shipAvoidanceDistance), 3.0f);

					//Negative because we want to move away from the other ship
					shipAvoidanceVector -= displacement.normalized * (percentageDistance * shipAvoidanceMaxForce);
				}
			}
		}
		//

		Vector3 velTarget = pathingVector + environmentalVector + shipAvoidanceVector;

		Debug.DrawRay(transform.position, shipAvoidanceVector, Color.cyan);

		velocity = Vector3.Lerp(velocity, velTarget, Time.deltaTime);


		//Apply
		transform.position += velocity * (10.0f * Time.deltaTime);

		if (velTarget.sqrMagnitude > 0)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velTarget, Vector3.up), Time.deltaTime);
		}
	}

	protected override void OnDeath(TakenDamageResult result)
	{
		base.OnDeath(result);
		//Stop drawing ship
		//Set ship to be destroyed sim side

		//(29/01/2025) something was causing this target to be null
		//Assumption is that the collection drawer unrendered at the same moment this ship was killed
		//a.k.a on death should not run if unlinked
		//Ideally the design would prevent this inherently but this is a better solution than a crash
		if (!Linked())
		{
			return;
		}

		(target as Ship).isWreck = true;

		//Tell parent collection drawer to not draw ship
		//Stop drawing ship
		//If has parent collection tell it to stop drawing this
		//Otherwise do it manually
		if (collectionDrawer == null)
		{
			GeneratorManagement.ReturnShip(this);
		}
		else
		{
			collectionDrawer.UndrawShip(this, true);
		}
	}
}
