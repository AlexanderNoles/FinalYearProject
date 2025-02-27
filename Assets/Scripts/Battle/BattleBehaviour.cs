using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
	[Header("Battle Settings")]
	public bool battleEnabled = true;
	public Collider targetCollider;
	public bool autoFindTargets = true;
	public bool autoRetaliate = true;

	[HideInInspector]
	public new Transform transform;

	[HideInInspector]
	public new GameObject gameObject;

	[HideInInspector]
	public List<WeaponProfile> weapons = new List<WeaponProfile>();

	[System.Serializable]
	public class Target
	{
		public enum TargetType
		{
			Normal,
			Maintained
		}

		public BattleBehaviour bb;
		public TargetType type;
	}

	public List<Target> currentTargets = new List<Target>();

	private bool Targeting(BattleBehaviour bb, out int index)
	{
		index = -1;
		foreach (Target target in currentTargets)
		{
			index++;
			if (target.bb.Equals(bb))
			{
				return true;
			}
		}

		return false;
	}

    protected float currentHealth;

    protected virtual void Awake()
	{
		transform = base.transform;
		gameObject = base.gameObject;
	}

	protected virtual void OnEnable()
	{
		if (battleEnabled)
		{
			BattleManagement.RegisterBattleBehaviour(targetCollider, this);
		}
	}

	protected virtual void OnDisable()
	{
		if (battleEnabled)
		{
			BattleManagement.DeRegisterBattleBehaviour(targetCollider);
		}

		//Do full clear
		currentTargets.Clear();
	}

	public virtual float GetHealthPercentage()
	{
		return Mathf.Clamp01(currentHealth / GetMaxHealth());
	}

	public virtual float GetMaxHealth()
	{
		return 100.0f;
	}

	public virtual float GetRegenPerTick()
	{
		return 0.5f;
	}

	public virtual bool CanBeAttacked()
	{
		return true;
	}

	protected virtual void Update()
	{
		//Clamp current health to max health
		//This is dirty but flexible
		currentHealth = Mathf.Min(currentHealth, GetMaxHealth());

		//Process all current targets
		ProcessTargets();
	}

	public virtual void DoTicks(int tickCount)
	{
		//Apply regen
		currentHealth = Mathf.Min(currentHealth + (tickCount * GetRegenPerTick() * BalanceManagement.overallBattlePace), GetMaxHealth()); 
	}

	protected void ToggleTarget(BattleBehaviour target, bool battleReset = true)
	{
		//Can't attack ourself
		if (Equals(target))
		{
			return;
		}

		if (Targeting(target, out int index))
		{
			//Remove
			currentTargets.RemoveAt(index);
			OnRemoveTargetRaw(target);
		}
		else
		{
			AddTarget(target);
		}
    }

	public void AddTarget(BattleBehaviour newTarget, bool battleReset = true, Target.TargetType type = Target.TargetType.Normal)
	{
		//Can't target something twice
		if (Targeting(newTarget, out int index))
		{
			//Switch target to maintained if it is currently normal and the new target is meant to be maintained
			if (currentTargets[index].type == Target.TargetType.Normal)
			{
				currentTargets[index].type = type;
			}
		}
		else
		{
			Target wrappedTarget = new Target();
			wrappedTarget.bb = newTarget;
			wrappedTarget.type = type;

			AddTargetInternal(wrappedTarget, battleReset);
		}
	}

	private void AddTargetInternal(Target target, bool battleReset)
	{
		//Can't attack ourself
		if (Equals(target.bb))
		{
			return;
		}

		if (battleReset)
		{
			BattleResetCheck();
		}

		OnAddTarget(target);
		currentTargets.Add(target);
	}

	private void BattleResetCheck()
	{
		if (currentTargets.Count == 0)
		{
			//No targets before this point
			foreach (WeaponProfile weapon in weapons)
			{
				weapon.OnBattleStart();
			}
		}
	}

	protected bool RemoveTarget(BattleBehaviour battleBehaviour)
	{
		if (Targeting(battleBehaviour, out int index))
		{
			currentTargets.RemoveAt(index);

			OnRemoveTargetRaw(battleBehaviour);

			return true;
		}

		return false;
	}

	protected bool RemoveTarget(Target target)
	{
		bool targetRemoved = RemoveTarget(target.bb);

		if (targetRemoved)
		{
			OnRemoveTarget(target);
		}

		return targetRemoved;
	}

	public void ClearNonMaintainedTargets()
	{
		OnClearTargets(currentTargets);

		for (int i = 0; i < currentTargets.Count;)
		{
			if (currentTargets[i].type == Target.TargetType.Maintained)
			{
				i++;
			}
			else
			{
				currentTargets.RemoveAt(i);
			}
		}
	}

	public void ClearTargets()
	{
		OnClearTargets(currentTargets);
		currentTargets.Clear();
	}

	//Insert hook methods
	protected virtual void OnAddTarget(Target newTarget)
	{
		//Do nothing by default
	}

	protected virtual void OnRemoveTargetRaw(BattleBehaviour target)
	{
		//Do nothing by default
	}

	protected virtual void OnRemoveTarget(Target target)
	{
		//Do nothing by default
	}

	protected virtual void OnClearTargets(List<Target> targetsBefore)
	{
		//Do nothing by default
	}
	//

	protected void ProcessTargets()
	{
		//Prune targets if their object is set inactive OR target is dead
		for (int i = 0; i < currentTargets.Count;)
		{
			if (currentTargets[i].bb.gameObject == null || !currentTargets[i].bb.gameObject.activeSelf || currentTargets[i].bb.Dead())
			{
				RemoveTarget(currentTargets[i]);
			}
			else
			{
				i++;
			}
		}
		//

		int count = currentTargets.Count;
		if(count <= 0)
		{
			//No targets
			return;
		}

		//Process weapons
		foreach (WeaponProfile profile in weapons)
		{
			//Lower time till next attack
			//It's important that we can lower the timer to zero and process the shot on the same frame
			profile.CountDownTimer();

			if (profile.CanFire())
			{
				//Try to attack a random target
				int randomOffset = Random.Range(0, count);

				//We want to iterate through all the targets until we find one within range
				//We use a randomized offset so there is no preference but we still find an in range target if one exists
				for (int i = 0; i < count; i++)
				{
					int index = (i + randomOffset) % count;
					Target target = currentTargets[index];

					Vector3 targetPosition = target.bb.GetPosition();

					//Within range!
					if (Vector3.Distance(GetPosition(), targetPosition) <= profile.GetRange())
					{
						//Preprocess
						PreTargetProcess(target);

						//Sometimes a large frame spike could occur on the can fire frame, so we allow for multiple attacks to happen
						int numberOfAttacks = profile.CaclculateNumberOfAttacks();

						for (int a = 0; a < numberOfAttacks; a++)
						{
							//Apply damage
							TakenDamageResult result = target.bb.TakeDamage(profile.GetDamage() * BalanceManagement.overallBattlePace, this);

							Vector3 shotTargetPosition = target.bb.GetTargetablePosition();
							//Draw Attack
							DrawAttack(shotTargetPosition, GetFireFromPosition(shotTargetPosition), profile);

							//Run on do damage
							OnDoDamage(result);
						}

						//Tell weapon to reset
						profile.ResetTimerValue();

						//Post process
						PostTargetProcess(target);

						//Don't want to process anymore targets
						break;
					}
				}

				//Post first iteration we want to clamp time till next to be above zero
				//This is because we only want to do an extra attack because of a frame spike
				//Not 30 seconds after a frame spike when the target finally comes into range
				profile.ClampTimerValue();
			}
		}
	}

	protected virtual void PreTargetProcess(Target target)
	{

	}

	protected virtual void PostTargetProcess(Target target)
	{

	}

	protected virtual void OnDoDamage(TakenDamageResult targetResult)
	{

	}

	protected virtual void DrawAttack(Vector3 targetPos, Vector3 firePos, WeaponProfile weaponProfile)
	{
		//Be default simply ask the battle management script to draw a line to the target positon from our fire position
		BattleManagement.CreateBasicBeamEffect(firePos, targetPos, 0.1f);
		//Alongside a basic explosion
		//(Disabled: 27/12/2024)
		//BattleManagement.CreateExplosion(targetPos, 0.6f);
    }

	protected virtual Vector3 GetTargetablePosition()
	{
		//Return a position within some range of the center transform
		//a.k.a a fuzzy position

		const float fuzziness = 0.75f;
		return transform.position + Random.onUnitSphere * fuzziness;
	}

	protected virtual Vector3 GetFireFromPosition(Vector3 targetPos)
	{
		return transform.position;
	}

	protected virtual Vector3 GetPosition()
	{
		return transform.position;
	}

	//DAMAGE

	public virtual bool Dead()
	{
		return currentHealth <= 0.0f;
	}

	public struct TakenDamageResult
	{
		public BattleBehaviour origin;
		public bool destroyed;
		public float damageTaken;
	}

	protected virtual TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		TakenDamageResult result = new TakenDamageResult();
		result.origin = origin;

		float finalDamageNumber = rawDamageNumber;
		//Don't allow attack to count as dealing more damage then we have health
		result.damageTaken = Mathf.Min(finalDamageNumber, currentHealth);

		currentHealth -= finalDamageNumber;

		if (Dead())
		{
			result.destroyed = true;
			OnDeath(result);
		}
		else
		{
			if (autoRetaliate)
			{
				AddTarget(origin, true, Target.TargetType.Maintained);
			}
		}

		return result;
	}

	protected virtual void OnDeath(TakenDamageResult killResult)
	{
		//Do nothing by default
	}

	//HELPER FUNCTIONS

	public Vector3 GetPointInsideCollider(BoxCollider collider)
	{
		Vector3 extents = collider.size / 2f;

		//Offset by collider center incase it isn't centered
		Vector3 point = new Vector3(
			Random.Range(-extents.x, extents.x),
			Random.Range(-extents.y, extents.y),
			Random.Range(-extents.z, extents.z)
		) + collider.center;

		return collider.transform.TransformPoint(point);
	}
}
