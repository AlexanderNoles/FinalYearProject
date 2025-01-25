using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitableLocationBattleContextLink : BattleContextLink
{
	public LocationContextLink simulationContext;
	public BattleBehaviour targetBattleBehaviour;

	public override float GetMaxHealth()
	{
		if (simulationContext == null || simulationContext.target == null || !simulationContext.target.HasHealth())
		{
			return base.GetMaxHealth();
		}

		return simulationContext.target.GetMaxHealth();
	}

	public override void OnDeath()
	{
		if (simulationContext == null || simulationContext.target != null)
		{
			simulationContext.target.OnDeath();
		}
	}

	public override int GetEntityID()
	{
		if (simulationContext == null || simulationContext.target == null)
		{
			return -1;
		}

		return simulationContext.target.GetEntityID();
	}

	private void OnEnable()
	{
		if (simulationContext == null || simulationContext.target == null)
		{
			return;
		}

		List<WeaponBase> weapons = simulationContext.target.GetWeapons();

		foreach (WeaponBase weapon in weapons)
		{
			//Add battle behaviour weapons
			ContextLinkedWeaponProfile wp = new ContextLinkedWeaponProfile().SetTarget(weapon);
			wp.MarkLastAttackTime(Time.time);
			targetBattleBehaviour.weapons.Add(wp);
		}
	}
}
