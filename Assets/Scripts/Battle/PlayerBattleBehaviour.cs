using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleBehaviour : BattleBehaviour
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

	public List<Vector3> firePoints = new List<Vector3>();

	private const float maxSelectDistance = 500;
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

	protected override Vector3 GetFireFromPosition(Vector3 targetPos)
	{
		//Find the smallest angle between the look direction and the displacement of the target position from the fire point

		Vector3 pos = transform.position;
		Vector3 right = transform.right;
		Vector3 forward = transform.forward;
		Vector3 up = transform.up;

		Vector3 firePos = pos;
		float currentMinimum = float.MaxValue;

		for (int i = 0; i < firePoints.Count; i++)
		{
			//Make fire point non-local
			Vector3 calculatedFirePoint = 
				(right * firePoints[i].x) + 
				(forward * firePoints[i].z) + 
				(up * firePoints[i].y);

			Vector3 lookDirection;
			if (Mathf.Abs(firePoints[i].x) > Mathf.Abs(firePoints[i].z))
			{
				//X component is dominant
				lookDirection = right * (firePoints[i].x / Mathf.Abs(firePoints[i].x));
			}
			else
			{
				//Z component is dominamt
				lookDirection = forward * (firePoints[i].z / Mathf.Abs(firePoints[i].z));
			}

			//Make fire point non local
			Vector3 calculatedOrigin = calculatedFirePoint + pos;

			Vector3 displacement = targetPos - calculatedOrigin;

			//Angle between 
			float angle = Mathf.Abs(Vector3.Angle(lookDirection, displacement));

			if (angle < currentMinimum)
			{
				currentMinimum = angle;
				firePos = calculatedOrigin;
			}
		}

		return firePos;
	}

	private void Update()
	{
		//Clamp current health to max health
		//This is dirty but flexible
		PlayerStats playerStats = PlayerManagement.GetStats();

		float maxHealth = 100;
		if (playerStats != null)
		{
			maxHealth = playerStats.GetStat(Stats.maxHealth.ToString());
        }

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        //

        float salvoAmountThisFrame = GetSalvoPercentage();
		if (salvoAmountThisFrame != lastRecordedSalvoPercentage)
		{
			lastRecordedSalvoPercentage = salvoAmountThisFrame;
			MainInfoUIControl.UpdateSalvoBarInensity(lastRecordedSalvoPercentage);
		}

		//Process all current targets
		ProcessTargets();
	}

	protected override void OnAddTarget(BattleBehaviour newTarget)
	{
		//Notify targets UI
		CurrentTargetsUI.AddTarget(newTarget);
	}

	protected override void OnRemoveTarget(BattleBehaviour target)
	{
		//Notify targets UI
		CurrentTargetsUI.RemoveTarget(target);
	}

	protected override void OnClearTargets(List<BattleBehaviour> targetsBefore)
	{
		//Just run on remove target for all of them
		foreach (BattleBehaviour bb in targetsBefore)
		{
			OnRemoveTarget(bb);
		}
	}
}
