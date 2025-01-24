using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipDrawer : BattleContextLink
{
	[HideInInspector]
	public new Transform transform;
	public Ship target = null;
	private SimulationEntity parentEntity;
	private ShipCollectionDrawer collectionDrawer;

	private int modelPoolIndex;
	private Transform model;

	public BattleBehaviour bb;

	private void Awake()
	{
		transform = base.transform;
	}

	private void OnDisable()
	{
		//Clear battle behaviour weapons
		bb.weapons.Clear();

		target = null;
	}

	public void Link(Ship newTarget)
	{
		target = newTarget;
	}

	public void SetParent(Transform newParent)
	{
		transform.parent = newParent;
	}

	public void Init(SimulationEntity entity, ShipCollectionDrawer parent)
	{
		collectionDrawer = parent;
		parentEntity = entity;

		//Set position
		Vector3 newShipPosition = Random.onUnitSphere;
		newShipPosition.y = 0.0f;
		newShipPosition.Normalize();

		transform.position = newShipPosition * Random.Range(10.0f, 100.0f);

		if (target.isWreck)
		{
			//Destroy rendered ship and place wreck there instead
			OnDeath();
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
			model.localRotation = Quaternion.identity;

			//Add battle behaviour weapons
			ContextLinkedWeaponProfile wp = new ContextLinkedWeaponProfile().SetTarget(target);
			wp.MarkLastAttackTime(Time.time);
			bb.weapons.Add(wp);
		}
	}

	// BATTLE CONTEXT LINK //

	public override int GetEntityID()
	{
		return parentEntity.id;
	}

	public override float GetMaxHealth()
	{
		if (target == null)
		{
			return base.GetMaxHealth();
		}

		return target.GetMaxHealth();
	}

	public override void OnDeath()
	{
		//Stop drawing ship
		//Set ship to be destroyed sim side
		target.isWreck = true;

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
