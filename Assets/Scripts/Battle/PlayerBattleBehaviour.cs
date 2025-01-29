using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleBehaviour : SimObjectBehaviour
{
	private static PlayerBattleBehaviour instance;

	public static bool IsPlayerBB(BattleBehaviour bb)
	{
		return instance != null && instance.Equals(bb);
	}

	public static void ToggleTargetExternal(BattleBehaviour target)
	{
		instance.ToggleTarget(target);
	}

	public static void ClearAllTargetsExternal()
	{
		instance.ClearTargets();
	}

	public static float GetSalvoPercentage()
	{
		return Mathf.Clamp01(instance.weapons[0].CaclculateNumberOfAttacks() / (float)instance.weapons[0].InitialSalvoSize());
	}

	public static float GetCurrentHealth()
	{
		if (instance == null)
		{
			return 0.0f;
		}

		return instance.currentHealth;
	}

	public override int TryGetEntityID()
	{
		if (!PlayerManagement.PlayerEntityExists())
		{
			return base.TryGetEntityID();
		}

		return PlayerManagement.GetTarget().id;
	}

	private float lastRecordedSalvoPercentage;

	protected override void Awake()
	{
		instance = this;
		currentHealth = 100.0f;
		base.Awake();

		//DEBUG
		//Add test weapon
		weapons.Add(new WeaponProfile());
		lastRecordedSalvoPercentage = -1;
    }

	public override float GetMaxHealth()
	{
		PlayerStats playerStats = PlayerManagement.GetStats();
		float maxHealth = 100;

		if (playerStats != null)
		{
			maxHealth = playerStats.GetStat(Stats.maxHealth.ToString());
		}

		return maxHealth;
	}

	public override float GetRegenPerTick()
	{
		if (!PlayerManagement.PlayerEntityExists())
		{
			return base.GetRegenPerTick();
		}

		return Mathf.Max(1.0f, PlayerManagement.GetStats().GetStat(Stats.healthRegen.ToString()));
	}

	protected override void Update()
	{
		base.Update();

        float salvoAmountThisFrame = GetSalvoPercentage();
		if (salvoAmountThisFrame != lastRecordedSalvoPercentage)
		{
			lastRecordedSalvoPercentage = salvoAmountThisFrame;
			MainInfoUIControl.UpdateSalvoBarInensity(lastRecordedSalvoPercentage);
		}
	}

	public override void DoTicks(int tickCount)
	{
		base.DoTicks(tickCount);

		MainInfoUIControl.ForceHealthBarRedraw();
	}

	protected override void OnAddTarget(Target newTarget)
	{
		//Notify targets UI
		CurrentTargetsUI.AddTarget(newTarget.bb);
	}

	protected override void OnRemoveTarget(Target target)
	{
		//Call raw method
		OnRemoveTargetRaw(target.bb);
	}

	protected override void OnRemoveTargetRaw(BattleBehaviour target)
	{
		//Notify targets UI
		CurrentTargetsUI.RemoveTarget(target);
	}

	protected override TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		TakenDamageResult result = base.TakeDamage(rawDamageNumber, origin);

		//Update ui
		MainInfoUIControl.ForceHealthBarRedraw();

		return result;
	}

	protected override void OnDeath()
	{
		PlayerManagement.KillPlayer();
	}

	protected override void OnClearTargets(List<Target> targetsBefore)
	{
		//Just run on remove target for all of them
		foreach (Target bb in targetsBefore)
		{
			OnRemoveTarget(bb);
		}
	}
}
