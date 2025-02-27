using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProfile
{
	public virtual float GetDamage()
	{
		return 0.5f;
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

	protected virtual bool AttackTimeVariance()
	{
		return false;
	}

	public virtual void OnBattleStart()
	{
		//When weapons begin firing
		ResetTimerValue();
	}

	protected float lastAttackTime;
	public void MarkLastAttackTime(float time)
	{
		if (AttackTimeVariance())
		{
			time += Random.Range(0.0f, GetTimeBetweenAttacks() * 0.25f);
		}

		lastAttackTime = time;
	}

	protected float timeTillNextAttack;

	public int CaclculateNumberOfAttacks()
	{
		return 1 + Mathf.FloorToInt(Mathf.Abs(timeTillNextAttack) / GetTimeBetweenAttacks());
	}

	public void MarkWeaponFired()
	{
		timeTillNextAttack = GetTimeBetweenAttacks();
	}

	public bool CanFire()
	{
		return timeTillNextAttack <= 0.0f;
	}

	public void CountDownTimer()
	{
		//If currently can't fire
		if (!CanFire())
		{
			timeTillNextAttack -= Time.deltaTime;
		}
	}

	public void ClampTimerValue()
	{
		timeTillNextAttack = Mathf.Max(timeTillNextAttack, 0.0f);
	}

	public void ResetTimerValue()
	{
		timeTillNextAttack = GetTimeBetweenAttacks();

		if (AttackTimeVariance())
		{
			float variance = timeTillNextAttack * 0.05f;

			timeTillNextAttack += Random.Range(-variance, variance);
		}
	}
}
