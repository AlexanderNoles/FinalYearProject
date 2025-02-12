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

	private class AttackProfile 
	{
		public WeaponProfile parentProfile;

		public int totalNumberOfAttacks;
		public int numberOfAttacksSoFar;

		public AttackProfile(int total, WeaponProfile weapon)
		{
			totalNumberOfAttacks = total;
			numberOfAttacksSoFar = 0;

			parentProfile = weapon;
		}
	}

	protected void ProcessTargets()
	{
		//Prune targets if their object is set inactive
		for (int i = 0; i < currentTargets.Count;)
		{
			if (currentTargets[i].bb.gameObject == null || !currentTargets[i].bb.gameObject.activeSelf)
			{
				currentTargets.RemoveAt(i);
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

		//Process attack pool so we know how many attacks we have to divide up between targets
		List<AttackProfile> attackProfiles = new List<AttackProfile>();

		foreach (WeaponProfile weaponProfile in weapons)
		{
			int attackCount = weaponProfile.CaclculateNumberOfAttacks();

			if (attackCount > 0)
			{
				attackProfiles.Add(new AttackProfile(attackCount, weaponProfile));
			}
		}

		ProcessTargetList(currentTargets, attackProfiles, count);
	}

	private void ProcessTargetList(List<Target> targets, List<AttackProfile> attackProfiles, int fullCount)
	{
		//Apply damage to targets
		//Apply some random offset to the index so if too few attacks are applied on one frame
		//They don't always keep going to the first index, a.k.a single target fire
		int randomOffset = Random.Range(0, targets.Count);

		for (int i = 0; i < targets.Count;)
		{
			//Use current count instead of the count cached at the start of these function incase element is removed
			int index = (i + randomOffset) % targets.Count;
			Target target = targets[index];

			if (target.bb.Dead())
			{
				if (targets.Remove(target))
				{
					OnRemoveTarget(target);
				}
				continue;
			}
			else
			{
				i++;
			}

			if (!target.bb.CanBeAttacked())
			{
				//Not allowed to attack this target
				continue;
			}

			Vector3 targetPosition = target.bb.GetPosition();

			bool targetKilled = false;
			//Preprocess
			PreTargetProcess(target);

			//Apply attacks to target
			//For each weapon we apply this targets share of the attacks rounded up
			for (int a = 0; a < attackProfiles.Count && !targetKilled; a++)
			{
				AttackProfile attack = attackProfiles[a];

				//Check if within range
				//If so then mark this weapon as used this frame
				if (Vector3.Distance(GetPosition(), targetPosition) <= attack.parentProfile.GetRange())
				{
					attack.parentProfile.MarkLastAttackTime(Time.time);

					//Number of attacks from this weapon for this target
					//Rounded up so we always use all our attacks, even if some targets don't get hit
					int attackCap = Mathf.CeilToInt(attack.totalNumberOfAttacks / (float)fullCount);
					while (attack.numberOfAttacksSoFar < attack.totalNumberOfAttacks && attackCap > 0)
					{
						//Apply damage
						TakenDamageResult result = target.bb.TakeDamage(attack.parentProfile.GetDamage() * BalanceManagement.overallBattlePace, this);

						Vector3 shotTargetPosition = target.bb.GetTargetablePosition();
						//Draw Attack
						DrawAttack(shotTargetPosition, GetFireFromPosition(shotTargetPosition), weapons[a]);

						//Reduce attack cap
						attackCap--;
						//Increment number of attacks so far
						attack.numberOfAttacksSoFar++;

						//Killed target
						if (result.destroyed)
						{
							targetKilled = true;
							break;
						}
					}
				}
			}

			//Postprocess
			PostTargetProcess(target);
		}
	}

	protected virtual void PreTargetProcess(Target target)
	{

	}

	protected virtual void PostTargetProcess(Target target)
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
