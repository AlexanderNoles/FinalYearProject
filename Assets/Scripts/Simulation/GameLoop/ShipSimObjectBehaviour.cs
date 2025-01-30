using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSimObjectBehaviour : SimObjectBehaviour
{
	private ShipCollectionDrawer collectionDrawer;

	private int modelPoolIndex;
	private Transform model;

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

	public void Init(ShipCollectionDrawer parent)
	{
		collectionDrawer = parent;

		//Set position
		Vector3 newShipPosition = Random.onUnitSphere;
		newShipPosition.y = 0.0f;
		newShipPosition.Normalize();

		transform.position = newShipPosition * Random.Range(10.0f, 100.0f);

		if ((target as Ship).isWreck)
		{
			//Destroy rendered ship and place wreck there instead
			OnDeath(new TakenDamageResult());
		}
		else
		{
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
			model.localRotation = Quaternion.Euler(0, Random.Range(0, 360.0f), 0);
		}
	}
}
