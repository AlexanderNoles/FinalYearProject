using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : InteractableBase
{ 
	public string targetsName = "Target";
	public Collider targetCollider;
	public bool autoFindTargets = true;
	public bool autoRetaliate = true;

	[HideInInspector]
	public new Transform transform;

	[HideInInspector]
	public List<WeaponProfile> weapons = new List<WeaponProfile>();
	public List<BattleBehaviour> currentTargets = new List<BattleBehaviour>();
	public List<BattleBehaviour> maintainedTargets = new List<BattleBehaviour>();

    protected float currentHealth;
	public BattleContextLink simulationLink;

    protected virtual void Awake()
	{
		transform = base.transform;
	}

	public virtual int TryGetEntityID()
	{
		if (simulationLink == null)
		{
			return -1;
		}

		return simulationLink.GetEntityID();
	}

	protected void OnEnable()
	{
		BattleManagement.RegisterBattleBehaviour(targetCollider, this);

		if (simulationLink != null )
		{
			currentHealth = simulationLink.GetMaxHealth();

			if (Dead())
			{
				OnDeath();
			}
		}
	}

	protected void OnDisable()
	{
		BattleManagement.DeRegisterBattleBehaviour(targetCollider);

		//Do full clear
		currentTargets.Clear();
		maintainedTargets.Clear();
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
		return 0.0f;
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
		currentHealth = Mathf.Min(currentHealth + (tickCount * GetRegenPerTick()), GetMaxHealth()); 
	}

	protected void ToggleTarget(BattleBehaviour target, bool battleReset = true)
	{
		//Can't attack ourself
		if (Equals(target))
		{
			return;
		}

        if (!RemoveTarget(target))
        {
            //If target is not removed (i.e., it was not in the list)
			AddTargetInternal(target, battleReset);
        }
    }

	public void AddTarget(BattleBehaviour newTarget, bool battleReset = true)
	{
		//Can't attack ourself
		if (Equals(newTarget))
		{
			return;
		}

		//Can't target something twice
		if (currentTargets.Contains(newTarget))
		{
			return;
		}

		AddTargetInternal(newTarget, battleReset);
	}

	public void AddMaintainedTarget(BattleBehaviour newTarget, bool battleReset = true)
	{
		//Can't attack ourself
		if (Equals(newTarget))
		{
			return;
		}

		//Can't target something twice
		if (maintainedTargets.Contains(newTarget))
		{
			return;
		}

		AddMaintainedTargetInternal(newTarget, battleReset);
	}

	private void AddTargetInternal(BattleBehaviour target, bool battleReset)
	{
		if (battleReset)
		{
			BattleResetCheck();
		}

		OnAddTarget(target);
		currentTargets.Add(target);
	}

	private void AddMaintainedTargetInternal(BattleBehaviour target, bool battleReset)
	{
		if (battleReset)
		{
			BattleResetCheck();
		}

		OnAddTarget(target);
		maintainedTargets.Add(target);
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

	protected bool RemoveTarget(BattleBehaviour target)
	{
		bool targetRemoved = currentTargets.Remove(target);

		if (targetRemoved)
		{
			OnRemoveTarget(target);
		}

		return targetRemoved;
	}

	public void ClearTargets()
	{
		OnClearTargets(currentTargets);
		currentTargets.Clear();
	}

	//Insert hook methods
	protected virtual void OnAddTarget(BattleBehaviour newTarget)
	{
		//Do nothing by default
	}

	protected virtual void OnRemoveTarget(BattleBehaviour target)
	{
		//Do nothing by default
	}

	protected virtual void OnClearTargets(List<BattleBehaviour> targetsBefore)
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
		int count = currentTargets.Count + maintainedTargets.Count;
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
		ProcessTargetList(maintainedTargets, attackProfiles, count);
	}

	private void ProcessTargetList(List<BattleBehaviour> targets, List<AttackProfile> attackProfiles, int fullCount)
	{
		//Apply damage to targets
		//Apply some random offset to the index so if too few attacks are applied on one frame
		//They don't always keep going to the first index, a.k.a single target fire
		int randomOffset = Random.Range(0, targets.Count);

		for (int i = 0; i < targets.Count;)
		{
			//Use current count instead of the count cached at the start of these function incase element is removed
			int index = (i + randomOffset) % targets.Count;
			BattleBehaviour target = targets[index];

			if (target.Dead())
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

			Vector3 targetPosition = target.GetPosition();

			//Preprocess
			PreTargetProcess(target);

			//Apply attacks to target
			//For each weapon we apply this targets share of the attacks rounded up
			for (int a = 0; a < attackProfiles.Count; a++)
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
						target.TakeDamage(attack.parentProfile.GetDamage(), this);

						Vector3 shotTargetPosition = target.GetTargetablePosition();
						//Draw Attack
						DrawAttack(shotTargetPosition, GetFireFromPosition(shotTargetPosition), weapons[a]);

						//Reduce attack cap
						attackCap--;
						//Increment number of attacks so far
						attack.numberOfAttacksSoFar++;
					}
				}
			}

			//Postprocess
			PostTargetProcess(target);
		}
	}

	protected virtual void PreTargetProcess(BattleBehaviour target)
	{

	}

	protected virtual void PostTargetProcess(BattleBehaviour target)
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
		public bool destroyed;
		public float damageTaken;
	}

	protected virtual TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		TakenDamageResult result = new TakenDamageResult();

		float finalDamageNumber = rawDamageNumber;
		result.damageTaken = finalDamageNumber;

		currentHealth -= finalDamageNumber;

		if (Dead())
		{
			result.destroyed = true;
			OnDeath();
		}
		else
		{
			if (autoRetaliate)
			{
				AddMaintainedTarget(origin);
			}
		}

		return result;
	}

	protected virtual void OnDeath()
	{
		if (simulationLink != null)
		{
			simulationLink.OnDeath();
		}
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
