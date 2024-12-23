using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProfile
{
	public virtual float GetDamage()
	{
		return 10;
	}

	public virtual float ShotsPerAttack()
	{
		return 1.0f;
	}

	public virtual float GetTimeBetweenAttacks()
	{
		return 0.1f;
	}

	public virtual float GetRange()
	{
		return 200.0f;
	}

	protected virtual bool SalvoEnabled()
	{
		return true;
	}

	public virtual void OnBattleStart()
	{
		//When weapons begin firing
		if (!SalvoEnabled())
		{
			MarkLastAttackTime(Time.time);
		}
	}

	//This is the amount of stored up attacks the weapon can store
	//All of these will be shot at once on engage
	//So players will need to think about who they attack first
	public virtual int InitialSalvoSize()
	{
		return 10;
	}

	protected float lastAttackTime;
	public void MarkLastAttackTime(float time)
	{
		lastAttackTime = time;
	}

	public int CaclculateNumberOfAttacks()
	{
		int calculatedNumberOfAttacks = Mathf.FloorToInt(((Time.time - lastAttackTime) / GetTimeBetweenAttacks()) * Mathf.Max(ShotsPerAttack(), 1.0f));

		int maxSalvoSize = InitialSalvoSize();
		if (calculatedNumberOfAttacks > maxSalvoSize)
		{
			calculatedNumberOfAttacks = maxSalvoSize;
		}

		return calculatedNumberOfAttacks;
	}
}
